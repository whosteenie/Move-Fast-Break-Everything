using UnityEngine;

public class Stats : MonoBehaviour
{
    private const float healthIncrease = 0.5f;
    private const float DexterityFireRateIncrease = 0.5f;
    private const float rangeDamageIncrease = 0.7f;
    private const float MightDamageIncreasePerRank = 0.05f;
    private const float MaxHealthIncreasePerRank = 0.1f;
    private const float HasteFireRateIncreasePerRank = 0.05f;
    private const float MoveSpeedIncreasePerRank = 0.04f;

    public float speedMultiplier = 0.2f;
    public float rangedDamageMultiplier = 1f;
    public float damageMultiplier => rangedDamageMultiplier;

    public float defense = .2f;
    public float thorns = 0.2f;
    public float pirece = .2f;

    public int baseHealth = 10;
    public int flatHealthBonus = 0;
    public float healthMultiplier = 1f;

    public float dexterityMultiplier = 1f;

    private Player player;
    private PlayerLevelUp levelSytem;

    private void Awake()
    {
        player = GetComponent<Player>();
        levelSytem = GetComponent<PlayerLevelUp>();
        ApplyPurchasedPowerUps();
    }

    private void OnEnable()
    {
        if (levelSytem != null)
        {
            levelSytem.OnLevelUp += OnStatChange;
        }
    }

    private void OnDisable()
    {
        if (levelSytem != null)
        {
            levelSytem.OnLevelUp -= OnStatChange;
        }
    }

    private void OnStatChange(object sender, System.EventArgs e)
    {
        IncreaseSpeed(0.1f);
        IncreaseRangedDamage(0.1f);
        IncreaseFlatHealth(2);
        IncreaseHealthPercent(0.1f);
        IncreaseDexterity(0.1f);
        IncreaseDefense(0.5f);

        if (player != null)
        {
            player.UpdateMaxHealth(GetMaxHealth());
        }
    }

    private void IncreaseFlatHealth(int amount)
    {
        flatHealthBonus += amount;
    }

    public void IncreaseHealthPercent(float percent)
    {
        healthMultiplier += percent;
    }

    public int GetMaxHealth()
    {
        return Mathf.RoundToInt((baseHealth + flatHealthBonus) * healthMultiplier);
    }

    public void IncreaseDexterity(float percent)
    {
        dexterityMultiplier += percent;
    }

    public float GetFireRate(float baseFireRate)
    {
        return baseFireRate * dexterityMultiplier;
    }

    public void IncreaseSpeed(float percent)
    {
        speedMultiplier += percent;
    }

    public float GetSpeed(float baseSpeed)
    {
        return baseSpeed * speedMultiplier;
    }

    public void IncreaseRangedDamage(float percent)
    {
        rangedDamageMultiplier += percent;
    }

    public int GetDamage(int baseDamage)
    {
        return Mathf.RoundToInt(baseDamage * rangedDamageMultiplier);
    }

    public void IncreaseDefense(float percent)
    {
        defense += percent;
    }

    private void ApplyPurchasedPowerUps()
    {
        var mightRank = ShopPowerUpProgress.GetRank("might");
        var maxHealthRank = ShopPowerUpProgress.GetRank("max_health");
        var hasteRank = ShopPowerUpProgress.GetRank("haste");
        var moveSpeedRank = ShopPowerUpProgress.GetRank("move_speed");

        rangedDamageMultiplier += mightRank * MightDamageIncreasePerRank;
        healthMultiplier += maxHealthRank * MaxHealthIncreasePerRank;
        dexterityMultiplier += hasteRank * HasteFireRateIncreasePerRank;
        speedMultiplier += moveSpeedRank * MoveSpeedIncreasePerRank;
    }

    public int CalculateDamageTaken(int incomingDamage)
    {
        float reduceDamage = incomingDamage * (1f / (1f + defense));
        return Mathf.Max(1, Mathf.RoundToInt(reduceDamage));
    }

    public void IncreasePierce(float percent)
    {
        pirece += percent;
    }

    public void IncreaseThorns(float percent)
    {
        thorns += percent;
    }

    public int GetThornsDamage(int incomingDamage)
    {
        return Mathf.RoundToInt(incomingDamage * thorns);
    }

    public float GetPierce()
    {
        return pirece;
    }

    public void ApplyLevelUpChoice(string choiceId)
    {
        switch (choiceId)
        {
            case "health":
                IncreaseHealthPercent(healthIncrease);
                if (player != null)
                {
                    player.UpdateMaxHealth(GetMaxHealth());
                }
                Debug.Log($"Health selected. Max Health: {GetMaxHealth()}", this);
                break;
            case "strength":
                IncreasePierce(pirece);
                IncreaseThorns(thorns);
                Debug.Log($"Strength selected. New pierce at: {pirece}", this);
                Debug.Log($"Strength selected. New thorns at: {thorns}", this);
                break;
            case "dexterity":
                IncreaseDexterity(DexterityFireRateIncrease);
                Debug.Log($"Dexterity selected. Fire Rate Multiplier: {dexterityMultiplier}", this);
                break;
            case "agility":
                IncreaseRangedDamage(rangeDamageIncrease);
                IncreaseSpeed(speedMultiplier);
                Debug.Log($"Agility selected. Ranged Damage Multiplier: {rangedDamageMultiplier}", this);
                Debug.Log($"Agility selected. Speed Multiplier: {speedMultiplier}", this);
                break;
            case "intelligence":
                IncreaseRangedDamage(rangeDamageIncrease);
                Debug.Log($"Intelligence selected. Ranged Damage Multiplier: {rangedDamageMultiplier}", this);
                break;
            case "defense":
                IncreaseDefense(0.5f);
                Debug.Log($"Defense selected. New defense at: {defense}", this);
                break;
            default:
                Debug.LogWarning($"Unknown level up choice: {choiceId}", this);
                break;
        }
    }
}
