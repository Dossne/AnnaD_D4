using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class PrototypeSceneBootstrap
{
    private const float LeftWallX = -2.35f;
    private const float RightWallX = 2.35f;
    private const string RuntimeRootName = "PrototypeRuntime";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void BuildPrototypeOnLoad()
    {
        RebuildPrototype();
    }

    public static void RestartPrototype()
    {
        RebuildPrototype();
    }

    private static void RebuildPrototype()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid())
        {
            return;
        }

        Camera camera = Camera.main;
        if (camera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            camera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
            camera.orthographic = true;
        }

        ClearExistingPrototype(camera);
        SetupCamera(camera);

        Sprite baseSprite = CreateSolidSprite();
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject root = new GameObject(RuntimeRootName);
        CreateParallaxBackdrop(camera);
        CreateWalls(camera, baseSprite);

        GameObject player = CreatePlayer(root.transform, baseSprite);
        GameObject obstacleTemplate = CreateObstacleTemplate(root.transform, baseSprite);
        CreateManagers(root.transform, camera, player, obstacleTemplate, font, baseSprite);
    }

    private static void ClearExistingPrototype(Camera camera)
    {
        GameObject existingRoot = GameObject.Find(RuntimeRootName);
        if (existingRoot != null)
        {
            SafeDestroy(existingRoot);
        }

        for (int i = camera.transform.childCount - 1; i >= 0; i--)
        {
            SafeDestroy(camera.transform.GetChild(i).gameObject);
        }
    }

    private static void SafeDestroy(Object target)
    {
        if (target == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Object.Destroy(target);
        }
        else
        {
            Object.DestroyImmediate(target);
        }
    }

    private static void SetupCamera(Camera camera)
    {
        camera.orthographic = true;
        camera.orthographicSize = 8.8f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.05f, 0.05f, 0.1f, 1f);
        camera.transform.position = new Vector3(0f, 0f, -10f);

        CameraFollow follow = camera.GetComponent<CameraFollow>();
        if (follow == null)
        {
            follow = camera.gameObject.AddComponent<CameraFollow>();
        }
    }

    private static void CreateParallaxBackdrop(Camera camera)
    {
        float visibleHeight = camera.orthographicSize * 2f;
        float visibleWidth = visibleHeight * camera.aspect;
        float layerWidth = visibleWidth + 2f;
        float layerHeight = visibleHeight + 2f;

        Texture2D farTexture = LoadSpaceBackgroundTexture();
        Texture2D midStars = CreateStarTexture(256, 512, 90, 1, 0.25f, 0.7f);
        Texture2D nearStars = CreateStarTexture(256, 512, 170, 2, 0.45f, 1f);

        Vector3 farLayerScale = GetAspectPreservingScale(farTexture, visibleWidth, visibleHeight, 2f);

        CreateParallaxQuad("FarBackground", camera, farTexture, new Color(0.82f, 0.86f, 1f, 0.9f), new Vector3(0f, 0f, 26f), farLayerScale, new Vector2(0.5f, 0.5f), 0.00025f, 0f, 0.0003f, 0f);
        CreateParallaxQuad("MidStars", camera, midStars, new Color(0.72f, 0.84f, 1f, 0.55f), new Vector3(0f, 0f, 22f), new Vector3(layerWidth, layerHeight, 1f), new Vector2(1.2f, 2f), 0.008f, 0.0015f, 0.006f, 0f);
        CreateParallaxQuad("NearStars", camera, nearStars, new Color(0.95f, 0.98f, 1f, 0.85f), new Vector3(0f, 0f, 20f), new Vector3(layerWidth, layerHeight, 1f), new Vector2(1.5f, 2.6f), 0.02f, 0.003f, 0.012f, 0f);

        CreateSpaceParticles(camera, visibleWidth, visibleHeight);
    }

    private static Vector3 GetAspectPreservingScale(Texture2D texture, float visibleWidth, float visibleHeight, float padding)
    {
        float width = visibleWidth + padding;
        float height = visibleHeight + padding;

        if (texture == null || texture.width <= 0 || texture.height <= 0)
        {
            return new Vector3(width, height, 1f);
        }

        float screenAspect = width / height;
        float textureAspect = (float)texture.width / texture.height;

        if (textureAspect > screenAspect)
        {
            width = height * textureAspect;
        }
        else
        {
            height = width / textureAspect;
        }

        return new Vector3(width, height, 1f);
    }

    private static void CreateSpaceParticles(Camera camera, float visibleWidth, float visibleHeight)
    {
        GameObject particlesObject = new GameObject("SpaceParticles");
        particlesObject.transform.SetParent(camera.transform);
        particlesObject.transform.localPosition = new Vector3(0f, 0f, 18f);

        ParticleSystem particles = particlesObject.AddComponent<ParticleSystem>();
        ParticleSystemRenderer renderer = particlesObject.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortMode = ParticleSystemSortMode.Distance;
        renderer.minParticleSize = 0.004f;
        renderer.maxParticleSize = 0.014f;

        var main = particles.main;
        main.playOnAwake = true;
        main.loop = true;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.startLifetime = 4f;
        main.startSpeed = 0.2f;
        main.startSize = 0.013f;
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.7f, 0.85f, 1f, 0.08f), new Color(1f, 1f, 1f, 0.28f));
        main.maxParticles = 1;

        var emission = particles.emission;
        emission.rateOverTime = 0.1f;

        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(visibleWidth + 2f, visibleHeight + 2f, 0.1f);

        var velocityOverLifetime = particles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(0f);
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(-0.16f);
        velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(0f);

        var noise = particles.noise;
        noise.enabled = true;
        noise.strength = 0.05f;
        noise.frequency = 0.2f;

        particles.Play();
    }

    private static void CreateWalls(Camera camera, Sprite sprite)
    {
        float visibleHeight = camera.orthographicSize * 2f;
        float wallHeight = visibleHeight + 2f;
        float wallZ = 14f;

        CreateWallStripe(camera.transform, sprite, "LeftWall", LeftWallX - 0.75f, wallHeight, wallZ);
        CreateWallStripe(camera.transform, sprite, "RightWall", RightWallX + 0.75f, wallHeight, wallZ);
    }

    private static void CreateWallStripe(Transform parent, Sprite sprite, string name, float x, float height, float z)
    {
        GameObject glow = CreateSpriteObject(name + "Glow", parent, sprite, new Color(0.7f, 0.8f, 1f, 0.28f), new Vector3(0.5f, height, 1f), new Vector3(x, 0f, z + 0.2f));
        glow.GetComponent<SpriteRenderer>().sortingOrder = 8;

        GameObject core = CreateSpriteObject(name, parent, sprite, new Color(0.96f, 0.97f, 1f, 1f), new Vector3(0.16f, height, 1f), new Vector3(x, 0f, z));
        core.GetComponent<SpriteRenderer>().sortingOrder = 9;
    }

    private static GameObject CreatePlayer(Transform root, Sprite sprite)
    {
        GameObject player = new GameObject("Player");
        player.transform.SetParent(root);
        player.transform.position = new Vector3(LeftWallX, -4f, 0f);

        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        BoxCollider2D collider = player.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.7f, 0.7f);

        GameObject body = CreateSpriteObject("Body", player.transform, sprite, new Color(0.2f, 0.9f, 1f, 1f), new Vector3(0.7f, 0.7f, 1f), Vector3.zero);
        body.GetComponent<SpriteRenderer>().sortingOrder = 2;

        PlayerController controller = player.AddComponent<PlayerController>();
        controller.Configure(4.5f, 34f, LeftWallX, RightWallX, true, body.transform, 6.8f, 10.5f, 0.08f, 0.18f);

        return player;
    }

    private static GameObject CreateObstacleTemplate(Transform root, Sprite sprite)
    {
        GameObject obstacle = new GameObject("ObstacleTemplate");
        obstacle.transform.SetParent(root);
        obstacle.SetActive(false);

        BoxCollider2D collider = obstacle.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.9f, 0.9f);

        obstacle.AddComponent<ObstacleMarker>();

        GameObject body = CreateSpriteObject("Body", obstacle.transform, sprite, new Color(1f, 0.35f, 0.35f, 1f), new Vector3(0.9f, 0.9f, 1f), Vector3.zero);
        body.GetComponent<SpriteRenderer>().sortingOrder = 2;

        return obstacle;
    }

    private static void CreateManagers(Transform root, Camera camera, GameObject player, GameObject obstacleTemplate, Font font, Sprite sprite)
    {
        GameObject gameManagerObject = new GameObject("GameManager");
        gameManagerObject.transform.SetParent(root);
        ScoreManager scoreManager = gameManagerObject.AddComponent<ScoreManager>();

        GameObject spawnerObject = new GameObject("ObstacleSpawner");
        spawnerObject.transform.SetParent(root);
        ObstacleSpawner spawner = spawnerObject.AddComponent<ObstacleSpawner>();
        spawner.Configure(obstacleTemplate, player.transform, LeftWallX, RightWallX, 10f, 3f, 0.7f, 36);

        CameraFollow follow = camera.GetComponent<CameraFollow>();
        follow.Configure(player.transform, 3.5f);

        CreateCanvas(root, font, out Text scoreText, out GameObject gameOverPanel, out Text gameOverText, out Text gameOverScoreText, out Image flashOverlay);
        scoreManager.Configure(player.GetComponent<PlayerController>(), scoreText, gameOverPanel, gameOverText, gameOverScoreText, flashOverlay);
    }

    private static void CreateCanvas(Transform root, Font font, out Text scoreText, out GameObject gameOverPanel, out Text gameOverText, out Text gameOverScoreText, out Image flashOverlay)
    {
        GameObject canvasObject = new GameObject("Canvas");
        canvasObject.transform.SetParent(root);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1f;

        canvasObject.AddComponent<GraphicRaycaster>();

        EnsureEventSystem();

        flashOverlay = CreateFullscreenImage(canvas.transform, "FlashOverlay", new Color(1f, 1f, 1f, 0f));
        flashOverlay.raycastTarget = false;

        scoreText = CreateText(canvas.transform, font, "ScoreText", "Score: 0", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -80f), 42, Color.white);

        gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(canvas.transform);
        Image panelImage = gameOverPanel.AddComponent<Image>();
        panelImage.color = new Color(0.02f, 0.04f, 0.1f, 0.82f);

        RectTransform panelRect = gameOverPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(760f, 380f);
        panelRect.anchoredPosition = new Vector2(0f, 120f);

        gameOverText = CreateText(gameOverPanel.transform, font, "GameOverText", "GAME OVER", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -90f), 86, Color.white);
        gameOverText.alignment = TextAnchor.MiddleCenter;
        gameOverText.fontStyle = FontStyle.Bold;

        gameOverScoreText = CreateText(gameOverPanel.transform, font, "GameOverScoreText", "Score: 0", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -180f), 44, Color.white);
        gameOverScoreText.alignment = TextAnchor.MiddleCenter;

        Text hintText = CreateText(gameOverPanel.transform, font, "HintText", "Tap anywhere to restart", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -255f), 34, new Color(0.8f, 0.9f, 1f, 1f));
        hintText.alignment = TextAnchor.MiddleCenter;

        gameOverPanel.SetActive(false);
    }

    private static void EnsureEventSystem()
    {
        EventSystem existingEventSystem = Object.FindObjectOfType<EventSystem>();
        if (existingEventSystem != null)
        {
            if (existingEventSystem.GetComponent<StandaloneInputModule>() == null)
            {
                existingEventSystem.gameObject.AddComponent<StandaloneInputModule>();
            }

            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }

    private static Image CreateFullscreenImage(Transform parent, string name, Color color)
    {
        GameObject imageObject = new GameObject(name);
        imageObject.transform.SetParent(parent);

        RectTransform rect = imageObject.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = imageObject.AddComponent<Image>();
        image.color = color;
        return image;
    }

    private static Text CreateText(Transform parent, Font font, string name, string content, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, int fontSize, Color color)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent);

        RectTransform rect = textObject.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(900f, 140f);
        rect.anchoredPosition = anchoredPosition;

        Text text = textObject.AddComponent<Text>();
        text.font = font;
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;

        return text;
    }

    private static GameObject CreateParallaxQuad(string name, Camera camera, Texture2D texture, Color tint, Vector3 localPosition, Vector3 localScale, Vector2 textureScale, float yScrollFactor, float xScrollFactor, float autoScrollY, float autoScrollX)
    {
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = name;
        quad.transform.SetParent(camera.transform);
        quad.transform.localPosition = localPosition;
        quad.transform.localScale = localScale;

        Object.Destroy(quad.GetComponent<Collider>());

        MeshRenderer renderer = quad.GetComponent<MeshRenderer>();
        renderer.material = new Material(Shader.Find("Unlit/Transparent"));
        renderer.material.mainTexture = texture;
        renderer.material.mainTextureScale = textureScale;
        renderer.material.mainTextureOffset = Vector2.zero;
        renderer.material.color = tint;

        ParallaxMaterialScroller scroller = quad.AddComponent<ParallaxMaterialScroller>();
        scroller.Configure(camera.transform, yScrollFactor, xScrollFactor, autoScrollY, autoScrollX);

        return quad;
    }

    private static GameObject CreateSpriteObject(string name, Transform parent, Sprite sprite, Color color, Vector3 scale, Vector3 localPosition)
    {
        GameObject gameObject = new GameObject(name);
        gameObject.transform.SetParent(parent);
        gameObject.transform.localPosition = localPosition;
        gameObject.transform.localScale = scale;

        SpriteRenderer renderer = gameObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;

        return gameObject;
    }

    private static Texture2D LoadSpaceBackgroundTexture()
    {
        string texturePath = Path.Combine(Application.dataPath, "Art", "SpaceBackground.png");
        if (!File.Exists(texturePath))
        {
            return CreateStarTexture(256, 512, 120, 1, 0.2f, 0.8f);
        }

        byte[] fileBytes = File.ReadAllBytes(texturePath);
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        texture.LoadImage(fileBytes, false);
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Bilinear;
        return texture;
    }

    private static Texture2D CreateStarTexture(int width, int height, int starCount, int starSize, float minAlpha, float maxAlpha)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color clear = new Color(0f, 0f, 0f, 0f);
        Color[] clearPixels = new Color[width * height];
        for (int i = 0; i < clearPixels.Length; i++)
        {
            clearPixels[i] = clear;
        }

        texture.SetPixels(clearPixels);

        for (int i = 0; i < starCount; i++)
        {
            int x = Random.Range(0, width - starSize);
            int y = Random.Range(0, height - starSize);
            float alpha = Random.Range(minAlpha, maxAlpha);
            Color starColor = new Color(1f, 1f, 1f, alpha);

            for (int px = 0; px < starSize; px++)
            {
                for (int py = 0; py < starSize; py++)
                {
                    texture.SetPixel(x + px, y + py, starColor);
                }
            }
        }

        texture.Apply();
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Point;
        return texture;
    }

    private static Sprite CreateSolidSprite()
    {
        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Point;

        return Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
    }
}
