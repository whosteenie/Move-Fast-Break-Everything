using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform player;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private int maxEnemies = 10; 
    [SerializeField] private float spawnDistance = 12f; // With current setup, 12f is just outside camera view
    [SerializeField] private float respawnDistance = 40f;
    
    private readonly List<GameObject> activeEnemies = new();
    private float spawnTimer;
    
    private void Update()
    {
        if (player == null)
        {
            return;
        }

        UpdateActiveEnemies();
        
        // TODO: This should gradually increase as the game progresses instead of staying a flat amount
        if (activeEnemies.Count >= maxEnemies)
        {
            return;
        }
        
        spawnTimer -= Time.deltaTime;
        
        // TODO: This should also shrink as the game progresses to up intensity during play
        if (spawnTimer > 0f)
        {
            return;
        }
        
        SpawnEnemy();
        spawnTimer = spawnInterval;
    }

    private void SpawnEnemy()
    {
        Vector3 spawnPosition = GetSpawn();
        activeEnemies.Add(Instantiate(enemyPrefab, spawnPosition, Quaternion.identity, transform));
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
}
