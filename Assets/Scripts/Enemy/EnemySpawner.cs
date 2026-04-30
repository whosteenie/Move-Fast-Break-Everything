using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    private struct SpawnPhase
    {
        public float startTimeSeconds;
        public int maxEnemies;
        public float spawnInterval;
    }
    
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private Transform player;
    [SerializeField] private float spawnDistance = 18f; // With current setup, 18f is just outside camera view
    [SerializeField] private float respawnDistance = 40f;
    [SerializeField] private SpawnPhase[] spawnPhases;
    
    private readonly List<GameObject> activeEnemies = new();
    private float spawnTimer;
    
    private void Update()
    {
        if (player == null)
        {
            return;
        }

        UpdateActiveEnemies();
        GetSpawnPhase(out int currentMaxEnemies, out float currentSpawnInterval);
        
        if (activeEnemies.Count >= currentMaxEnemies)
        {
            return;
        }
        
        spawnTimer -= Time.deltaTime;
        
        if (spawnTimer > 0f)
        {
            return;
        }
        
        SpawnEnemy();
        spawnTimer = currentSpawnInterval;
    }

    private void SpawnEnemy()
    {
        Vector3 spawnPosition = GetSpawn();

        
        GameObject randEnemy = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        GameObject enemy = Instantiate(randEnemy, spawnPosition, Quaternion.identity, transform);
        activeEnemies.Add(enemy);
    }
    
    // Picks random position around player at a set distance
    private Vector3 GetSpawn()
    {
        Vector2 spawnDirection = Random.insideUnitCircle.normalized;
        Vector3 spawnPosition = player.position + new Vector3(spawnDirection.x, spawnDirection.y, 0f) * spawnDistance;
        spawnPosition.z = 0f;
        return spawnPosition;
    }
    
    // Removes destroyed enemy references and moves distant enemies back near the player
    private void UpdateActiveEnemies()
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            GameObject enemyObject = activeEnemies[i];

            if (enemyObject == null)
            {
                activeEnemies.RemoveAt(i);
                continue;
            }

            if (Vector2.Distance(enemyObject.transform.position, player.position) > respawnDistance)
            {
                enemyObject.transform.position = GetSpawn();
            }
        }
    }
    
    // Picks the active spawn phase based on current run time
    private void GetSpawnPhase(out int currentMaxEnemies, out float currentSpawnInterval)
    {
        SpawnPhase currentPhase = spawnPhases[0];
        float runTimeSeconds = GameManager.CurrentRunTimeSeconds;
        
        for (int i = 1; i < spawnPhases.Length; i++)
        {
            if (runTimeSeconds < spawnPhases[i].startTimeSeconds)
            {
                break;
            }
            
            currentPhase = spawnPhases[i];
        }
        
        currentMaxEnemies = currentPhase.maxEnemies;
        currentSpawnInterval = currentPhase.spawnInterval;
    }
}
