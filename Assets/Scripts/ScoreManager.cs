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

    private float survivalTime;
    private bool isGameOver;
    private bool isRestarting;
    private Coroutine flashRoutine;

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
    }

    private void Update()
    {
        if (isGameOver)
        {
            bool restartPressed = Input.GetMouseButtonDown(0) || Input.touchCount > 0 || Input.GetKeyDown(KeyCode.R);
            if (!isRestarting && restartPressed)
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

        if (gameOverText != null)
        {
            gameOverText.text = "GAME OVER";
        }

        if (gameOverScoreText != null)
        {
            gameOverScoreText.text = "Score: " + Mathf.FloorToInt(survivalTime);
        }

        Camera.main?.GetComponent<CameraFollow>()?.PlayHitEffect();
        PlayFlash();
        SetGameOverPanelVisible(true);
    }

    public void Configure(PlayerController playerController, Text scoreLabel, GameObject panel, Text gameOverLabel, Text gameOverScoreLabel, Image flashImage)
    {
        player = playerController;
        scoreText = scoreLabel;
        gameOverPanel = panel;
        gameOverText = gameOverLabel;
        gameOverScoreText = gameOverScoreLabel;
        flashOverlay = flashImage;
        isGameOver = false;
        isRestarting = false;
        survivalTime = 0f;
        UpdateScoreText();
        SetGameOverPanelVisible(false);
        SetFlashAlpha(0f);

        if (gameOverScoreText != null)
        {
            gameOverScoreText.text = "Score: 0";
        }
    }

    private IEnumerator RestartPrototypeNextFrame()
    {
        isRestarting = true;
        Instance = null;
        yield return null;
        PrototypeSceneBootstrap.RestartPrototype();
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

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + Mathf.FloorToInt(survivalTime);
        }
    }
}
