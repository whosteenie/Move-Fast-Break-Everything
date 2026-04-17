using System;
using UnityEngine;

public class Stats : MonoBehaviour
{

    public int speedMultiplier = 5;
    public int damageMultiplier = 2;
    private PlayerLevelUp levelSytem;

    private void Awake()
    {
        levelSytem = GetComponent<PlayerLevelUp>();
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
        Debug.Log("Speed Multiplier: " + speedMultiplier);
        Debug.Log("Damage Multiplier: " + damageMultiplier);
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
