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
    [SerializeField] private Image flashOverlay;
    [SerializeField] private Button restartButton;

    private const float RestartInputDelay = 1f;
    private float survivalTime;
    private float gameOverShownAt;
    private bool isGameOver;
    private bool isRestarting;
    private Coroutine flashRoutine;
    private Coroutine gameOverTextRoutine;

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

    private void Start()
    {
        UpdateScoreText();
        SetGameOverPanelVisible(false);
        SetFlashAlpha(0f);
        ResetGameOverTextScale();
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
            gameOverScoreText.text = "SCORE " + Mathf.FloorToInt(survivalTime);
        }

        Camera.main?.GetComponent<CameraFollow>()?.PlayHitEffect();
        GameAudio.PlayGameOver();
        PlayFlash();
        SetGameOverPanelVisible(true);
    }

    public void Configure(PlayerController playerController, Text scoreLabel, GameObject panel, Text gameOverLabel, Text gameOverScoreLabel, Image flashImage, Button restart)
    {
        player = playerController;
        scoreText = scoreLabel;
        gameOverPanel = panel;
        gameOverText = gameOverLabel;
        gameOverScoreText = gameOverScoreLabel;
        flashOverlay = flashImage;
        restartButton = restart;
        isGameOver = false;
        isRestarting = false;
        survivalTime = 0f;
        gameOverShownAt = 0f;
        UpdateScoreText();
        SetGameOverPanelVisible(false);
        SetFlashAlpha(0f);
        ResetGameOverTextScale();

        if (gameOverScoreText != null)
        {
            gameOverScoreText.text = "SCORE 0";
        }

        BindRestartButton();
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
            scoreText.text = Mathf.FloorToInt(survivalTime).ToString();
        }
    }
}
