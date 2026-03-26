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

    private readonly Queue<GameObject> spawnedObstacles = new Queue<GameObject>();
    private float nextSpawnY;

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError("ObstacleSpawner needs a player reference.");
            enabled = false;
            return;
        }

        nextSpawnY = player.position.y + startOffsetY;
    }

    private void Update()
    {
        if (player == null)
        {
            return;
        }

        while (player.position.y + startOffsetY >= nextSpawnY)
        {
            SpawnRow(nextSpawnY);
            nextSpawnY += spawnStepY;
        }
    }

    private void SpawnRow(float spawnY)
    {
        bool spawnLeft = Random.value < obstacleChancePerSide;
        bool spawnRight = Random.value < obstacleChancePerSide;

        if (!spawnLeft && !spawnRight)
        {
            if (Random.value < 0.5f)
            {
                spawnLeft = true;
            }
            else
            {
                spawnRight = true;
            }
        }

        if (spawnLeft)
        {
            SpawnObstacle(new Vector2(leftWallX, spawnY));
        }

        if (spawnRight)
        {
            SpawnObstacle(new Vector2(rightWallX, spawnY));
        }
    }

    private void SpawnObstacle(Vector2 position)
    {
        GameObject obstacle = Instantiate(obstaclePrefab, position, Quaternion.identity, transform);
        spawnedObstacles.Enqueue(obstacle);

        while (spawnedObstacles.Count > maxSpawnedObstacles)
        {
            GameObject oldestObstacle = spawnedObstacles.Dequeue();
            if (oldestObstacle != null)
            {
                Destroy(oldestObstacle);
            }
        }
    }
}
