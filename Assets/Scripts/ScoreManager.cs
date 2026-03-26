using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private PlayerController player;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text gameOverText;

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

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (isGameOver)
        {
            if (!isRestarting && ShouldRestart())
            {
                StartCoroutine(ReloadSceneNextFrame());
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
            gameOverText.gameObject.SetActive(true);
            gameOverText.text = "Game Over\nTap or Press R to Restart";
        }
    }

    public void Configure(PlayerController playerController, Text scoreLabel, Text gameOverLabel)
    {
        player = playerController;
        scoreText = scoreLabel;
        gameOverText = gameOverLabel;
        isGameOver = false;
        isRestarting = false;
        survivalTime = 0f;
        UpdateScoreText();

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }
    }

    private bool ShouldRestart()
    {
        return Input.GetKeyDown(KeyCode.R)
            || Input.GetMouseButtonDown(0)
            || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
    }

    private IEnumerator ReloadSceneNextFrame()
    {
        isRestarting = true;
        yield return null;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + Mathf.FloorToInt(survivalTime);
        }
    }
}
