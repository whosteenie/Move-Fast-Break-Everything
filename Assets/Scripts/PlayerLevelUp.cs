using System;
using UnityEngine;

public class PlayerLevelUp : MonoBehaviour
{
    [SerializeField] private float baseXp = 50f;
    [SerializeField] private float growthPerLevel = 25f;
    [SerializeField] private float debugXpAmount = 25f;

    private int _currentLevel;
    private float _currentXp;
    private float _xpLevelTarget;

    public int CurrentLevel => _currentLevel;
    public float CurrentXp => _currentXp;
    public float XpLevelTarget => _xpLevelTarget;

    public event EventHandler OnLevelUp;
    public event EventHandler OnXpChanged;

    private void Awake()
    {
        _currentLevel = 1;
        _currentXp = 0f;
        _xpLevelTarget = GetXpTargetForLevel(_currentLevel);

        Debug.Log($"Starting at level {_currentLevel} with {_currentXp} XP. Next level at {_xpLevelTarget} XP.");
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.X))
        {
            return;
        }

        AddXp(debugXpAmount);
        Debug.Log($"Debug XP added: {debugXpAmount}. Level {_currentLevel}, XP {_currentXp}/{_xpLevelTarget}", this);
    }

    public void AddXp(float xpAmount)
    {
        if (xpAmount <= 0f)
        {
            return;
        }

        _currentXp += xpAmount;

        while (_currentXp >= _xpLevelTarget)
        {
            var overflowXp = _currentXp - _xpLevelTarget;
            _currentLevel++;
            _currentXp = overflowXp;
            _xpLevelTarget = GetXpTargetForLevel(_currentLevel);
            OnLevelUp?.Invoke(this, EventArgs.Empty);
        }

        OnXpChanged?.Invoke(this, EventArgs.Empty);
    }

    private float GetXpTargetForLevel(int level)
    {
        return baseXp + growthPerLevel * (level - 1);
    }
}
