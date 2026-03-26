using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class PrototypeSceneBootstrap
{
    private const float LeftWallX = -2.35f;
    private const float RightWallX = 2.35f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void BuildPrototype()
    {
        if (Object.FindObjectOfType<PlayerController>() != null)
        {
            return;
        }

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

        SetupCamera(camera);

        Sprite baseSprite = CreateSolidSprite();
        Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        GameObject root = new GameObject("PrototypeRuntime");
        CreateBackdrop(root.transform, camera.transform, baseSprite);
        CreateWalls(root.transform, baseSprite);

        GameObject player = CreatePlayer(root.transform, baseSprite);
        GameObject obstacleTemplate = CreateObstacleTemplate(root.transform, baseSprite);
        CreateManagers(root.transform, camera, player, obstacleTemplate, font);
    }

    private static void SetupCamera(Camera camera)
    {
        camera.orthographic = true;
        camera.orthographicSize = 8.8f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.03f, 0.02f, 0.12f, 1f);
        camera.transform.position = new Vector3(0f, 0f, -10f);

        CameraFollow follow = camera.GetComponent<CameraFollow>();
        if (follow == null)
        {
            follow = camera.gameObject.AddComponent<CameraFollow>();
        }
    }

    private static void CreateBackdrop(Transform root, Transform cameraTransform, Sprite sprite)
    {
        GameObject backdrop = new GameObject("Backdrop");
        backdrop.transform.SetParent(root);

        GameObject deepSpace = CreateSpriteObject("DeepSpace", backdrop.transform, sprite, new Color(0.05f, 0.04f, 0.16f, 1f), new Vector3(8f, 500f, 1f), Vector3.zero);
        deepSpace.transform.position = new Vector3(0f, 250f, 15f);

        GameObject centerGlow = CreateSpriteObject("CenterGlow", backdrop.transform, sprite, new Color(0.1f, 0.22f, 0.45f, 0.18f), new Vector3(3.6f, 500f, 1f), Vector3.zero);
        centerGlow.transform.position = new Vector3(0f, 250f, 14f);

        GameObject leftGlow = CreateSpriteObject("LeftGlow", backdrop.transform, sprite, new Color(0.9f, 0.15f, 1f, 0.14f), new Vector3(2.5f, 500f, 1f), Vector3.zero);
        leftGlow.transform.position = new Vector3(-4.6f, 250f, 14f);

        GameObject rightGlow = CreateSpriteObject("RightGlow", backdrop.transform, sprite, new Color(0.9f, 0.15f, 1f, 0.14f), new Vector3(2.5f, 500f, 1f), Vector3.zero);
        rightGlow.transform.position = new Vector3(4.6f, 250f, 14f);

        GameObject starRoot = new GameObject("Stars");
        starRoot.transform.SetParent(cameraTransform);
        starRoot.transform.localPosition = new Vector3(0f, 0f, 12f);

        for (int i = 0; i < 90; i++)
        {
            float x = Random.Range(-3.2f, 3.2f);
            float y = Random.Range(-9f, 9f);
            float size = Random.Range(0.035f, 0.09f);
            Color color = Color.Lerp(new Color(0.3f, 0.7f, 1f, 0.5f), new Color(1f, 1f, 1f, 0.9f), Random.value);
            CreateSpriteObject("Star", starRoot.transform, sprite, color, new Vector3(size, size, 1f), new Vector3(x, y, 0f));
        }
    }

    private static void CreateWalls(Transform root, Sprite sprite)
    {
        CreateNeonWall(root, sprite, "LeftWall", LeftWallX - 0.75f, new Color(0.72f, 0.3f, 1f, 0.95f), new Color(0.85f, 0.4f, 1f, 0.25f));
        CreateNeonWall(root, sprite, "RightWall", RightWallX + 0.75f, new Color(1f, 0.36f, 0.76f, 0.95f), new Color(1f, 0.25f, 0.8f, 0.25f));
    }

    private static void CreateNeonWall(Transform root, Sprite sprite, string name, float x, Color coreColor, Color glowColor)
    {
        GameObject glow = CreateSpriteObject(name + "Glow", root, sprite, glowColor, new Vector3(0.48f, 500f, 1f), new Vector3(x, 250f, 0f));
        glow.GetComponent<SpriteRenderer>().sortingOrder = -4;

        GameObject core = CreateSpriteObject(name, root, sprite, coreColor, new Vector3(0.12f, 500f, 1f), new Vector3(x, 250f, 0f));
        core.GetComponent<SpriteRenderer>().sortingOrder = -3;
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
        collider.size = new Vector2(0.72f, 0.72f);

        GameObject glow = CreateSpriteObject("Glow", player.transform, sprite, new Color(0.2f, 1f, 0.95f, 0.22f), new Vector3(1.2f, 1.2f, 1f), Vector3.zero);
        glow.GetComponent<SpriteRenderer>().sortingOrder = 3;

        GameObject outline = new GameObject("Outline");
        outline.transform.SetParent(player.transform);
        outline.transform.localPosition = Vector3.zero;
        LineRenderer square = outline.AddComponent<LineRenderer>();
        square.useWorldSpace = false;
        square.loop = true;
        square.positionCount = 4;
        square.widthMultiplier = 0.08f;
        square.material = new Material(Shader.Find("Sprites/Default"));
        square.startColor = new Color(0.35f, 1f, 0.95f, 1f);
        square.endColor = new Color(0.35f, 1f, 0.95f, 1f);
        square.sortingOrder = 4;
        square.SetPositions(new[]
        {
            new Vector3(-0.34f, -0.34f, 0f),
            new Vector3(-0.34f, 0.34f, 0f),
            new Vector3(0.34f, 0.34f, 0f),
            new Vector3(0.34f, -0.34f, 0f)
        });

        TrailRenderer trail = player.AddComponent<TrailRenderer>();
        trail.time = 0.35f;
        trail.startWidth = 0.12f;
        trail.endWidth = 0f;
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.startColor = new Color(0.35f, 1f, 0.95f, 0.75f);
        trail.endColor = new Color(0.35f, 1f, 0.95f, 0f);
        trail.sortingOrder = 2;

        PlayerController controller = player.AddComponent<PlayerController>();
        controller.Configure(5f, 18f, LeftWallX, RightWallX, true, outline.transform);

        return player;
    }

    private static GameObject CreateObstacleTemplate(Transform root, Sprite sprite)
    {
        GameObject obstacle = new GameObject("ObstacleTemplate");
        obstacle.transform.SetParent(root);
        obstacle.SetActive(false);

        BoxCollider2D collider = obstacle.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.95f, 0.95f);

        obstacle.AddComponent<ObstacleMarker>();

        LineRenderer triangle = obstacle.AddComponent<LineRenderer>();
        triangle.useWorldSpace = false;
        triangle.loop = true;
        triangle.positionCount = 3;
        triangle.widthMultiplier = 0.08f;
        triangle.material = new Material(Shader.Find("Sprites/Default"));
        triangle.startColor = new Color(1f, 0.3f, 0.8f, 1f);
        triangle.endColor = new Color(1f, 0.3f, 0.8f, 1f);
        triangle.sortingOrder = 4;
        triangle.SetPositions(new[]
        {
            new Vector3(0f, 0.46f, 0f),
            new Vector3(0.78f, 0f, 0f),
            new Vector3(0f, -0.46f, 0f)
        });

        GameObject glow = CreateSpriteObject("Glow", obstacle.transform, sprite, new Color(1f, 0.2f, 0.8f, 0.18f), new Vector3(1.15f, 1.15f, 1f), new Vector3(0.18f, 0f, 0f));
        glow.GetComponent<SpriteRenderer>().sortingOrder = 3;

        return obstacle;
    }

    private static void CreateManagers(Transform root, Camera camera, GameObject player, GameObject obstacleTemplate, Font font)
    {
        GameObject gameManagerObject = new GameObject("GameManager");
        gameManagerObject.transform.SetParent(root);
        ScoreManager scoreManager = gameManagerObject.AddComponent<ScoreManager>();

        GameObject spawnerObject = new GameObject("ObstacleSpawner");
        spawnerObject.transform.SetParent(root);
        ObstacleSpawner spawner = spawnerObject.AddComponent<ObstacleSpawner>();
        spawner.Configure(obstacleTemplate, player.transform, LeftWallX, RightWallX, 10f, 2.8f, 0.75f, 36);

        CameraFollow follow = camera.GetComponent<CameraFollow>();
        follow.Configure(player.transform, 3.5f);

        Canvas canvas = CreateCanvas(root, font, out Text scoreText, out Text gameOverText);
        scoreManager.Configure(player.GetComponent<PlayerController>(), scoreText, gameOverText);
    }

    private static Canvas CreateCanvas(Transform root, Font font, out Text scoreText, out Text gameOverText)
    {
        GameObject canvasObject = new GameObject("Canvas");
        canvasObject.transform.SetParent(root);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObject.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1080f, 1920f);
        canvasObject.GetComponent<CanvasScaler>().screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasObject.GetComponent<CanvasScaler>().matchWidthOrHeight = 1f;
        canvasObject.AddComponent<GraphicRaycaster>();

        CreateText(canvas.transform, font, "Title", "Gravity Shaft", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -90f), 64, new Color(0.83f, 1f, 1f, 1f));
        scoreText = CreateText(canvas.transform, font, "ScoreText", "Score: 0", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -170f), 56, new Color(0.55f, 1f, 1f, 1f));
        CreateText(canvas.transform, font, "Hint", "TAP TO FLIP SIDES", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 120f), 56, new Color(0.72f, 1f, 1f, 1f));
        gameOverText = CreateText(canvas.transform, font, "GameOverText", "Game Over", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), 64, new Color(1f, 0.6f, 0.85f, 1f));
        gameOverText.alignment = TextAnchor.MiddleCenter;

        return canvas;
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

    private static Sprite CreateSolidSprite()
    {
        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        return Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
    }
}

