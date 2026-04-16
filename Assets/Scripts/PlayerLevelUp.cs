using System;
using UnityEngine;

public class PlayerLevelUp : MonoBehaviour
{
    private const float BaseXp = 50f;
    private const float GrowthPerLevel = 25f;

    private int _currentLevel;
    private float _currentXp;
    private float _XpLevelTarget;

    // subscribe to this event to be notified of level ups
    public event EventHandler OnLevelUp;

    private void Awake()
    {
        _currentLevel = 1;
        _currentXp = 0f;
        _XpLevelTarget = BaseXp + GrowthPerLevel * (_currentLevel - 1);
    }

    public void AddXp(float xpAmount)
    {
        // add xp
        _currentXp += xpAmount;

        // check for level up
        if (_currentXp >= _XpLevelTarget)
        {
            // send overshot xp through
            LevelUp(_currentXp - _XpLevelTarget);
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
            _XpLevelTarget = BaseXp + GrowthPerLevel * (_currentLevel - 1);
            OnLevelUp?.Invoke(this, EventArgs.Empty);

            // check for multiple level ups
            if (_currentXp >= _XpLevelTarget)
            {
                // level up again if we're still above requirement
                overflowXp = _currentXp - _XpLevelTarget;
                continue;
            }

            break;
        }
    }
}
