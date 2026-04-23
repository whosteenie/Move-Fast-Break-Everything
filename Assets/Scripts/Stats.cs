using UnityEngine;

public class Stats : MonoBehaviour
{
    private const float StrengthHealthIncrease = 0.1f;
    private const float DexterityFireRateIncrease = 0.1f;
    private const float IntelligenceDamageIncrease = 0.1f;

    public float speedMultiplier = 1f;
    public float damageMultiplier = 1f;
    public float maxHealthStat = 1f;
    public float dexterityMultiplier = 1f;

    private Player player;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void IncreaseDexterity(float percent)
    {
        dexterityMultiplier += percent;
    }

    public void IncreaseSpeed(float percent)
    {
        speedMultiplier += percent;
    }

    public void IncreaseDamage(float percent)
    {
        damageMultiplier += percent;
    }

    public void IncreaseHealth(float percent)
    {
        maxHealthStat += percent;
    }

    public float GetSpeed(float baseSpeed)
    {
        return baseSpeed * speedMultiplier;
    }

    public int GetDamage(int baseDamage)
    {
        return Mathf.RoundToInt(baseDamage * damageMultiplier);
    }

    public int GetMaxHealth()
    {
        int baseHealth = 10;
        return Mathf.RoundToInt(baseHealth * maxHealthStat);
    }

    public float GetFireRate(float baseFireRate)
    {
        return baseFireRate * dexterityMultiplier;
    }

    public void ApplyLevelUpChoice(string choiceId)
    {
        switch (choiceId)
        {
            case "strength":
                IncreaseHealth(StrengthHealthIncrease);
                if (player != null)
                {
                    player.UpdateMaxHealth(GetMaxHealth());
                }
                Debug.Log($"Strength selected. Max Health: {GetMaxHealth()}", this);
                break;
            case "dexterity":
                IncreaseDexterity(DexterityFireRateIncrease);
                Debug.Log($"Dexterity selected. Fire Rate Multiplier: {dexterityMultiplier}", this);
                break;
            case "intelligence":
                IncreaseDamage(IntelligenceDamageIncrease);
                Debug.Log($"Intelligence selected. Damage Multiplier: {damageMultiplier}", this);
                break;
            default:
                Debug.LogWarning($"Unknown level up choice: {choiceId}", this);
                break;
        }
    }
}
