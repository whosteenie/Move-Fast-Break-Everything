using System.Collections;
using System;
using UnityEngine;

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
        stats = GetComponent<Stats>();
        if (visualSpriteRenderer != null)
        {
            _originalColor = visualSpriteRenderer.color;
        }
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



    public void TakeDamage(int damageTaken)
    {
        if (_isInvulnerable)
        {
            return;
        }

        CurrentHealth -= damageTaken;
        CurrentHealth = Mathf.Max(CurrentHealth, 0);
        SoundManager.Play(hurtSound);
        Debug.Log("Player HP: " + CurrentHealth);
        NotifyHealthChanged();
        if (CurrentHealth <= 0)
        {
            Die();
            return;
        }

        if (_invulnerabilityRoutine != null)
        {
            StopCoroutine(_invulnerabilityRoutine);
        }

        _invulnerabilityRoutine = StartCoroutine(InvulnerabilityFlashRoutine());
    }

    private void Die()
    {
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
