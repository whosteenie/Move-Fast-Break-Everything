using UnityEngine;

public class TerrainDamage : MonoBehaviour
{
    [SerializeField] private ChunkManager chunkManager;
    [SerializeField] private Player player;
    [SerializeField] private Jump jump;
    [SerializeField] private int hazardDamage = 1;
    [SerializeField] private int deathDamage = 2;
    
    private void Awake()
    {
        player = GetComponent<Player>();
        jump = GetComponent<Jump>();
        chunkManager = FindAnyObjectByType<ChunkManager>();
    }
    
    private void Update()
    {
        if (jump.IsJumping)
        {
            return;
        }
        
        Vector2 currentPosition = transform.position;
        
        if (chunkManager.IsDeathTerrain(currentPosition))
        {
            player.TakeDamage(deathDamage);
            return;
        }
        
        if (chunkManager.IsHazardTerrain(currentPosition))
        {
            player.TakeDamage(hazardDamage);
        }
    }
}
