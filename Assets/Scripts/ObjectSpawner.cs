using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [System.Serializable]
    private struct SpawnGroup
    {
        public GameObject[] prefabs;
        public int objectCount;
        public float spawnChance;
        public Vector2 playerSpawnPoint;
        public float objectSpacing; // Empty space between objects
        public float playerSpacing; // Empty space around player spawn
        public int maxPlacementAttempts;
        public bool spawnOnEarth;
        public bool spawnOnTech;
        public bool spawnOnHazard;
        public bool spawnOnDeath;
    }
    
    [SerializeField] private ChunkManager chunkManager;
    [SerializeField] private SpawnGroup[] spawnGroups;
    
    /* Gets the chunk edges from the center point, stores placed object positions,
       and sends each spawn group to the object generator */
    public void GenerateChunk(Transform chunkParent, Vector2 chunkCenter, float chunkSize)
    {
        // Get half the chunk size so we can use the center point to find each edge
        float halfChunkSize = chunkSize * 0.5f;
        
        // Stores placed object positions to avoid overlapping placements
        List<Vector2> placedPositions = new();
        
        for (int i = 0; i < spawnGroups.Length; i++)
        {
            // Send the chunk edges to the object generator
            GenerateObjects(
                spawnGroups[i],
                chunkCenter.x - halfChunkSize,
                chunkCenter.x + halfChunkSize,
                chunkCenter.y - halfChunkSize,
                chunkCenter.y + halfChunkSize,
                chunkParent,
                placedPositions
            );
        }
    }

    /* Places objects from a spawn group inside the chunk bounds, giving each object
       several chances to find a valid spot */
    private void GenerateObjects(SpawnGroup spawnGroup, float spawnMinX, float spawnMaxX, float spawnMinY, float spawnMaxY, Transform parent, List<Vector2> placedPositions)
    {
        float edgePadding = spawnGroup.objectSpacing * 0.5f;

        // Caps number of objects spawning to objectCount
        for (int i = 0; i < spawnGroup.objectCount; i++)
        {
            if (Random.value > spawnGroup.spawnChance)
            {
                continue;
            }
            
            // Give each object several chances to find a valid spot
            for (int attempt = 0; attempt < spawnGroup.maxPlacementAttempts; attempt++)
            {
                Vector2 spawnPosition = new(
                    Random.Range(spawnMinX + edgePadding, spawnMaxX - edgePadding),
                    Random.Range(spawnMinY + edgePadding, spawnMaxY - edgePadding)
                );
                
                if (!IsValidSpawnPosition(spawnGroup, spawnPosition, placedPositions))
                {
                    continue;
                }
                
                GameObject prefab = spawnGroup.prefabs[Random.Range(0, spawnGroup.prefabs.Length)];
                Instantiate(prefab, spawnPosition, Quaternion.identity, parent);
                placedPositions.Add(spawnPosition);
                break;
            }
        }
    }

    /* Checks whether a random position is on the spawn group's allowed terrain and far enough
       from the player spawn and all previously placed objects */
    private bool IsValidSpawnPosition(SpawnGroup spawnGroup, Vector2 candidatePosition, List<Vector2> placedPositions)
    {
        // Objects will only spawn on their spawn group's allowed terrain
        if ((!spawnGroup.spawnOnEarth || !chunkManager.IsEarthTerrain(candidatePosition)) &&
            (!spawnGroup.spawnOnTech || !chunkManager.IsTechTerrain(candidatePosition)) &&
            (!spawnGroup.spawnOnHazard || !chunkManager.IsHazardTerrain(candidatePosition)) &&
            (!spawnGroup.spawnOnDeath || !chunkManager.IsDeathTerrain(candidatePosition)))
        {
            return false;
        }
        
        if (Vector2.Distance(candidatePosition, spawnGroup.playerSpawnPoint) < spawnGroup.playerSpacing)
        {
            return false;
        }
        
        for (int i = 0; i < placedPositions.Count; i++)
        {
            if (Vector2.Distance(candidatePosition, placedPositions[i]) < spawnGroup.objectSpacing)
            {
                return false;
            }
        }
        
        return true;
    }
}
