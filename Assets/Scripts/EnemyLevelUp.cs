using System;
using UnityEngine;

public class EnemyLevelUp : MonoBehaviour
{

    private const float LevelUpamount = 1f;

    private float _currentLevel;


    public event EventHandler OnLevelUp;

    private void Awake()
    {
        _currentLevel = 1;

    }


    private void Update()
    {
        // TODO: Remove this debug code
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Debug.Log($"Current enemy level at {_currentLevel}", this);
            LevelUp(LevelUpamount);
        }


    }

    public void LevelUp(float levelAmount)
    {
        // add level
        _currentLevel += levelAmount;

        // check for level up
        Debug.Log($"Enemy leveled up! Current level: {_currentLevel}", this);
        OnLevelUp?.Invoke(this, EventArgs.Empty);
    }

}