using System;
using UnityEngine;

public class Stats : MonoBehaviour
{

    public int speedMultiplier = 5;
    public int damageMultiplier = 2;
    public int maxHealthStat = 10;
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
            levelSytem.OnLevelUp += statChange;
        }

    }

    private void OnDisable()
    {
        if (levelSytem != null)
        {
            levelSytem.OnLevelUp -= statChange;
        }
    }

    private void statChange(object sender, EventArgs e)
    {
        IncreaseSpeedStat(1);
        IncreaseDamageStat(10);
        IncreaseHealthStat(10);

        if (player != null)
        {
            player.UpdateMaxHealth(maxHealthStat);
        }
        Debug.Log("Speed Multiplier: " + speedMultiplier);
        Debug.Log("Damage Multiplier: " + damageMultiplier);
        Debug.Log("Max health stat: " + maxHealthStat);
    }

    private void IncreaseHealthStat(int amount)
    {
        maxHealthStat += amount;
    }

    private void IncreaseSpeedStat(int amount)
    {
        speedMultiplier += amount;
    }

    private void IncreaseDamageStat(int amount)
    {
        damageMultiplier += amount;
    }
}
