using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Stats stats;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    public int maxHealth = 10;
    private int currentHealth;

    [SerializeField] private float invulnerabilityDuration = 1f;
    [SerializeField] private float flashInterval = 0.1f;
    [SerializeField] private float flashAlpha = 0.35f;

    private const int debugHealAmount = 10;
    private bool isInvulnerable;
    private Coroutine invulnerabilityRoutine;

    private void Start()
    {
        if (stats != null)
        {
            maxHealth = stats.GetMaxHealth();
        }

        currentHealth = maxHealth;
    }

    private void Awake()
    {
        stats = GetComponent<Stats>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    public void Update()
    {


    }


    public void UpdateMaxHealth(int newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = maxHealth;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        Debug.Log("Max HP: " + maxHealth + " | Current HP: " + currentHealth);
    }

    public void Heal(int healAmount)
    {

        currentHealth += healAmount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        Debug.Log("Player HP: " + currentHealth);
    }



    public int TakeDamage(int damageTaken)
    {
        if (isInvulnerable)
        {
            return 0;
        }

        int finalDamage = damageTaken;

        if (stats != null)
        {
            finalDamage = stats.CalculateDamageTaken(damageTaken);
        }
        currentHealth -= finalDamage;
        Debug.Log($"Player took {finalDamage} damage. HP: {currentHealth}");

        int thornsDamage = stats != null ? stats.GetThornsDamage(damageTaken) : 0;

        if (currentHealth <= 0)
        {
            Die();
            return finalDamage;
        }

        if (invulnerabilityRoutine != null)
        {
            StopCoroutine(invulnerabilityRoutine);
        }

        invulnerabilityRoutine = StartCoroutine(InvulnerabilityFlashRoutine());

        return finalDamage;
    }

    private void Die()
    {
        RestoreSpriteColors();
        GameManager.Instance.ShowGameOver();
        Destroy(gameObject);
    }

    private IEnumerator InvulnerabilityFlashRoutine()
    {
        isInvulnerable = true;
        float timer = 0f;
        bool useFlashColor = true;

        while (timer < invulnerabilityDuration)
        {
            SetSpriteAlpha(useFlashColor ? flashAlpha : 1f);
            useFlashColor = !useFlashColor;
            yield return new WaitForSeconds(flashInterval);
            timer += flashInterval;
        }

        RestoreSpriteColors();
        isInvulnerable = false;
        invulnerabilityRoutine = null;
    }

    private void SetSpriteAlpha(float alpha)
    {
        if (spriteRenderer == null)
        {
            return;
        }

        Color color = originalColor;
        color.a = alpha;
        spriteRenderer.color = color;
    }

    private void RestoreSpriteColors()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.color = originalColor;
    }

}
