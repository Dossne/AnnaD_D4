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
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject root = new GameObject("PrototypeRuntime");
        CreateBackdrop(root.transform, baseSprite);
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
        camera.backgroundColor = new Color(0.12f, 0.12f, 0.14f, 1f);
        camera.transform.position = new Vector3(0f, 0f, -10f);

        CameraFollow follow = camera.GetComponent<CameraFollow>();
        if (follow == null)
        {
            follow = camera.gameObject.AddComponent<CameraFollow>();
        }
    }

    private static void CreateBackdrop(Transform root, Sprite sprite)
    {
        GameObject background = CreateSpriteObject(
            "Background",
            root,
            sprite,
            new Color(0.16f, 0.16f, 0.18f, 1f),
            new Vector3(8f, 500f, 1f),
            new Vector3(0f, 250f, 15f));

        background.GetComponent<SpriteRenderer>().sortingOrder = -10;
    }

    private static void CreateWalls(Transform root, Sprite sprite)
    {
        GameObject leftWall = CreateSpriteObject(
            "LeftWall",
            root,
            sprite,
            new Color(0.8f, 0.8f, 0.85f, 1f),
            new Vector3(0.2f, 500f, 1f),
            new Vector3(LeftWallX - 0.75f, 250f, 0f));

        leftWall.GetComponent<SpriteRenderer>().sortingOrder = -2;

        GameObject rightWall = CreateSpriteObject(
            "RightWall",
            root,
            sprite,
            new Color(0.8f, 0.8f, 0.85f, 1f),
            new Vector3(0.2f, 500f, 1f),
            new Vector3(RightWallX + 0.75f, 250f, 0f));

        rightWall.GetComponent<SpriteRenderer>().sortingOrder = -2;
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

        GameObject body = CreateSpriteObject(
            "Body",
            player.transform,
            sprite,
            new Color(0.2f, 0.9f, 1f, 1f),
            new Vector3(0.7f, 0.7f, 1f),
            Vector3.zero);

        body.GetComponent<SpriteRenderer>().sortingOrder = 2;

        PlayerController controller = player.AddComponent<PlayerController>();
        controller.Configure(4.5f, 34f, LeftWallX, RightWallX, true, body.transform, 0.09f, 8.5f, 0.08f);

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

        GameObject body = CreateSpriteObject(
            "Body",
            obstacle.transform,
            sprite,
            new Color(1f, 0.35f, 0.35f, 1f),
            new Vector3(0.9f, 0.9f, 1f),
            Vector3.zero);

        body.GetComponent<SpriteRenderer>().sortingOrder = 2;

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
        spawner.Configure(obstacleTemplate, player.transform, LeftWallX, RightWallX, 10f, 3f, 0.7f, 36);

        CameraFollow follow = camera.GetComponent<CameraFollow>();
        follow.Configure(player.transform, 3.5f);

        CreateCanvas(root, font, out Text scoreText, out Text gameOverText);
        scoreManager.Configure(player.GetComponent<PlayerController>(), scoreText, gameOverText);
    }

    private static void CreateCanvas(Transform root, Font font, out Text scoreText, out Text gameOverText)
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

        scoreText = CreateText(
            canvas.transform,
            font,
            "ScoreText",
            "Score: 0",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -80f),
            42,
            Color.white);

        gameOverText = CreateText(
            canvas.transform,
            font,
            "GameOverText",
            "Game Over\nTap or Press R to Restart",
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            48,
            Color.white);

        gameOverText.alignment = TextAnchor.MiddleCenter;
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
        texture.filterMode = FilterMode.Point;

        return Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
    }
}

