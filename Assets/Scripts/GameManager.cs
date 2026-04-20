using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour {
    [SerializeField] private UIDocument uiDocument;

    private const string RunTimerLabelName = "run-timer-label";
    private const string LevelProgressFillName = "level-progress-fill";

    private Label _runTimerLabel;
    private VisualElement _levelProgressFill;
    private PlayerLevelUp _playerLevelUp;
    private float _currentRunTime;

    public static GameManager Instance { get; private set; }
    public static float CurrentRunTimeSeconds => Instance != null ? Instance._currentRunTime : 0f;

    private void Awake() {
        Instance = this;

        if(uiDocument == null) {
            uiDocument = GetComponent<UIDocument>();
        }
    }

    private void OnDestroy() {
        if (_playerLevelUp != null)
        {
            _playerLevelUp.OnXpChanged -= HandleXpChanged;
        }

        if(Instance == this) {
            Instance = null;
        }
    }

    private void Start() {
        if(uiDocument == null) return;

        var root = uiDocument.rootVisualElement;
        _runTimerLabel = root.Q<Label>(RunTimerLabelName);
        _levelProgressFill = root.Q<VisualElement>(LevelProgressFillName);

        _playerLevelUp = FindAnyObjectByType<PlayerLevelUp>();
        if (_playerLevelUp != null)
        {
            _playerLevelUp.OnXpChanged += HandleXpChanged;
        }

        _runTimerLabel.text = FormatRunTime(_currentRunTime);
        RefreshLevelProgressBar();
    }

    private void Update() {
        _currentRunTime += Time.deltaTime;

        if(_runTimerLabel != null) {
            _runTimerLabel.text = FormatRunTime(_currentRunTime);
        }
    }

    private void HandleXpChanged(object sender, System.EventArgs e)
    {
        RefreshLevelProgressBar();
    }

    private void RefreshLevelProgressBar()
    {
        if (_levelProgressFill == null)
        {
            return;
        }

        if (_playerLevelUp == null || _playerLevelUp.XpLevelTarget <= 0f)
        {
            _levelProgressFill.style.width = Length.Percent(0f);
            return;
        }

        var progress = _playerLevelUp.CurrentXp / _playerLevelUp.XpLevelTarget;
        _levelProgressFill.style.width = Length.Percent(progress * 100f);
    }

    private static string FormatRunTime(float runTimeSeconds) {
        var totalSeconds = Mathf.FloorToInt(runTimeSeconds);
        var minutes = totalSeconds / 60;
        var seconds = totalSeconds % 60;

        return $"{minutes:00}:{seconds:00}";
    }
}
