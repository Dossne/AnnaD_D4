using System.Collections;
using UnityEngine;

public class EnergyOrb : MonoBehaviour
{
    [SerializeField] private int scoreValue = 5;

    private bool isCollected;
    private Transform rootTransform;
    private Vector3 baseScale;
    private Transform burstTransform;

    private void OnEnable()
    {
        rootTransform = transform.parent != null ? transform.parent : transform;
        baseScale = rootTransform.localScale;
        burstTransform = rootTransform.Find("Burst");
        isCollected = false;
    }

    private void Update()
    {
        if (rootTransform == null)
        {
            return;
        }

        if (!isCollected)
        {
            float pulse = 1f + Mathf.Sin(Time.time * 4.1f) * 0.06f;
            rootTransform.localScale = baseScale * pulse;
        }

        if (burstTransform != null)
        {
            burstTransform.localRotation = Quaternion.Euler(0f, 0f, Time.time * 34f);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected)
        {
            return;
        }

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null || !player.IsAlive)
        {
            return;
        }

        isCollected = true;
        ScoreManager.Instance?.CollectOrb(scoreValue);
        GameAudio.PlayPickup();
        StartCoroutine(CollectRoutine());
    }

    private IEnumerator CollectRoutine()
    {
        Transform root = rootTransform != null ? rootTransform : (transform.parent != null ? transform.parent : transform);
        Collider2D trigger = GetComponent<Collider2D>();
        if (trigger != null)
        {
            trigger.enabled = false;
        }

        SpriteRenderer[] renderers = root.GetComponentsInChildren<SpriteRenderer>(true);
        Vector3 startScale = root.localScale;
        float scaleDuration = 0.06f;
        float scaleElapsed = 0f;

        while (scaleElapsed < scaleDuration)
        {
            scaleElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(scaleElapsed / scaleDuration);
            root.localScale = Vector3.Lerp(startScale, baseScale * 1.28f, t);
            yield return null;
        }

        CreateBurst(root.position, renderers);

        float fadeDuration = 0.08f;
        float fadeElapsed = 0f;
        Color[] baseColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            baseColors[i] = renderers[i].color;
        }

        while (fadeElapsed < fadeDuration)
        {
            fadeElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(fadeElapsed / fadeDuration);
            root.localScale = Vector3.Lerp(baseScale * 1.28f, baseScale * 0.6f, t);

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null)
                {
                    continue;
                }

                Color color = baseColors[i];
                color.a *= 1f - t;
                renderers[i].color = color;
            }

            yield return null;
        }

        Destroy(root.gameObject);
    }

    private void CreateBurst(Vector3 position, SpriteRenderer[] sourceRenderers)
    {
        if (sourceRenderers == null || sourceRenderers.Length == 0)
        {
            return;
        }

        Sprite sprite = sourceRenderers[sourceRenderers.Length - 1].sprite;
        if (sprite == null)
        {
            return;
        }

        GameObject burstObject = new GameObject("OrbPickupBurst");
        burstObject.transform.position = position;

        ParticleSystem burst = burstObject.AddComponent<ParticleSystem>();
        burst.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ParticleSystemRenderer renderer = burstObject.GetComponent<ParticleSystemRenderer>();
        Material material = new Material(Shader.Find("Sprites/Default"));
        material.mainTexture = sprite.texture;
        renderer.material = material;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortingOrder = 6;

        var main = burst.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = 0.2f;
        main.startLifetime = 0.16f;
        main.startSpeed = 1.8f;
        main.startSize = 0.11f;
        main.startColor = new Color(0.72f, 1f, 1f, 0.9f);
        main.maxParticles = 10;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = burst.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 8) });

        var shape = burst.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.05f;

        var colorOverLifetime = burst.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.8f, 1f, 1f), 0f),
                new GradientColorKey(new Color(0.35f, 0.9f, 1f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(0.85f, 0f),
                new GradientAlphaKey(0f, 1f)
            });
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

        var sizeOverLifetime = burst.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(1f, 0.2f));
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        burst.Play();
        Destroy(burstObject, 0.35f);
    }
}
