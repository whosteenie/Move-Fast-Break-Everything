using System;

public static class PlayerLevelUp
{
    private static int _playerLevel = 1;
    private static float _playerXp;
    private static float _playerMaxXp = LevelScale * _playerLevel;
    private const float LevelScale = 50f;

    public static event EventHandler OnLevelUp;

    public static void AddXp(float xp)
    {
        // add xp
        _playerXp += xp;

        // check for level up
        if (_playerXp >= _playerMaxXp)
        {
            // send overshot xp through
            LevelUp(_playerXp - _playerMaxXp);
        }
    }

    private static void LevelUp(float remainingXp)
    {
        while (true)
        {
            // level up and set remainder xp
            _playerLevel++;
            _playerXp = remainingXp;

            // change xp target, currently linear
            _playerMaxXp = _playerLevel * LevelScale;
            OnLevelUp?.Invoke(null, EventArgs.Empty);

            // check for multiple level ups
            if (_playerXp >= _playerMaxXp)
            {
                // level up again if we're still above requirement
                remainingXp = _playerXp - _playerMaxXp;
                continue;
            }

            break;
        }
    }
}