using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour {
    [SerializeField] private UIDocument uiDocument;

    private const string RunTimerLabelName = "run-timer-label";
    private const string LevelProgressFillName = "level-progress-fill";
    private const string LevelUpRootName = "level-up-root";
    private const string StrengthButtonName = "strength-button";
    private const string DexterityButtonName = "dexterity-button";
    private const string IntelligenceButtonName = "intelligence-button";
    private const string GameOverRootName = "game-over-root";
    private const string RetryButtonName = "retry-button";
    private const string QuitButtonName = "quit-button";

    private Label _runTimerLabel;
    private VisualElement _levelProgressFill;
    private VisualElement _levelUpRoot;
    private PlayerLevelUp _playerLevelUp;
    private VisualElement _gameOverRoot;
    private float _currentRunTime;
    private bool _isGameOver;

    public static GameManager Instance { get; private set; }
    public static float CurrentRunTimeSeconds => Instance != null ? Instance._currentRunTime : 0f;

    private void Awake() {
        Instance = this;

        if(uiDocument == null) {
            uiDocument = GetComponent<UIDocument>();
        }
    }

    private void OnDestroy() {
        Time.timeScale = 1f;

        if (_playerLevelUp != null)
        {
            _playerLevelUp.OnXpChanged -= HandleXpChanged;
            _playerLevelUp.OnLevelUp -= HandleLevelUp;
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
        _levelUpRoot = root.Q<VisualElement>(LevelUpRootName);
        _gameOverRoot = root.Q<VisualElement>(GameOverRootName);

        var strengthButton = root.Q<Button>(StrengthButtonName);
        var dexterityButton = root.Q<Button>(DexterityButtonName);
        var intelligenceButton = root.Q<Button>(IntelligenceButtonName);
        var retryButton = root.Q<Button>(RetryButtonName);
        var quitButton = root.Q<Button>(QuitButtonName);

        if (strengthButton != null) strengthButton.clicked += () => ResolveLevelUpChoice("strength");
        if (dexterityButton != null) dexterityButton.clicked += () => ResolveLevelUpChoice("dexterity");
        if (intelligenceButton != null) intelligenceButton.clicked += () => ResolveLevelUpChoice("intelligence");
        if (retryButton != null) retryButton.clicked += RetryRun;
        if (quitButton != null) quitButton.clicked += QuitToMenu;

        _playerLevelUp = FindObjectOfType<PlayerLevelUp>();
        if (_playerLevelUp != null)
        {
            _playerLevelUp.OnXpChanged += HandleXpChanged;
            _playerLevelUp.OnLevelUp += HandleLevelUp;
        }

        if (_runTimerLabel != null)
        {
            _runTimerLabel.text = FormatRunTime(_currentRunTime);
        }

        if (_levelUpRoot != null)
        {
            _levelUpRoot.style.display = DisplayStyle.None;
        }

        if (_gameOverRoot != null)
        {
            _gameOverRoot.style.display = DisplayStyle.None;
        }

        RefreshLevelProgressBar();
    }

    private void Update() {
        if(_isGameOver) {
            return;
        }

        _currentRunTime += Time.deltaTime;

        if(_runTimerLabel != null) {
            _runTimerLabel.text = FormatRunTime(_currentRunTime);
        }
    }

    private void HandleXpChanged(object sender, EventArgs e)
    {
        RefreshLevelProgressBar();
    }

    private void HandleLevelUp(object sender, EventArgs e)
    {
        Time.timeScale = 0f;
        if (_levelUpRoot != null)
        {
            _levelUpRoot.style.display = DisplayStyle.Flex;
        }
    }

    private void ResolveLevelUpChoice(string choiceId)
    {
        if (_playerLevelUp == null)
        {
            return;
        }

        Debug.Log($"Level up choice selected: {choiceId}", this);
        if (_levelUpRoot != null)
        {
            _levelUpRoot.style.display = DisplayStyle.None;
        }
        Time.timeScale = 1f;
        _playerLevelUp.ResolveLevelUpChoice();
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

    public void ShowGameOver() {
        if(_isGameOver) return;

        _isGameOver = true;
        if (_gameOverRoot != null)
        {
            _gameOverRoot.style.display = DisplayStyle.Flex;
        }
    }

    private static void RetryRun() {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }

    private static void QuitToMenu() {
        Debug.LogWarning("Not yet implemented");
    }

    private static string FormatRunTime(float runTimeSeconds) {
        var totalSeconds = Mathf.FloorToInt(runTimeSeconds);
        var minutes = totalSeconds / 60;
        var seconds = totalSeconds % 60;

        return $"{minutes:00}:{seconds:00}";
    }
}
