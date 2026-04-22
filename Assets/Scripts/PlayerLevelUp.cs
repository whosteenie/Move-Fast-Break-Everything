using System;
using UnityEngine;

public class PlayerLevelUp : MonoBehaviour
{
    [SerializeField] private float baseXp = 50f;
    [SerializeField] private float growthPerLevel = 25f;
    [SerializeField] private float debugXpAmount = 25f;

    private float _bufferedXp;
    private bool _isAwaitingLevelUpChoice;

    private int CurrentLevel { get; set; }

    public float CurrentXp { get; private set; }

    public float XpLevelTarget { get; private set; }

    public event EventHandler OnLevelUp;
    public event EventHandler OnXpChanged;

    private void Awake()
    {
        CurrentLevel = 1;
        CurrentXp = 0f;
        XpLevelTarget = GetXpTargetForLevel(CurrentLevel);

        Debug.Log($"Starting at level {CurrentLevel} with {CurrentXp} XP. Next level at {XpLevelTarget} XP.");
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.X))
        {
            return;
        }

        AddXp(debugXpAmount);
        Debug.Log($"Debug XP added: {debugXpAmount}. Level {CurrentLevel}, XP {CurrentXp}/{XpLevelTarget}", this);
    }

    public void AddXp(float xpAmount)
    {
        if (xpAmount <= 0f)
        {
            return;
        }

        _bufferedXp += xpAmount;
        ProcessBufferedXp();
    }

    public void ResolveLevelUpChoice()
    {
        _isAwaitingLevelUpChoice = false;
        ProcessBufferedXp();
    }

    private void ProcessBufferedXp()
    {
        while (!_isAwaitingLevelUpChoice && _bufferedXp > 0f)
        {
            var xpNeededToLevel = XpLevelTarget - CurrentXp;
            var xpToApply = Mathf.Min(_bufferedXp, xpNeededToLevel);

            CurrentXp += xpToApply;
            _bufferedXp -= xpToApply;

            if(!(CurrentXp >= XpLevelTarget)) continue;
            CurrentLevel++;
            CurrentXp = 0f;
            XpLevelTarget = GetXpTargetForLevel(CurrentLevel);
            _isAwaitingLevelUpChoice = true;
            OnLevelUp?.Invoke(this, EventArgs.Empty);
            OnXpChanged?.Invoke(this, EventArgs.Empty);
            return;
        }

        OnXpChanged?.Invoke(this, EventArgs.Empty);
    }

    private float GetXpTargetForLevel(int level)
    {
        return baseXp + growthPerLevel * (level - 1);
    }
}
