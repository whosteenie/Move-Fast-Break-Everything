using System;
using UnityEngine;

public class Stats : MonoBehaviour
{

    public float speedMultiplier = 1f;
    public float damageMultiplier = 1f;
    public float maxHealthStat = 1f;

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
        IncreaseHealth(0.1f);
        IncreaseDexterity(0.1f);

        if (player != null)
        {
            player.UpdateMaxHealth(GetMaxHealth());
        }

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


}
