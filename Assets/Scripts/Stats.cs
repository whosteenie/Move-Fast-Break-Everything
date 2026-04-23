using System;
using UnityEngine;

public class Stats : MonoBehaviour
{

    public float speedMultiplier = 1f;
    public float damageMultiplier = 1f;


    public int baseHealth = 10;
    public int flatHealthBonus = 0;
    public float healthMultiplier = 1f;

    public float dexterityMultiplier = 1f;
    private PlayerLevelUp levelSytem;
    private Player player;

    private void Awake()
    {
        levelSytem = GetComponent<PlayerLevelUp>();
        player = GetComponent<Player>();
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

    private void OnStatChange(object sender, EventArgs e)
    {
        IncreaseSpeed(0.1f);
        IncreaseDamage(0.1f);

        IncreaseFlatHealth(2);
        IncreaseHealthPercent(0.1f);

        IncreaseDexterity(0.1f);

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
        int baseHealth = 10;
        return Mathf.RoundToInt((baseHealth + flatHealthBonus) * healthMultiplier);
    }


    //Dex stats____________________________________________________________________________
    private void IncreaseDexterity(float percent)
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
    //DMG stats____________________________________________________________________________
    public void IncreaseDamage(float percent)
    {
        damageMultiplier += percent;
    }

    public int GetDamage(int baseDamage)
    {
        return Mathf.RoundToInt(baseDamage * damageMultiplier);
    }



}
