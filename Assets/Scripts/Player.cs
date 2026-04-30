using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Player : MonoBehaviour
{
    private Stats stats;
    [SerializeField] private SpriteRenderer visualSpriteRenderer;
    private Color _originalColor;


    private PlayerHealthBar _healthBar;
    public int maxHealth = 10;

    
   


    [SerializeField] private float invulnerabilityDuration = 1f;
    [SerializeField] private float flashInterval = 0.1f;
    [SerializeField] private float flashAlpha = 0.35f;
    [SerializeField] private SoundDefinition hurtSound;

    private const int debugHealAmount = 10;
    private bool _isInvulnerable;
    private Coroutine _invulnerabilityRoutine;
    public bool IsDead { get; private set; }

    public event Action<int, int> OnHealthChanged;

    public int CurrentHealth { get; private set; }

    public int MaxHealth => maxHealth;

    private void Start()
    {
        if (stats != null)
        {
            maxHealth = stats.GetMaxHealth();
        }

        CurrentHealth = maxHealth;
        EnsureHealthBar();
        NotifyHealthChanged();
    }

    private void Awake()
    {
        GetComponent<AutoAim>().enabled = true;
        GetComponentInChildren<Melee>().enabled = false;
        GetComponent<Circle>().enabled = false;
      

        stats = GetComponent<Stats>();
        if (visualSpriteRenderer != null)
        {
            _originalColor = visualSpriteRenderer.color;
        }
        EnsureHealthBar();
    }

    public void Update()
    {


    }

    public void UpdateMaxHealth(int newMaxHealth)
    {
        maxHealth = newMaxHealth;
        CurrentHealth = maxHealth;

        if (CurrentHealth > maxHealth)
        {
            CurrentHealth = maxHealth;
        }
        Debug.Log("Max HP: " + maxHealth + " | Current HP: " + CurrentHealth);
        NotifyHealthChanged();
    }

    public void Heal(int healAmount)
    {

        CurrentHealth += healAmount;
        if (CurrentHealth > maxHealth)
        {
            CurrentHealth = maxHealth;
        }
        Debug.Log("Player HP: " + CurrentHealth);
        NotifyHealthChanged();
    }



    public int TakeDamage(int damageTaken)
    {
        if (_isInvulnerable)
        {
            return 0;
        }

        int finalDamage = damageTaken;

        if (stats != null)
        {
            finalDamage = stats.CalculateDamageTaken(damageTaken);
        }
        CurrentHealth -= finalDamage;
        CurrentHealth = Mathf.Max(CurrentHealth, 0);
        SoundManager.Play(hurtSound);
        Debug.Log($"Player took {finalDamage} damage. HP: {CurrentHealth}");
        NotifyHealthChanged();

        int thornsDamage = stats != null ? stats.GetThornsDamage(damageTaken) : 0;

        if (CurrentHealth <= 0)
        {
            Die();
            return finalDamage;
        }

        if (_invulnerabilityRoutine != null)
        {
            StopCoroutine(_invulnerabilityRoutine);
        }

        _invulnerabilityRoutine = StartCoroutine(InvulnerabilityFlashRoutine());
        return finalDamage;
    }

    private void Die()
    {
        IsDead = true;
        RestoreSpriteColors();
        GameManager.Instance.ShowGameOver();
        Destroy(gameObject);
    }

    private IEnumerator InvulnerabilityFlashRoutine()
    {
        _isInvulnerable = true;
        var timer = 0f;
        var useFlashColor = true;

        while (timer < invulnerabilityDuration)
        {
            SetSpriteAlpha(useFlashColor ? flashAlpha : 1f);
            useFlashColor = !useFlashColor;
            yield return new WaitForSeconds(flashInterval);
            timer += flashInterval;
        }

        RestoreSpriteColors();
        _isInvulnerable = false;
        _invulnerabilityRoutine = null;
    }

    private void SetSpriteAlpha(float alpha)
    {
        if (visualSpriteRenderer == null)
        {
            return;
        }

        var color = _originalColor;
        color.a = alpha;
        visualSpriteRenderer.color = color;
    }

    private void RestoreSpriteColors()
    {
        if (visualSpriteRenderer == null)
        {
            return;
        }

        visualSpriteRenderer.color = _originalColor;
    }
    private void EnsureHealthBar()
    {
        if (_healthBar == null)
        {
            _healthBar = GetComponentInChildren<PlayerHealthBar>(true);
        }

        if (_healthBar != null)
        {
            _healthBar.Initialize(this);
            return;
        }

        GameObject healthBarObject = new("PlayerHealthBar");
        healthBarObject.transform.SetParent(transform, false);
        _healthBar = healthBarObject.AddComponent<PlayerHealthBar>();
        _healthBar.Initialize(this);
    }

    private void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        if(_healthBar != null) _healthBar.Refresh(CurrentHealth, maxHealth);
    }

}
