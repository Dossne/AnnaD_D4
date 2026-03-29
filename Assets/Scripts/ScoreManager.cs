using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private PlayerController player;
    [SerializeField] private Text scoreText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text gameOverText;
    [SerializeField] private Text gameOverScoreText;
    [SerializeField] private Text pickupPopupText;
    [SerializeField] private Image flashOverlay;
    [SerializeField] private Button restartButton;
    [SerializeField] private ObstacleSpawner obstacleSpawner;
    [SerializeField] private EnergyOrbSpawner orbSpawner;

    private const float RestartInputDelay = 1f;
    private const int OrbsToStartRush = 10;
    private const int RushOrbsToFinish = 10;
    private const float RushWallTension = 1f;
    private const float RushUpwardSpeed = 18f;
    private const float RushSpawnOffsetY = 4.5f;
    private const float RushOrbSpacing = 2.8f;

    private float survivalTime;
    private int collectedPoints;
    private int collectedOrbs;
    private int rushCollectedOrbs;
    private float gameOverShownAt;
    private bool isGameOver;
    private bool isRestarting;
    private bool isRushMode;
    private Coroutine flashRoutine;
    private Coroutine gameOverTextRoutine;
    private Coroutine pickupPopupRoutine;
    private Coroutine corridorRushRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public static void PrepareForPrototypeRebuild()
    {
        Instance = null;
    }

    private void Start()
    {
        UpdateScoreText();
        SetGameOverPanelVisible(false);
        SetFlashAlpha(0f);
        ResetGameOverTextScale();
        SetPickupPopupVisible(false);
        BindRestartButton();
    }

    private void Update()
    {
        if (isGameOver)
        {
            bool canRestart = Time.unscaledTime >= gameOverShownAt + RestartInputDelay;
            bool restartPressed = Input.GetMouseButtonDown(0) || Input.touchCount > 0 || Input.GetKeyDown(KeyCode.R);
            if (canRestart && !isRestarting && restartPressed)
            {
                StartCoroutine(RestartPrototypeNextFrame());
            }

            return;
        }

        if (player == null || !player.IsAlive)
        {
            return;
        }

        survivalTime += Time.deltaTime;
        UpdateScoreText();
    }

    public void GameOver()
    {
        if (isGameOver)
        {
            return;
        }

        isGameOver = true;
        gameOverShownAt = Time.unscaledTime;
        ForceExitRushMode();

        if (gameOverText != null)
        {
            gameOverText.text = "GAME OVER";
            gameOverText.rectTransform.localScale = Vector3.zero;

            if (gameOverTextRoutine != null)
            {
                StopCoroutine(gameOverTextRoutine);
            }

            gameOverTextRoutine = StartCoroutine(AnimateGameOverText());
        }

        if (gameOverScoreText != null)
        {
            gameOverScoreText.text = "SCORE " + GetCurrentScore();
        }

        Camera.main?.GetComponent<CameraFollow>()?.PlayHitEffect();
        GameAudio.PlayGameOver();
        PlayFlash();
        SetGameOverPanelVisible(true);
    }

    public void Configure(PlayerController playerController, Text scoreLabel, GameObject panel, Text gameOverLabel, Text gameOverScoreLabel, Text pickupLabel, Image flashImage, Button restart, ObstacleSpawner obstacleSpawnController, EnergyOrbSpawner orbSpawnController)
    {
        player = playerController;
        scoreText = scoreLabel;
        gameOverPanel = panel;
        gameOverText = gameOverLabel;
        gameOverScoreText = gameOverScoreLabel;
        pickupPopupText = pickupLabel;
        flashOverlay = flashImage;
        restartButton = restart;
        obstacleSpawner = obstacleSpawnController;
        orbSpawner = orbSpawnController;
        isGameOver = false;
        isRestarting = false;
        isRushMode = false;
        survivalTime = 0f;
        collectedPoints = 0;
        collectedOrbs = 0;
        rushCollectedOrbs = 0;
        gameOverShownAt = 0f;
        UpdateScoreText();
        SetGameOverPanelVisible(false);
        SetFlashAlpha(0f);
        ResetGameOverTextScale();
        SetPickupPopupVisible(false);
        PrototypeSceneBootstrap.SetCorridorTension(0f);

        if (gameOverScoreText != null)
        {
            gameOverScoreText.text = "SCORE 0";
        }

        BindRestartButton();
    }

    public void AddPoints(int amount)
    {
        if (isGameOver || amount <= 0)
        {
            return;
        }

        collectedPoints += amount;
        UpdateScoreText();
    }

    public void CollectOrb(int amount)
    {
        if (isGameOver || amount <= 0)
        {
            return;
        }

        collectedPoints += amount;
        UpdateScoreText();
        ShowPickupPopup("+" + amount);

        if (isRushMode)
        {
            rushCollectedOrbs++;
            if (rushCollectedOrbs >= RushOrbsToFinish)
            {
                EndRushMode();
            }

            return;
        }

        collectedOrbs++;
        if (collectedOrbs >= OrbsToStartRush)
        {
            StartRushMode();
        }
    }

    private void BindRestartButton()
    {
        if (restartButton == null)
        {
            return;
        }

        restartButton.onClick.RemoveListener(OnRestartButtonPressed);
        restartButton.onClick.AddListener(OnRestartButtonPressed);
    }

    private void OnRestartButtonPressed()
    {
        bool canRestart = Time.unscaledTime >= gameOverShownAt + RestartInputDelay;
        if (isGameOver && canRestart && !isRestarting)
        {
            StartCoroutine(RestartPrototypeNextFrame());
        }
    }

    private IEnumerator RestartPrototypeNextFrame()
    {
        isRestarting = true;
        GameAudio.PlayRestart();
        Instance = null;
        yield return null;
        PrototypeSceneBootstrap.RestartPrototype();
    }

    private IEnumerator AnimateGameOverText()
    {
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float scale = Mathf.SmoothStep(0f, 1f, t);

            if (gameOverText != null)
            {
                gameOverText.rectTransform.localScale = Vector3.one * scale;
            }

            yield return null;
        }

        ResetGameOverTextScale();
        gameOverTextRoutine = null;
    }

    private IEnumerator AnimatePickupPopup(string value)
    {
        if (pickupPopupText == null)
        {
            yield break;
        }

        pickupPopupText.text = value;
        pickupPopupText.rectTransform.anchoredPosition = new Vector2(0f, -250f);
        pickupPopupText.rectTransform.localScale = Vector3.one * 0.7f;
        SetPickupPopupAlpha(1f);
        SetPickupPopupVisible(true);

        float duration = 0.65f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            pickupPopupText.rectTransform.anchoredPosition = Vector2.Lerp(new Vector2(0f, -250f), new Vector2(0f, -360f), t);
            pickupPopupText.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.7f, 1.05f, t);
            SetPickupPopupAlpha(1f - t);
            yield return null;
        }

        SetPickupPopupVisible(false);
        pickupPopupRoutine = null;
    }

    private IEnumerator AnimateCorridorRush(float targetTension)
    {
        float startTension = PrototypeSceneBootstrap.GetCorridorTension();
        float duration = 0.35f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = Mathf.SmoothStep(0f, 1f, t);
            PrototypeSceneBootstrap.SetCorridorTension(Mathf.Lerp(startTension, targetTension, eased));
            yield return null;
        }

        PrototypeSceneBootstrap.SetCorridorTension(targetTension);
        corridorRushRoutine = null;
    }

    private IEnumerator FlashRoutine()
    {
        float duration = 0.18f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0.18f, 0f, elapsed / duration);
            SetFlashAlpha(alpha);
            yield return null;
        }

        SetFlashAlpha(0f);
        flashRoutine = null;
    }

    private void PlayFlash()
    {
        if (flashOverlay == null)
        {
            return;
        }

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private void StartRushMode()
    {
        if (isRushMode || player == null)
        {
            return;
        }

        isRushMode = true;
        rushCollectedOrbs = 0;
        collectedOrbs = 0;

        obstacleSpawner?.SetPaused(true);
        orbSpawner?.SetPaused(true);
        orbSpawner?.ClearAllOrbs();
        orbSpawner?.SpawnRushOrbs(player.transform.position.y + RushSpawnOffsetY, RushOrbsToFinish, RushOrbSpacing);
        player.EnterRushMode(0f, RushUpwardSpeed);
        StartCorridorRushAnimation(RushWallTension);
    }

    private void EndRushMode()
    {
        if (!isRushMode)
        {
            return;
        }

        isRushMode = false;
        rushCollectedOrbs = 0;
        obstacleSpawner?.SetPaused(false);
        orbSpawner?.SetPaused(false);
        player?.ExitRushMode();
        StartCorridorRushAnimation(0f);
    }

    private void ForceExitRushMode()
    {
        isRushMode = false;
        rushCollectedOrbs = 0;
        collectedOrbs = 0;
        obstacleSpawner?.SetPaused(false);
        orbSpawner?.SetPaused(false);
        player?.ExitRushMode();
        StartCorridorRushAnimation(0f);
    }

    private void StartCorridorRushAnimation(float targetTension)
    {
        if (corridorRushRoutine != null)
        {
            StopCoroutine(corridorRushRoutine);
        }

        corridorRushRoutine = StartCoroutine(AnimateCorridorRush(targetTension));
    }

    private void ShowPickupPopup(string value)
    {
        if (pickupPopupText == null)
        {
            return;
        }

        if (pickupPopupRoutine != null)
        {
            StopCoroutine(pickupPopupRoutine);
        }

        pickupPopupRoutine = StartCoroutine(AnimatePickupPopup(value));
    }

    private int GetCurrentScore()
    {
        return Mathf.FloorToInt(survivalTime) + collectedPoints;
    }

    private void SetFlashAlpha(float alpha)
    {
        if (flashOverlay == null)
        {
            return;
        }

        Color color = flashOverlay.color;
        color.a = alpha;
        flashOverlay.color = color;
    }

    private void SetGameOverPanelVisible(bool visible)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(visible);
        }
        else if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(visible);
        }
    }

    private void SetPickupPopupVisible(bool visible)
    {
        if (pickupPopupText != null)
        {
            pickupPopupText.gameObject.SetActive(visible);
        }
    }

    private void SetPickupPopupAlpha(float alpha)
    {
        if (pickupPopupText == null)
        {
            return;
        }

        Color color = pickupPopupText.color;
        color.a = alpha;
        pickupPopupText.color = color;
    }

    private void ResetGameOverTextScale()
    {
        if (gameOverText != null)
        {
            gameOverText.rectTransform.localScale = Vector3.one;
        }
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = GetCurrentScore().ToString();
        }
    }
}
