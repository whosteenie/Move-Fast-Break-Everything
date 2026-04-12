using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private int obstacleCount = 10;
    [SerializeField] private float minX = -8f;
    [SerializeField] private float maxX = 8f;
    [SerializeField] private float minY = -4f;
    [SerializeField] private float maxY = 4f;
    [SerializeField] private Vector2 playerSpawnPoint = Vector2.zero;
    [SerializeField] private float obstacleSpacing = 1.5f; // Empty space between obstacles
    [SerializeField] private float playerSpacing = 2f; // Empty space around player spawn
    [SerializeField] private int maxPlacementAttempts = 25;
    
    private void Start()
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
                    Random.Range(minX, maxX),
                    Random.Range(minY, maxY)
                );

                if (!IsValidSpawnPosition(spawnPosition, placedPositions, placedCount))
                {
                    continue;
                }

                Instantiate(obstaclePrefab, spawnPosition, Quaternion.identity);
                placedPositions[placedCount] = spawnPosition;
                placedCount++;
                break;
            }
        }
    }

    /* Checks whether a random position is far enough from the player spawn
       and all previously placed obstacles */
    private bool IsValidSpawnPosition(Vector2 candidatePosition, Vector2[] placedPositions, int placedCount)
    {
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
