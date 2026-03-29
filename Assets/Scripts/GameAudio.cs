using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GameAudio : MonoBehaviour
{
    public static GameAudio Instance { get; private set; }

    private const float MusicVolume = 0.32f;

    private AudioSource audioSource;
    private AudioSource musicSource;
    private AudioClip switchClip;
    private AudioClip gameOverClip;
    private AudioClip restartClip;
    private AudioClip pickupClip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 0.9f;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.volume = MusicVolume;

        switchClip = CreateToneClip("Switch", 780f, 1120f, 0.055f, 0.12f);
        gameOverClip = CreateToneClip("GameOver", 360f, 170f, 0.16f, 0.18f);
        restartClip = CreateToneClip("Restart", 420f, 760f, 0.08f, 0.12f);
        pickupClip = CreateToneClip("Pickup", 920f, 1380f, 0.06f, 0.1f);

        StartBackgroundMusic();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public static void PlaySwitch()
    {
        if (Instance != null && Instance.switchClip != null)
        {
            Instance.audioSource.PlayOneShot(Instance.switchClip, 0.75f);
        }
    }

    public static void PlayGameOver()
    {
        if (Instance != null && Instance.gameOverClip != null)
        {
            Instance.audioSource.PlayOneShot(Instance.gameOverClip, 0.95f);
        }
    }

    public static void PlayPickup()
    {
        if (Instance != null && Instance.pickupClip != null)
        {
            Instance.audioSource.PlayOneShot(Instance.pickupClip, 0.7f);
        }
    }

    public static void PlayRestart()
    {
        if (Instance == null || Instance.restartClip == null)
        {
            return;
        }

        Vector3 position = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
        AudioSource.PlayClipAtPoint(Instance.restartClip, position, 0.8f);
    }

    private void StartBackgroundMusic()
    {
        if (musicSource == null || musicSource.isPlaying)
        {
            return;
        }

        AudioClip preferredClip = Resources.Load<AudioClip>("Audio/Music/music_2");
        if (preferredClip != null)
        {
            musicSource.clip = preferredClip;
            musicSource.Play();
            return;
        }

        AudioClip[] musicClips = Resources.LoadAll<AudioClip>("Audio/Music");
        if (musicClips == null || musicClips.Length == 0)
        {
            return;
        }

        musicSource.clip = musicClips[0];
        musicSource.Play();
    }

    private AudioClip CreateToneClip(string clipName, float startFrequency, float endFrequency, float duration, float amplitude)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];
        float phase = 0f;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = sampleCount <= 1 ? 0f : (float)i / (sampleCount - 1);
            float frequency = Mathf.Lerp(startFrequency, endFrequency, t);
            phase += 2f * Mathf.PI * frequency / sampleRate;

            float fadeIn = Mathf.Clamp01(t / 0.08f);
            float fadeOut = Mathf.Clamp01((1f - t) / 0.22f);
            float envelope = fadeIn * fadeOut;
            float harmonic = Mathf.Sin(phase * 2f) * 0.15f;
            samples[i] = (Mathf.Sin(phase) + harmonic) * amplitude * envelope;
        }

        AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}