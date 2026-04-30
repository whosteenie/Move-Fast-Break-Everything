using System;
using UnityEngine;

public class EnemyLevelUp : MonoBehaviour
{

    private float currentLevel = 1;

    public event EventHandler OnLevelUp;

    private void Start()
    {


        int targetLevel = GameManager.CurrentEnemyLevel;

        for (int i = 1; i < targetLevel; i++)
        {
            LevelUp(1);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEnemyLevelChanged += HandleEnemyLevelChanged;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEnemyLevelChanged -= HandleEnemyLevelChanged;
        }
    }

    private void HandleEnemyLevelChanged(object sender, EventArgs e)
    {
        LevelUp(1);
    }


    public void LevelUp(float levelAmount)
    {
        // add level
        currentLevel += levelAmount;

        // check for level up
        Debug.Log($"Enemy leveled up! Current level: {currentLevel}", this);
        OnLevelUp?.Invoke(this, EventArgs.Empty);
    }

}