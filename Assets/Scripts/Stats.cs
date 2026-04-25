using UnityEngine;

public class Stats : MonoBehaviour
{
    private const float healthIncrease = 0.5f;
    private const float DexterityFireRateIncrease = 0.5f;
    private const float rangeDamageIncrease = 0.7f;

    public float speedMultiplier = 0.2f;
    public float rangedDamageMultiplier = 1f;

    public float defense = 0f;


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
    //health stats____________________________________________________________________________
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


    //Dex stats____________________________________________________________________________
    public void IncreaseDexterity(float percent)
    {
        dexterityMultiplier += percent;
    }

    public float GetFireRate(float baseFireRate)
    {
        return baseFireRate * dexterityMultiplier;
    }
    //Speed Stats____________________________________________________________________________

    public void IncreaseSpeed(float percent)
    {
        speedMultiplier += percent;
    }

    public float GetSpeed(float baseSpeed)
    {
        return baseSpeed * speedMultiplier;
    }
    //Range DMG stats____________________________________________________________________________
    public void IncreaseRangedDamage(float percent)
    {
        rangedDamageMultiplier += percent;
    }

    public int GetDamage(int baseDamage)
    {
        return Mathf.RoundToInt(baseDamage * rangedDamageMultiplier);
    }

    //Defense stats__________________________________________________________________________________________
    public void IncreaseDefense(float percent)
    {
        defense += percent;
    }

    public int CalculateDamageTaken(int incomingDamage)
    {
        float reduceDamage = incomingDamage * (1f / (1f + defense));
        return Mathf.Max(1, Mathf.RoundToInt(reduceDamage));
    }


    public void ApplyLevelUpChoice(string choiceId)
    {
        switch (choiceId)
        {
            case "strength":
                IncreaseHealthPercent(healthIncrease);
                if (player != null)
                {
                    player.UpdateMaxHealth(GetMaxHealth());
                }
                Debug.Log($"Health selected. Max Health: {GetMaxHealth()}", this);
                break;
            case "dexterity":
                IncreaseDexterity(DexterityFireRateIncrease);
                Debug.Log($"Dexterity selected. Fire Rate Multiplier: {dexterityMultiplier}", this);
                break;
            case "intelligence":
                IncreaseRangedDamage(rangeDamageIncrease);
                IncreaseSpeed(speedMultiplier);
                Debug.Log($"Agility selected.  Ranged Damage Multiplier: {rangedDamageMultiplier}", this);
                Debug.Log($"Agility selected.Speed  Multiplier: {speedMultiplier}", this);
                break;
            case "defense":
                IncreaseDefense(0.5f);
                Debug.Log($"Deffense selected. New defense at: {defense}", this);
                break;
            default:
                Debug.LogWarning($"Unknown level up choice: {choiceId}", this);
                break;
        }
    }
}
