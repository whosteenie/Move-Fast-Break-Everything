using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Stats stats;
    private SpeedTower speedTower;
    private PlayerHealthBar healthBar;
    public int maxHealth = 10;
    private int currentHealth;

    [Header("Health Bar")]
    [SerializeField] private Vector2 healthBarSize = new(1.4f, 0.18f);
    [SerializeField] private bool autoPositionHealthBar = true;
    [SerializeField] private Vector3 healthBarOffset = new(0f, -0.85f, 0f);
    [SerializeField] private Color healthBarFillColor = new(0.2f, 0.9f, 0.3f, 1f);
    [SerializeField] private Color healthBarBackgroundColor = new(0.1f, 0.1f, 0.1f, 0.85f);

    private const int debugHealAmount = 10;

    public event Action<int, int> OnHealthChanged;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private void Start()
    {
        if (stats != null)
        {
            maxHealth = stats.maxHealthStat;
        }

        currentHealth = maxHealth;
        EnsureHealthBar();
        NotifyHealthChanged();
    }

    private void Awake()
    {
        stats = GetComponent<Stats>();
        EnsureHealthBar();
    }

    public void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Q))
        {
            return;
        }

        Heal(debugHealAmount);

    }


    public void UpdateMaxHealth(int newMaxHealth)
    {
        maxHealth = newMaxHealth;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        Debug.Log("Max HP: " + maxHealth + " | Current HP: " + currentHealth);
        NotifyHealthChanged();
    }

    public void Heal(int healAmount)
    {

        currentHealth += healAmount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        Debug.Log("Player HP: " + currentHealth);
        NotifyHealthChanged();
    }



    public void TakeDamage(int damageTaken)
    {

        currentHealth -= damageTaken;
        currentHealth = Mathf.Max(currentHealth, 0);
        Debug.Log("Player HP: " + currentHealth);
        NotifyHealthChanged();
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        GameManager.Instance.ShowGameOver();
        Destroy(gameObject);
    }

    private void EnsureHealthBar()
    {
        if (healthBar == null)
        {
            healthBar = GetComponentInChildren<PlayerHealthBar>();
        }

        if (healthBar != null)
        {
            healthBar.Configure(healthBarSize, GetResolvedHealthBarOffset(), healthBarFillColor, healthBarBackgroundColor);
            return;
        }

        GameObject healthBarObject = new("PlayerHealthBar");
        healthBarObject.transform.SetParent(transform, false);
        healthBar = healthBarObject.AddComponent<PlayerHealthBar>();
        healthBar.Configure(healthBarSize, GetResolvedHealthBarOffset(), healthBarFillColor, healthBarBackgroundColor);
    }

    private void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        healthBar?.Refresh(currentHealth, maxHealth);
    }

    private Vector3 GetResolvedHealthBarOffset()
    {
        if (!autoPositionHealthBar)
        {
            return healthBarOffset;
        }

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            return transform.InverseTransformPoint(new Vector3(
                transform.position.x,
                spriteRenderer.bounds.min.y - (healthBarSize.y * 1.5f),
                transform.position.z));
        }

        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            return transform.InverseTransformPoint(new Vector3(
                transform.position.x,
                playerCollider.bounds.min.y - (healthBarSize.y * 1.5f),
                transform.position.z));
        }

        return healthBarOffset;
    }

}
