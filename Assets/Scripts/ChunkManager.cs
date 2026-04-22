using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public enum TerrainType
    {
        Death,
        Hazard,
        Earth,
        Tech
    }
    
    [SerializeField] private Transform player;
    [SerializeField] private GameObject floorDeathPrefab;
    [SerializeField] private GameObject floorHazardPrefab;
    [SerializeField] private GameObject floorEarthPrefab;
    [SerializeField] private GameObject floorTechPrefab;
    [SerializeField] private ObstacleSpawner obstacleSpawner;
    [SerializeField] private float chunkSize = 20f;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private int loadRadius = 1; // How many chunks away from player are loaded
    [SerializeField] private int unloadRadius = 10; // How many chunks away from player until unloaded
    [SerializeField] private float noiseScale = 15f; // Higher = Wider zones; Lower = Tighter zones
    [SerializeField] private float deathMax = 0.13f;
    [SerializeField] private float hazardMax = 0.25f;
    [SerializeField] private float earthMax = 0.5f;
    
    // Currently, our terrain and objects unload at the same time.
    /* TODO: If performance starts hurting, split terrain/object unloading (terrain can be unloaded
       just outside camera, while objects stick around longer for game balance) */
    
    // In our current setup, terrain can only touch neighboring layers (Death -> Hazard -> Earth -> Tech)
    /* TODO: Layer two Perlin noise maps on top of each other, one for Tech/Earth and one for Hazard/Death.
       This will make the ground split open anywhere making for a more interesting map */
    
    private readonly Dictionary<Vector2Int, GameObject> loadedChunks = new();
    private Vector2Int currentPlayerChunk;
    private Vector2 noiseOffset;
    
    private void Start()
    {
        // Random offset so each world is unique 
        noiseOffset = new Vector2(
            Random.Range(-10000f, 10000f),
            Random.Range(-10000f, 10000f)
        );
        
        currentPlayerChunk = Vector2Int.zero;
        UpdateLoadedChunks();
    }
    
    // Checks if player moved to different chunk 
    private void Update()
    {
        Vector2Int playerChunk = GetChunkCoordinate(player.position);
        
        if (playerChunk == currentPlayerChunk)
        {
            return;
        }
        
        currentPlayerChunk = playerChunk;
        UpdateLoadedChunks();
    }
    
    // Converts a world position into a chunk coordinate 
    public Vector2Int GetChunkCoordinate(Vector3 worldPosition)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPosition.x / chunkSize),
            Mathf.FloorToInt(worldPosition.y / chunkSize)
        );
    }
    
    // Loads chunks close to the player and destroys chunks that are too far away 
    private void UpdateLoadedChunks()
    {
        // Checks X and Y around player and loads missing chunk 
        for (int y = -loadRadius; y <= loadRadius; y++)
        {
            for (int x = -loadRadius; x <= loadRadius; x++)
            {
                Vector2Int chunkCoordinate = currentPlayerChunk + new Vector2Int(x, y);
                
                if (!loadedChunks.ContainsKey(chunkCoordinate))
                {
                    LoadChunk(chunkCoordinate);
                }
            }
        }

        List<Vector2Int> chunksToUnload = new();

        // Checks X and Y around player and marks for removal if chunk is too far
        foreach (Vector2Int chunkCoordinate in loadedChunks.Keys) 
        {
            int xDistance = Mathf.Abs(chunkCoordinate.x - currentPlayerChunk.x);
            int yDistance = Mathf.Abs(chunkCoordinate.y - currentPlayerChunk.y);
            
            if (xDistance > unloadRadius || yDistance > unloadRadius)
            {
                chunksToUnload.Add(chunkCoordinate);
            }
        }
        
        foreach (Vector2Int chunkCoordinate in chunksToUnload)
        {
            UnloadChunk(chunkCoordinate);
        }
    }

    private void LoadChunk(Vector2Int chunkCoordinate)
    {
        Vector3 chunkCenter = GetChunkCenter(chunkCoordinate);
        GameObject chunkObject = new GameObject();
        chunkObject.transform.SetParent(transform);
        chunkObject.transform.position = chunkCenter;
        
        loadedChunks.Add(chunkCoordinate, chunkObject);
        CreateChunkCells(chunkObject.transform, chunkCenter);
        
        obstacleSpawner.GenerateChunk(chunkObject.transform, chunkCenter, chunkSize);
    }
    
    private void UnloadChunk(Vector2Int chunkCoordinate)
    {
        Destroy(loadedChunks[chunkCoordinate]);
        loadedChunks.Remove(chunkCoordinate); 
    }

    private Vector3 GetChunkCenter(Vector2Int chunkCoordinate)
    {
        float halfChunkSize = chunkSize * 0.5f;
        
        return new Vector3(
            chunkCoordinate.x * chunkSize + halfChunkSize,
            chunkCoordinate.y * chunkSize + halfChunkSize,
            0f
        );
    }

    // Creates every floor cell inside a chunk 
    private void CreateChunkCells(Transform chunkParent, Vector3 chunkCenter)
    {
        int cellsPerAxis = Mathf.RoundToInt(chunkSize / cellSize);
        float halfChunkSize = chunkSize * 0.5f; 
        Vector2 chunkBottomLeft = new(
            chunkCenter.x - halfChunkSize,
            chunkCenter.y - halfChunkSize 
        );
        
        // Assigns terrain based on Perlin noise value of cell position
        for (int y = 0; y < cellsPerAxis; y++)
        {
            for (int x = 0; x < cellsPerAxis; x++)
            {
                Vector3 cellPosition = new(
                    chunkBottomLeft.x + (x + 0.5f) * cellSize,
                    chunkBottomLeft.y + (y + 0.5f) * cellSize,
                    0f
                );
                
                float noiseValue = EvaluateNoise(cellPosition);
                TerrainType terrainType = GetTerrainType(noiseValue);
                GameObject floorPrefab = GetFloorPrefab(terrainType);
                
                Instantiate(floorPrefab, cellPosition, Quaternion.identity, chunkParent);
            }
        }
    }

    private float EvaluateNoise(Vector2 worldPosition)
    {
        float sampleX = (worldPosition.x + noiseOffset.x) / noiseScale;
        float sampleY = (worldPosition.y + noiseOffset.y) / noiseScale;
        
        return Mathf.PerlinNoise(sampleX, sampleY);
    }

    private TerrainType GetTerrainType(float noiseValue)
    {
        if (noiseValue <= deathMax)
        {
            return TerrainType.Death;
        }
        
        if (noiseValue <= hazardMax)
        {
            return TerrainType.Hazard;
        }
        
        if (noiseValue <= earthMax)
        {
            return TerrainType.Earth;
        }
        
        return TerrainType.Tech;
    }

    private GameObject GetFloorPrefab(TerrainType terrainType)
    {
        if (terrainType == TerrainType.Death)
        {
            return floorDeathPrefab;
        }
        
        if (terrainType == TerrainType.Hazard)
        {
            return floorHazardPrefab;
        }
        
        if (terrainType == TerrainType.Earth)
        {
            return floorEarthPrefab;
        }
        
        return floorTechPrefab;
    }
    
    public bool IsDeathTerrain(Vector2 worldPosition)
    {
        return GetTerrainType(EvaluateNoise(worldPosition)) == TerrainType.Death;
    }
    
    public bool IsHazardTerrain(Vector2 worldPosition)
    {
        return GetTerrainType(EvaluateNoise(worldPosition)) == TerrainType.Hazard;
    }
    
    public bool IsEarthTerrain(Vector2 worldPosition)
    {
        return GetTerrainType(EvaluateNoise(worldPosition)) == TerrainType.Earth;
    }
    
    public bool IsTechTerrain(Vector2 worldPosition)
    {
        return GetTerrainType(EvaluateNoise(worldPosition)) == TerrainType.Tech;
    }
}
