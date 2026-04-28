using System;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [Header("Base Stats")]
    public float baseSpeed = 1f;
    public int baseDamage = 1;
    public int baseHealth = 10;
    public float baseDefense = .1f;

    [Header("Scaling Multipliers")]
    public float speedMult = 1f;
    public float damageMult = 1f;
    public float healthMult = 1f;
    public float defenseMult = 1f;

    [Header("Level Scaling")]
    public float speedIncreasePerLvl = 0.5f;
    public float damageIncreasePerLvl = 0.2f;
    public float healthIncreasePerLvl = 0.3f;
    public float defenseIncreaePerLvl = 0.3f;
    private EnemyLevelUp levelSystem;
    private Enemy enemy;

    private void Awake()
    {
        levelSystem = GetComponent<EnemyLevelUp>();
        enemy = GetComponent<Enemy>();
    }

    private void OnEnable()
    {
        if (levelSystem != null)
        {
            levelSystem.OnLevelUp += StatChange;
        }
    }

    private void OnDisable()
    {
        if (levelSystem != null)
        {
            levelSystem.OnLevelUp -= StatChange;
        }
    }

    private void StatChange(object sender, EventArgs e)
    {
        speedMult += speedIncreasePerLvl;
        damageMult += damageIncreasePerLvl;
        healthMult += healthIncreasePerLvl;
        defenseMult += defenseIncreaePerLvl;

        if (enemy != null)
        {
            enemy.UpdateMaxHealth(GetMaxHealth());
        }
        Debug.Log("Enemy Speed: " + GetSpeed());
        Debug.Log("Enemy Damage: " + GetDamage());
        Debug.Log("Enemy Health: " + GetMaxHealth());
        Debug.Log("Enemy Defense: " + GetDefense());
    }
    public float GetSpeed()
    {
        return baseSpeed * speedMult;
    }

    public int GetDamage()
    {
        return Mathf.RoundToInt(baseDamage * damageMult);
    }

    public int GetMaxHealth()
    {
        return Mathf.RoundToInt(baseHealth * healthMult);
    }

    public float GetDefense()
    {
        return baseDefense * defenseMult;
    }

    public int CalculateDamageTaken(int incomingDamage, float pierce)
    {
        float effectiveDefense = Mathf.Max(0f, GetDefense() - pierce);

        float reducedDamage =
            incomingDamage * (1f / (1f + effectiveDefense));

        return Mathf.Max(1, Mathf.RoundToInt(reducedDamage));
    }

}
