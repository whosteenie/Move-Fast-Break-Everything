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
    [SerializeField] private ObjectSpawner objectSpawner;
    [SerializeField] private float chunkSize = 20f;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private int loadRadius = 1; // How many chunks away from player are loaded
    [SerializeField] private int terrainUnloadRadius = 1; // How many chunks away from player until terrain is unloaded
    [SerializeField] private int objectUnloadRadius = 20; // How many chunks away from player until objects are unloaded
    [SerializeField] private float noiseScale = 30f; // Harmless terrain scale. Higher = Wider zones; Lower = Tighter zones
    [SerializeField] private float dangerNoiseScale = 30f; // Danger terrain scale. Higher = Wider zones; Lower = Tighter zones
    [SerializeField] private float deathMax = 0.1f;
    [SerializeField] private float hazardMax = 0.18f;
    [SerializeField] private float earthMax = 0.4f;
    
    private readonly Dictionary<Vector2Int, GameObject> loadedTerrainChunks = new();
    private readonly Dictionary<Vector2Int, GameObject> loadedObjectChunks = new();
    private Vector2Int currentPlayerChunk;
    private Vector2 noiseOffset;
    private Vector2 dangerNoiseOffset;
    
    private void Start()
    {
        // Random offset so each world is unique 
        noiseOffset = new Vector2(
            Random.Range(-10000f, 10000f),
            Random.Range(-10000f, 10000f)
        );

        dangerNoiseOffset = new Vector2(
            Random.Range(-10000f, 10000f),
            Random.Range(-10000f, 10000f)
        );
        
        currentPlayerChunk = Vector2Int.zero;
        UpdateLoadedChunks();
    }
    
    // Checks if player moved to different chunk 
    private void Update()
    {
        if (player == null)
        {
            return;
        }

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
    
    // Loads chunks close to the player and destroys terrain/objects that are too far away
    private void UpdateLoadedChunks()
    {
        // Checks X and Y around player and loads missing chunk 
        for (int y = -loadRadius; y <= loadRadius; y++)
        {
            for (int x = -loadRadius; x <= loadRadius; x++)
            {
                Vector2Int chunkCoordinate = currentPlayerChunk + new Vector2Int(x, y);
                
                if (!loadedTerrainChunks.ContainsKey(chunkCoordinate))
                {
                    LoadTerrainChunk(chunkCoordinate);
                }
                
                if (!loadedObjectChunks.ContainsKey(chunkCoordinate))
                {
                    LoadObjectChunk(chunkCoordinate);
                }
            }
        }
        
        List<Vector2Int> terrainChunksToUnload = new();
        List<Vector2Int> objectChunksToUnload = new();

        // Checks X and Y around player and marks terrain for removal if chunk is too far
        foreach (Vector2Int chunkCoordinate in loadedTerrainChunks.Keys)
        {
            int xDistance = Mathf.Abs(chunkCoordinate.x - currentPlayerChunk.x);
            int yDistance = Mathf.Abs(chunkCoordinate.y - currentPlayerChunk.y);
            
            if (xDistance > terrainUnloadRadius || yDistance > terrainUnloadRadius)
            {
                terrainChunksToUnload.Add(chunkCoordinate);
            }
        }

        // Checks X and Y around player and marks objects for removal if chunk is too far
        foreach (Vector2Int chunkCoordinate in loadedObjectChunks.Keys)
        {
            int xDistance = Mathf.Abs(chunkCoordinate.x - currentPlayerChunk.x);
            int yDistance = Mathf.Abs(chunkCoordinate.y - currentPlayerChunk.y);
            
            if (xDistance > objectUnloadRadius || yDistance > objectUnloadRadius)
            {
                objectChunksToUnload.Add(chunkCoordinate);
            }
        }
        
        foreach (Vector2Int chunkCoordinate in terrainChunksToUnload)
        {
            UnloadTerrainChunk(chunkCoordinate);
        }

        foreach (Vector2Int chunkCoordinate in objectChunksToUnload)
        {
            UnloadObjectChunk(chunkCoordinate);
        }
    }

    private void LoadTerrainChunk(Vector2Int chunkCoordinate)
    {
        Vector3 chunkCenter = GetChunkCenter(chunkCoordinate);
        GameObject terrainChunkObject = new GameObject();
        terrainChunkObject.name = $"Terrain Chunk {chunkCoordinate}";
        terrainChunkObject.transform.SetParent(transform);
        terrainChunkObject.transform.position = chunkCenter;
        
        loadedTerrainChunks.Add(chunkCoordinate, terrainChunkObject);
        CreateChunkCells(terrainChunkObject.transform, chunkCenter);
    }

    private void LoadObjectChunk(Vector2Int chunkCoordinate)
    {
        Vector3 chunkCenter = GetChunkCenter(chunkCoordinate);
        GameObject objectChunkObject = new GameObject();
        objectChunkObject.name = $"Object Chunk {chunkCoordinate}";
        objectChunkObject.transform.SetParent(transform);
        objectChunkObject.transform.position = chunkCenter;
        
        loadedObjectChunks.Add(chunkCoordinate, objectChunkObject);
        objectSpawner.GenerateChunk(objectChunkObject.transform, chunkCenter, chunkSize);
    }

    private void UnloadTerrainChunk(Vector2Int chunkCoordinate)
    {
        Destroy(loadedTerrainChunks[chunkCoordinate]);
        loadedTerrainChunks.Remove(chunkCoordinate);
    }
    
    private void UnloadObjectChunk(Vector2Int chunkCoordinate)
    {
        Destroy(loadedObjectChunks[chunkCoordinate]);
        loadedObjectChunks.Remove(chunkCoordinate);
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
                
                TerrainType terrainType = GetTerrainType(cellPosition);
                GameObject floorPrefab = GetFloorPrefab(terrainType);

                Instantiate(floorPrefab, cellPosition, Quaternion.identity, chunkParent);
            }
        }
    }

    private float EvaluateNoise(Vector2 worldPosition, Vector2 offset, float scale)
    {
        float sampleX = (worldPosition.x + offset.x) / scale;
        float sampleY = (worldPosition.y + offset.y) / scale;
        
        return Mathf.PerlinNoise(sampleX, sampleY);
    }

    private TerrainType GetTerrainType(Vector2 worldPosition)
    {
        TerrainType baseTerrainType = GetBaseTerrainType(worldPosition);
        TerrainType? dangerTerrainType = GetDangerTerrainType(worldPosition);
        
        if (dangerTerrainType.HasValue)
        {
            return dangerTerrainType.Value;
        }
        
        return baseTerrainType;
    }

    private TerrainType GetBaseTerrainType(Vector2 worldPosition)
    {
        float baseNoiseValue = EvaluateNoise(worldPosition, noiseOffset, noiseScale);
        
        if (baseNoiseValue <= earthMax) 
        {
            return TerrainType.Earth;
        }
        
        return TerrainType.Tech;
    }

    private TerrainType? GetDangerTerrainType(Vector2 worldPosition)
    {
        float dangerNoiseValue = EvaluateNoise(worldPosition, dangerNoiseOffset, dangerNoiseScale);
        
        if (dangerNoiseValue <= deathMax)
        {
            return TerrainType.Death;
        }
        
        if (dangerNoiseValue <= hazardMax)
        {
            return TerrainType.Hazard;
        }
        
        return null;
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
        return GetTerrainType(worldPosition) == TerrainType.Death;
    }
    
    public bool IsHazardTerrain(Vector2 worldPosition)
    {
        return GetTerrainType(worldPosition) == TerrainType.Hazard;
    }
    
    public bool IsEarthTerrain(Vector2 worldPosition)
    {
        return GetTerrainType(worldPosition) == TerrainType.Earth;
    }
    
    public bool IsTechTerrain(Vector2 worldPosition)
    {
        return GetTerrainType(worldPosition) == TerrainType.Tech;
    }
}
