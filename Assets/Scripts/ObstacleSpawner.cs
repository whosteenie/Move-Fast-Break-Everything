using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] obstaclePrefabs;
    [SerializeField] private ChunkManager chunkManager;
    [SerializeField] private int obstacleCount = 3;
    [SerializeField] private Vector2 playerSpawnPoint = Vector2.zero;
    [SerializeField] private float obstacleSpacing = 1.5f; // Empty space between obstacles
    [SerializeField] private float playerSpacing = 2f; // Empty space around player spawn
    [SerializeField] private int maxPlacementAttempts = 10;
    
    public void GenerateChunk(Transform chunkParent, Vector2 chunkCenter, float chunkSize)
    {
        // Get half the chunk size so we can use the center point to find each edge
        float halfChunkSize = chunkSize * 0.5f;
        
        // Send the chunk edges to the obstacle generator
        GenerateObstacles(
            chunkCenter.x - halfChunkSize,
            chunkCenter.x + halfChunkSize,
            chunkCenter.y - halfChunkSize,
            chunkCenter.y + halfChunkSize,
            chunkParent
        );
    }
    
    private void GenerateObstacles(float spawnMinX, float spawnMaxX, float spawnMinY, float spawnMaxY, Transform parent)
    {
        // Stores placed obstacle positions to avoid overlapping placements
        Vector2[] placedPositions = new Vector2[obstacleCount];
        int placedCount = 0;

        // Place total number of set obstacles
        for (int i = 0; i < obstacleCount; i++)
        {
            // Give each obstacle several chances to find a valid spot
            for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
            {
                Vector2 spawnPosition = new Vector2(
                    Random.Range(spawnMinX, spawnMaxX),
                    Random.Range(spawnMinY, spawnMaxY)
                );

                if (!IsValidSpawnPosition(spawnPosition, placedPositions, placedCount))
                {
                    continue;
                }

                GameObject obstaclePrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
                Instantiate(obstaclePrefab, spawnPosition, Quaternion.identity, parent);
                placedPositions[placedCount] = spawnPosition;
                placedCount++;
                break;
            }
        }
    }

    /* Checks whether a random position is on Earth terrain and far enough from the player spawn
       and all previously placed obstacles */
    private bool IsValidSpawnPosition(Vector2 candidatePosition, Vector2[] placedPositions, int placedCount)
    {
        // Trees will only spawn on Earth terrain
        if (!chunkManager.IsEarthTerrain(candidatePosition))
        {
            return false;
        }

        if (Vector2.Distance(candidatePosition, playerSpawnPoint) < playerSpacing)
        {
            return false;
        }

        for (int i = 0; i < placedCount; i++)
        {
            if (Vector2.Distance(candidatePosition, placedPositions[i]) < obstacleSpacing)
            {
                return false;
            }
        }

        return true;
    }
}
