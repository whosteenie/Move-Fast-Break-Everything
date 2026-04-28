using UnityEngine;

public class TowerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] towerPrefabs;
    [SerializeField] private ChunkManager chunkManager;
    [SerializeField] private int towerCount = 5;
    [SerializeField] private float towerSpawnChance = 0.25f;
    [SerializeField] private Vector2 playerSpawnPoint = Vector2.zero;
    [SerializeField] private float towerSpacing = 4f; // Empty space between towers
    [SerializeField] private float obstacleSpacing = 1.5f; // Empty space between obstacles
    [SerializeField] private float playerSpacing = 20f; // Empty space around player spawn
    [SerializeField] private int maxPlacementAttempts = 10;
    
    public void GenerateChunk(Transform chunkParent, Vector2 chunkCenter, float chunkSize)
    {
        if (Random.value > towerSpawnChance)
        {
            return;
        }
        
        // Get half the chunk size so we can use the center point to find each edge
        float halfChunkSize = chunkSize * 0.5f;
        
        // Send the chunk edges to the tower generator
        GenerateTowers(
            chunkCenter.x - halfChunkSize,
            chunkCenter.x + halfChunkSize,
            chunkCenter.y - halfChunkSize,
            chunkCenter.y + halfChunkSize,
            chunkParent
        );
    }

    private void GenerateTowers(float spawnMinX, float spawnMaxX, float spawnMinY, float spawnMaxY, Transform parent)
    {
        // Stores placed tower positions to avoid overlapping placements
        Vector2[] placedPositions = new Vector2[towerCount];
        int placedCount = 0;
        
        // Caps number of towers spawning to towerCount
        for (int i = 0; i < towerCount; i++)
        {
            if (i > 0 && Random.value > towerSpawnChance)
            {
                break;
            }
            
            // Give each tower several chances to find a valid spot
            for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
            {
                Vector2 spawnPosition = new(
                    Random.Range(spawnMinX, spawnMaxX),
                    Random.Range(spawnMinY, spawnMaxY)
                );
                
                if (!IsValidSpawnPosition(spawnPosition, placedPositions, placedCount, parent))
                {
                    continue;
                }
                
                GameObject towerPrefab = towerPrefabs[Random.Range(0, towerPrefabs.Length)];
                Instantiate(towerPrefab, spawnPosition, Quaternion.identity, parent);
                placedPositions[placedCount] = spawnPosition;
                placedCount++;
                break;
            }
        }
    }

    private bool IsValidSpawnPosition(Vector2 candidatePosition, Vector2[] placedPositions, int placedCount, Transform parent)
    {
        // Towers will only spawn on Earth and Tech terrain
        if (!chunkManager.IsEarthTerrain(candidatePosition) && !chunkManager.IsTechTerrain(candidatePosition))
        {
            return false;
        }
        
        if (Vector2.Distance(candidatePosition, playerSpawnPoint) < playerSpacing)
        {
            return false;
        }
        
        for (int i = 0; i < placedCount; i++)
        {
            if (Vector2.Distance(candidatePosition, placedPositions[i]) < towerSpacing)
            {
                return false;
            }
        }

        // Checks if tower spawn is too close to obstacleSpacing
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(candidatePosition, obstacleSpacing);
        for (int i = 0; i < nearbyColliders.Length; i++)
        {
            if (nearbyColliders[i].transform.IsChildOf(parent))
            {
                return false;
            }
        }
        
        return true;
    }
}