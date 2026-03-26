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
    [SerializeField] private Button restartButton;

    private float survivalTime;
    private bool isGameOver;
    private bool isRestarting;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        UpdateScoreText();
        SetGameOverPanelVisible(false);

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(OnRestartPressed);
            restartButton.onClick.AddListener(OnRestartPressed);
        }
    }

    private void Update()
    {
        if (isGameOver)
        {
            if (!isRestarting && Input.GetKeyDown(KeyCode.R))
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
            gameOverText.text = "Game Over";
        }

        SetGameOverPanelVisible(true);
    }

    public void Configure(PlayerController playerController, Text scoreLabel, GameObject panel, Text gameOverLabel, Button restart)
    {
        player = playerController;
        scoreText = scoreLabel;
        gameOverPanel = panel;
        gameOverText = gameOverLabel;
        restartButton = restart;
        isGameOver = false;
        isRestarting = false;
        survivalTime = 0f;
        UpdateScoreText();
        SetGameOverPanelVisible(false);

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(OnRestartPressed);
            restartButton.onClick.AddListener(OnRestartPressed);
        }
    }

    public void OnRestartPressed()
    {
        if (!isGameOver || isRestarting)
        {
            return;
        }

        StartCoroutine(RestartPrototypeNextFrame());
    }

    private IEnumerator RestartPrototypeNextFrame()
    {
        isRestarting = true;
        yield return null;
        PrototypeSceneBootstrap.RestartPrototype();
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
