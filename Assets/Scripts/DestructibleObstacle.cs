using UnityEngine;

public class DestructibleObstacle : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private SoundDefinition hurtSound;

    [Header("Drops")]
    [SerializeField] private GameObject coinDropPrefab;
    [SerializeField] private GameObject foodDropPrefab;
    [SerializeField, Range(0f, 1f)] private float coinDropChance = 0.5f;
    [SerializeField, Range(0f, 1f)] private float foodDropChance = 0.25f;

    private int _currentHealth;

    private void Awake()
    {
        _currentHealth = maxHealth;
    }

    public void TakeDamage(int damageTaken)
    {
        if (damageTaken <= 0)
        {
            return;
        }

        _currentHealth -= damageTaken;
        _currentHealth = Mathf.Max(_currentHealth, 0);
        SoundManager.Play(hurtSound);

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        SpawnDrop();
        Destroy(gameObject);
    }

    private void SpawnDrop()
    {
        var roll = Random.value;

        if (coinDropPrefab != null && roll < coinDropChance)
        {
            Instantiate(coinDropPrefab, transform.position, coinDropPrefab.transform.rotation);
            return;
        }

        if (foodDropPrefab != null && roll < coinDropChance + foodDropChance)
        {
            Instantiate(foodDropPrefab, transform.position, foodDropPrefab.transform.rotation);
        }
    }
}
