using System.Collections.Generic;
using UnityEngine;

public class EnergyOrbSpawner : MonoBehaviour
{
    [SerializeField] private GameObject orbPrefab;
    [SerializeField] private Transform player;
    [SerializeField] private float centerX;
    [SerializeField] private float startOffsetY = 12f;
    [SerializeField] private float spawnStepY = 8f;
    [SerializeField] private int maxSpawnedOrbs = 8;
    [SerializeField] private float spawnPaddingAboveView = 3f;
    [SerializeField] private float cleanupPaddingBelowView = 4f;
    [SerializeField] private float spawnChance = 0.385f;

    private readonly Queue<GameObject> spawnedOrbs = new Queue<GameObject>();
    private float nextSpawnY;
    private bool isPaused;

    private void Start()
    {
        if (player == null || orbPrefab == null)
        {
            Debug.LogError("EnergyOrbSpawner needs player and orb references.");
            enabled = false;
            return;
        }

        float firstSpawnY = Mathf.Max(GetTopOfViewY() + spawnPaddingAboveView, player.position.y + startOffsetY);
        nextSpawnY = Mathf.Ceil(firstSpawnY / spawnStepY) * spawnStepY;
    }

    private void Update()
    {
        if (player == null)
        {
            return;
        }

        CleanupOrbsBelowView();

        if (isPaused)
        {
            return;
        }

        float spawnTriggerY = Mathf.Max(player.position.y + startOffsetY, GetTopOfViewY() + spawnPaddingAboveView);
        while (spawnTriggerY >= nextSpawnY)
        {
            if (Random.value < spawnChance)
            {
                SpawnOrb(nextSpawnY);
            }

            nextSpawnY += spawnStepY;
        }
    }

    private void SpawnOrb(float spawnY)
    {
        GameObject orb = Instantiate(orbPrefab, new Vector3(centerX, spawnY, 0f), Quaternion.identity, transform);
        orb.SetActive(true);
        spawnedOrbs.Enqueue(orb);

        while (spawnedOrbs.Count > maxSpawnedOrbs)
        {
            DestroyOldestOrb();
        }
    }

    private void CleanupOrbsBelowView()
    {
        float cleanupY = GetBottomOfViewY() - cleanupPaddingBelowView;

        while (spawnedOrbs.Count > 0)
        {
            GameObject oldestOrb = spawnedOrbs.Peek();
            if (oldestOrb == null)
            {
                spawnedOrbs.Dequeue();
                continue;
            }

            if (oldestOrb.transform.position.y >= cleanupY)
            {
                break;
            }

            DestroyOldestOrb();
        }
    }

    private void DestroyOldestOrb()
    {
        GameObject oldestOrb = spawnedOrbs.Dequeue();
        if (oldestOrb != null)
        {
            Destroy(oldestOrb);
        }
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

    public void Configure(GameObject template, Transform playerTarget, float x, float offsetY, float stepY, int maxOrbs, float chance)
    {
        orbPrefab = template;
        player = playerTarget;
        centerX = x;
        startOffsetY = offsetY;
        spawnStepY = stepY;
        maxSpawnedOrbs = maxOrbs;
        spawnChance = Mathf.Clamp01(chance * 0.7f);
    }

    public void SetPaused(bool paused)
    {
        isPaused = paused;
    }

    public void ClearAllOrbs()
    {
        while (spawnedOrbs.Count > 0)
        {
            DestroyOldestOrb();
        }
    }

    public void SpawnRushOrbs(float startY, int count, float spacing)
    {
        if (orbPrefab == null)
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            GameObject orb = Instantiate(orbPrefab, new Vector3(centerX, startY + spacing * i, 0f), Quaternion.identity, transform);
            orb.transform.localScale = Vector3.one * 0.7f;
            orb.SetActive(true);
            spawnedOrbs.Enqueue(orb);
        }

        nextSpawnY = Mathf.Max(nextSpawnY, startY + spacing * count);
    }
}

