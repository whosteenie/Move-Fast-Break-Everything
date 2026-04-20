using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour {
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private int startingXpToNextLevel = 5;
    [SerializeField] private int xpRequirementIncreasePerLevel = 3;

    private const string RunTimerLabelName = "run-timer-label";
    private const string LevelProgressFillName = "level-progress-fill";

    private Label _runTimerLabel;
    private VisualElement _levelProgressFill;
    private float _currentRunTime;
    private int _currentLevel = 1;
    private int _currentXp;
    private int _xpToNextLevel;

    public static GameManager Instance { get; private set; }
    public static float CurrentRunTimeSeconds => Instance != null ? Instance._currentRunTime : 0f;

    private void Awake() {
        Instance = this;
        _xpToNextLevel = startingXpToNextLevel;

        if(uiDocument == null) {
            uiDocument = GetComponent<UIDocument>();
        }
    }

    private void OnDestroy() {
        if(Instance == this) {
            Instance = null;
        }
    }

    private void Start() {
        if(uiDocument == null) return;

        var root = uiDocument.rootVisualElement;
        _runTimerLabel = root.Q<Label>(RunTimerLabelName);
        _levelProgressFill = root.Q<VisualElement>(LevelProgressFillName);

        _runTimerLabel.text = FormatRunTime(_currentRunTime);
        RefreshLevelProgressBar();
    }

    private void Update() {
        _currentRunTime += Time.deltaTime;

        if(_runTimerLabel != null) {
            _runTimerLabel.text = FormatRunTime(_currentRunTime);
        }
    }

    public void AddExperience(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        _currentXp += amount;

        while (_currentXp >= _xpToNextLevel)
        {
            _currentXp -= _xpToNextLevel;
            _currentLevel++;
            _xpToNextLevel = GetXpRequiredForLevel(_currentLevel);
        }

        RefreshLevelProgressBar();
    }

    private void RefreshLevelProgressBar()
    {
        if (_levelProgressFill == null)
        {
            return;
        }

        var progress = _xpToNextLevel > 0 ? (float)_currentXp / _xpToNextLevel : 0f;
        _levelProgressFill.style.width = Length.Percent(progress * 100f);
    }

    private int GetXpRequiredForLevel(int level)
    {
        return startingXpToNextLevel + (level - 1) * xpRequirementIncreasePerLevel;
    }

    private static string FormatRunTime(float runTimeSeconds) {
        var totalSeconds = Mathf.FloorToInt(runTimeSeconds);
        var minutes = totalSeconds / 60;
        var seconds = totalSeconds % 60;

        return $"{minutes:00}:{seconds:00}";
    }
}
