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
    [SerializeField] private float obstacleChancePerSide = 0.6f;
    [SerializeField] private int maxSpawnedObstacles = 30;
    [SerializeField] private float spawnPaddingAboveView = 2f;
    [SerializeField] private float cleanupPaddingBelowView = 4f;

    private readonly Queue<GameObject> spawnedObstacles = new Queue<GameObject>();
    private float nextSpawnY;

    private void Start()
    {
        if (player == null || obstaclePrefab == null)
        {
            Debug.LogError("ObstacleSpawner needs player and obstacle references.");
            enabled = false;
            return;
        }

        float initialSpawnY = Mathf.Max(player.position.y + startOffsetY, GetTopOfViewY() + spawnPaddingAboveView);
        nextSpawnY = initialSpawnY;
    }

    private void Update()
    {
        if (player == null)
        {
            return;
        }

        float spawnTriggerY = Mathf.Max(player.position.y + startOffsetY, GetTopOfViewY() + spawnPaddingAboveView);
        while (spawnTriggerY >= nextSpawnY)
        {
            SpawnRow(nextSpawnY);
            nextSpawnY += spawnStepY;
        }

        CleanupObstaclesBelowView();
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

    private void SpawnRow(float spawnY)
    {
        if (Random.value >= obstacleChancePerSide)
        {
            return;
        }

        bool spawnLeft = Random.value < 0.5f;

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
        obstacleChancePerSide = chancePerSide;
        maxSpawnedObstacles = maxObstacles;
    }
}
