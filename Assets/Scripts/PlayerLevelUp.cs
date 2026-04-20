using System;
using UnityEngine;

public class PlayerLevelUp : MonoBehaviour
{
    private const float BaseXp = 50f;
    private const float GrowthPerLevel = 25f;
    private const float DebugXpAmount = 25f;

    private int _currentLevel;
    private float _currentXp;
    private float _xpLevelTarget;

    // subscribe to this event to be notified of level ups
    public event EventHandler OnLevelUp;

    private void Awake()
    {
        _currentLevel = 1;
        _currentXp = 0f;
        _xpLevelTarget = BaseXp + GrowthPerLevel * (_currentLevel - 1);

        Debug.Log($"Starting at level {_currentLevel} with {_currentXp} XP. Next level at {_xpLevelTarget} XP.");
    }

    // debug input to add xp, replace with actual xp gain from gameplay
    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.X))
        {

            return;
        }

        AddXp(DebugXpAmount);
        Debug.Log($"Debug XP added: {DebugXpAmount}. Level {_currentLevel}, XP {_currentXp}/{_xpLevelTarget}", this);
    }

    public void AddXp(float xpAmount)
    {
        // add xp
        _currentXp += xpAmount;

        // check for level up
        if (_currentXp >= _xpLevelTarget)
        {
            // send overshot xp through
            LevelUp(_currentXp - _xpLevelTarget);
        }
    }

    private void LevelUp(float overflowXp)
    {
        while (true)
        {
            // level up and set remainder xp
            _currentLevel++;
            _currentXp = overflowXp;

            // change xp target, currently linear
            _xpLevelTarget = BaseXp + GrowthPerLevel * (_currentLevel - 1);
            OnLevelUp?.Invoke(this, EventArgs.Empty);

            // check for multiple level ups
            if (_currentXp >= _xpLevelTarget)
            {
                // level up again if we're still above requirement
                overflowXp = _currentXp - _xpLevelTarget;
                continue;
            }

            break;
        }
    }
}