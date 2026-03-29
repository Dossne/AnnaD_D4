using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private Transform player;
    [SerializeField] private float leftWallX = -2f;
    [SerializeField] private float rightWallX = 2f;
    [SerializeField] private float startOffsetY = 8f;
    [SerializeField] private float spawnStepY = 3f;
    [SerializeField] private int maxSpawnedObstacles = 30;
    [SerializeField] private float spawnPaddingAboveView = 2f;
    [SerializeField] private float cleanupPaddingBelowView = 4f;
    [SerializeField] private float initialVisibleSpawnOffsetY = 7f;
    [SerializeField] private float initialVisibleEndPadding = 2f;
    [SerializeField] private int hardMaxSameSideChain = 7;

    private readonly Queue<GameObject> spawnedObstacles = new Queue<GameObject>();
    private float nextSpawnY;
    private float elapsedTime;
    private bool nextSpawnOnLeft = true;
    private int sameSideChainCount;
    private int consecutiveEmptyRows;
    private bool isPaused;
    private float difficultyMultiplier = 1f;

    private struct DifficultyPhase
    {
        public float spawnChance;
        public float sideChangeChance;
        public int maxChainLength;
        public int maxEmptyRows;
    }

    private void Start()
    {
        if (player == null || obstaclePrefab == null)
        {
            Debug.LogError("ObstacleSpawner needs player and obstacle references.");
            enabled = false;
            return;
        }

        ResetSpawnerState(false);
        SeedOpeningRows();
        ResetNextSpawnY(startOffsetY);
    }

    private void Update()
    {
        if (player == null)
        {
            return;
        }

        elapsedTime += Time.deltaTime;
        CleanupObstaclesBelowView();

        if (isPaused)
        {
            return;
        }

        float spawnTriggerY = Mathf.Max(player.position.y + startOffsetY, GetTopOfViewY() + spawnPaddingAboveView);
        while (spawnTriggerY >= nextSpawnY)
        {
            SpawnRow(nextSpawnY, false);
            nextSpawnY += spawnStepY;
        }
    }

    private void SeedOpeningRows()
    {
        float seedStartY = player.position.y + initialVisibleSpawnOffsetY;
        float seedEndY = GetTopOfViewY() - initialVisibleEndPadding;

        for (float spawnY = seedStartY; spawnY <= seedEndY; spawnY += spawnStepY)
        {
            SpawnRow(spawnY, true);
        }
    }

    private DifficultyPhase GetCurrentPhase()
    {
        DifficultyPhase phase;
        if (elapsedTime < 10f)
        {
            phase = new DifficultyPhase
            {
                spawnChance = 0.45f,
                sideChangeChance = 0.12f,
                maxChainLength = 4,
                maxEmptyRows = 1
            };
        }
        else if (elapsedTime < 30f)
        {
            phase = new DifficultyPhase
            {
                spawnChance = 0.65f,
                sideChangeChance = 0.45f,
                maxChainLength = 2,
                maxEmptyRows = 1
            };
        }
        else
        {
            phase = new DifficultyPhase
            {
                spawnChance = 0.85f,
                sideChangeChance = 0.65f,
                maxChainLength = 2,
                maxEmptyRows = 0
            };
        }

        phase.spawnChance = Mathf.Clamp01(phase.spawnChance * difficultyMultiplier);
        phase.sideChangeChance = Mathf.Clamp01(phase.sideChangeChance * difficultyMultiplier);
        phase.maxChainLength = Mathf.Max(1, Mathf.RoundToInt(phase.maxChainLength / difficultyMultiplier));
        phase.maxEmptyRows = difficultyMultiplier > 1f ? Mathf.Max(0, phase.maxEmptyRows - 1) : phase.maxEmptyRows;
        return phase;
    }

    private float GetTopOfViewY()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null || !mainCamera.orthographic)
        {
            return player.position.y;
        }

        return mainCamera.transform.position.y + mainCamera.orthographicSize;
    }

    private float GetBottomOfViewY()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null || !mainCamera.orthographic)
        {
            return player.position.y;
        }

        return mainCamera.transform.position.y - mainCamera.orthographicSize;
    }

    private void SpawnRow(float spawnY, bool forceSpawn)
    {
        DifficultyPhase phase = GetCurrentPhase();
        bool shouldSpawn = forceSpawn || Random.value < phase.spawnChance || consecutiveEmptyRows >= phase.maxEmptyRows;
        if (!shouldSpawn)
        {
            consecutiveEmptyRows++;
            return;
        }

        bool reachedSoftLimit = sameSideChainCount >= phase.maxChainLength;
        bool reachedHardLimit = sameSideChainCount >= hardMaxSameSideChain;
        bool shouldChangeSide = reachedHardLimit || reachedSoftLimit || Random.value < phase.sideChangeChance;
        if (shouldChangeSide)
        {
            nextSpawnOnLeft = !nextSpawnOnLeft;
            sameSideChainCount = 0;
        }

        SpawnSingle(nextSpawnOnLeft, spawnY);
        sameSideChainCount++;
        consecutiveEmptyRows = 0;
    }

    private void SpawnSingle(bool spawnLeft, float spawnY)
    {
        if (spawnLeft)
        {
            SpawnObstacle(new Vector2(leftWallX, spawnY), true);
        }
        else
        {
            SpawnObstacle(new Vector2(rightWallX, spawnY), false);
        }
    }

    private void SpawnObstacle(Vector2 position, bool isLeftSide)
    {
        GameObject obstacle = Instantiate(obstaclePrefab, position, Quaternion.identity, transform);
        obstacle.SetActive(true);

        Vector3 scale = obstacle.transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (isLeftSide ? 1f : -1f);
        obstacle.transform.localScale = scale;

        spawnedObstacles.Enqueue(obstacle);

        while (spawnedObstacles.Count > maxSpawnedObstacles)
        {
            DestroyOldestObstacle();
        }
    }

    private void CleanupObstaclesBelowView()
    {
        float cleanupY = GetBottomOfViewY() - cleanupPaddingBelowView;

        while (spawnedObstacles.Count > 0)
        {
            GameObject oldestObstacle = spawnedObstacles.Peek();
            if (oldestObstacle == null)
            {
                spawnedObstacles.Dequeue();
                continue;
            }

            if (oldestObstacle.transform.position.y >= cleanupY)
            {
                break;
            }

            DestroyOldestObstacle();
        }
    }

    private void DestroyOldestObstacle()
    {
        GameObject oldestObstacle = spawnedObstacles.Dequeue();
        if (oldestObstacle != null)
        {
            Destroy(oldestObstacle);
        }
    }

    private void ResetSpawnerState(bool randomizeSide)
    {
        if (randomizeSide)
        {
            nextSpawnOnLeft = Random.value < 0.5f;
        }

        sameSideChainCount = 0;
        consecutiveEmptyRows = 0;
    }

    private void ResetNextSpawnY(float offsetY)
    {
        float firstOffscreenSpawnY = Mathf.Max(GetTopOfViewY() + spawnPaddingAboveView, player.position.y + offsetY);
        nextSpawnY = Mathf.Ceil(firstOffscreenSpawnY / spawnStepY) * spawnStepY;
    }

    public void Configure(
        GameObject template,
        Transform playerTarget,
        float leftX,
        float rightX,
        float offsetY,
        float stepY,
        float chancePerSide,
        int maxObstacles)
    {
        obstaclePrefab = template;
        player = playerTarget;
        leftWallX = leftX;
        rightWallX = rightX;
        startOffsetY = offsetY;
        spawnStepY = stepY;
        maxSpawnedObstacles = maxObstacles;
    }

    public void SetPaused(bool paused)
    {
        isPaused = paused;
    }

    public void BeginRushMode()
    {
        isPaused = true;
        ClearAllObstacles();
    }

    public void EndRushMode()
    {
        isPaused = false;
        difficultyMultiplier = 1.25f;
        elapsedTime = 0f;
        ResetSpawnerState(true);
        ResetNextSpawnY(6f);
    }

    public void ClearAllObstacles()
    {
        while (spawnedObstacles.Count > 0)
        {
            DestroyOldestObstacle();
        }
    }
}
