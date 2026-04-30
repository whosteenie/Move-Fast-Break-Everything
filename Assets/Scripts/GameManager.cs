using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private InputActionAsset playerInputActions;
    [SerializeField] private Sprite runCoinSprite;
    [SerializeField] private SoundDefinition gameMusic;
    [SerializeField] private SoundDefinition levelUpSound;
    [SerializeField] private float enemyLevelInterval = 30f;

    private const string RunTimerLabelName = "run-timer-label";
    private const string RunCoinRootName = "run-coin-root";
    private const string RunCoinIconName = "run-coin-icon";
    private const string RunCoinLabelName = "run-coin-label";
    private const string LevelProgressFillName = "level-progress-fill";
    private const string LevelUpRootName = "level-up-root";
    private const string StrengthButtonName = "strength-button";
    private const string DexterityButtonName = "dexterity-button";
    private const string IntelligenceButtonName = "intelligence-button";
    private const string PauseRootName = "pause-root";
    private const string ResumeButtonName = "resume-button";
    private const string PauseOptionsButtonName = "pause-options-button";
    private const string PauseQuitButtonName = "pause-quit-button";
    private const string GameOverRootName = "game-over-root";
    private const string RetryButtonName = "retry-button";
    private const string GameOverQuitButtonName = "quit-button";

    private Label _runTimerLabel;
    private Label _runCoinLabel;
    private Image _runCoinIcon;
    private VisualElement _levelProgressFill;
    private VisualElement _levelUpRoot;
    private PlayerLevelUp _playerLevelUp;
    private Stats _playerStats;
    private VisualElement _pauseRoot;
    private VisualElement _gameOverRoot;
    private OptionsMenuView _optionsMenuView;
    public event EventHandler OnEnemyLevelChanged;
    private float _currentRunTime;
    private int _currentRunCoins;
    private bool _isGameOver;
    private bool _isPaused;
    private int currentEnemyLevel = 1;
    private int previousEnemyLevel = 1;

    public static GameManager Instance { get; private set; }
    public static float CurrentRunTimeSeconds => Instance != null ? Instance._currentRunTime : 0f;
    public bool IsPaused => _isPaused;
    public bool IsGameOver => _isGameOver;
    public bool IsInputBlocked => _isPaused || _isGameOver;
    public static int CurrentEnemyLevel => Instance != null ? Instance.currentEnemyLevel : 1;

    private void Awake()
    {
        Instance = this;

        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;

        if (_playerLevelUp != null)
        {
            _playerLevelUp.OnXpChanged -= HandleXpChanged;
            _playerLevelUp.OnLevelUp -= HandleLevelUp;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        if (uiDocument == null) return;

        var root = uiDocument.rootVisualElement;
        var optionsRoot = root.Q<VisualElement>(OptionsMenuView.RootName);
        var playerInput = FindAnyObjectByType<PlayerInput>();
        _optionsMenuView = optionsRoot != null ? new OptionsMenuView(optionsRoot, playerInputActions, playerInput != null ? playerInput.actions : null) : null;
        SoundManager.Play(gameMusic);
        _runTimerLabel = root.Q<Label>(RunTimerLabelName);
        _runCoinLabel = root.Q<Label>(RunCoinLabelName);
        _runCoinIcon = root.Q<Image>(RunCoinIconName);
        _levelProgressFill = root.Q<VisualElement>(LevelProgressFillName);
        _levelUpRoot = root.Q<VisualElement>(LevelUpRootName);
        _pauseRoot = root.Q<VisualElement>(PauseRootName);
        _gameOverRoot = root.Q<VisualElement>(GameOverRootName);

        var strengthButton = root.Q<Button>(StrengthButtonName);
        var dexterityButton = root.Q<Button>(DexterityButtonName);
        var intelligenceButton = root.Q<Button>(IntelligenceButtonName);
        var resumeButton = root.Q<Button>(ResumeButtonName);
        var pauseOptionsButton = root.Q<Button>(PauseOptionsButtonName);
        var pauseQuitButton = root.Q<Button>(PauseQuitButtonName);
        var retryButton = root.Q<Button>(RetryButtonName);
        var gameOverQuitButton = root.Q<Button>(GameOverQuitButtonName);
        var optionsCloseButton = root.Q<Button>(OptionsMenuView.CloseButtonName);

        if (strengthButton != null) strengthButton.clicked += () => ResolveLevelUpChoice("strength");
        if (dexterityButton != null) dexterityButton.clicked += () => ResolveLevelUpChoice("dexterity");
        if (intelligenceButton != null) intelligenceButton.clicked += () => ResolveLevelUpChoice("intelligence");
        if (resumeButton != null) resumeButton.clicked += ResumeGame;
        if (pauseOptionsButton != null) pauseOptionsButton.clicked += OpenPauseOptions;
        if (pauseQuitButton != null) pauseQuitButton.clicked += QuitToMenu;
        if (retryButton != null) retryButton.clicked += RetryRun;
        if (gameOverQuitButton != null) gameOverQuitButton.clicked += QuitToMenu;
        if (optionsCloseButton != null) optionsCloseButton.clicked += ClosePauseOptions;

        _playerLevelUp = FindAnyObjectByType<PlayerLevelUp>();
        _playerStats = FindAnyObjectByType<Stats>();
        if (_playerLevelUp != null)
        {
            _playerLevelUp.OnXpChanged += HandleXpChanged;
            _playerLevelUp.OnLevelUp += HandleLevelUp;
        }

        if (_runTimerLabel != null)
        {
            _runTimerLabel.text = FormatRunTime(_currentRunTime);
        }

        RefreshRunCoinDisplay();
        if (_runCoinIcon != null)
        {
            _runCoinIcon.sprite = runCoinSprite;
        }

        if (_levelUpRoot != null)
        {
            _levelUpRoot.style.display = DisplayStyle.None;
        }

        if (_gameOverRoot != null)
        {
            _gameOverRoot.style.display = DisplayStyle.None;
        }

        if (_pauseRoot != null)
        {
            _pauseRoot.style.display = DisplayStyle.None;
        }

        RefreshLevelProgressBar();
    }

    private void Update()
    {
        if (_isGameOver)
        {
            return;
        }

        if (IsPauseOptionsOpen() || _isPaused)
        {
            return;
        }

        _currentRunTime += Time.deltaTime;
        currentEnemyLevel = Mathf.FloorToInt(_currentRunTime / enemyLevelInterval) + 1;

        if (currentEnemyLevel > previousEnemyLevel)
        {
            previousEnemyLevel = currentEnemyLevel;
            Debug.Log("Enemy level increased to: " + currentEnemyLevel);

            OnEnemyLevelChanged?.Invoke(this, EventArgs.Empty);
        }

        if (_runTimerLabel != null)
        {
            _runTimerLabel.text = FormatRunTime(_currentRunTime);
        }
    }

    public void HandlePauseInput()
    {
        if (_isGameOver)
        {
            return;
        }

        if (IsPauseOptionsOpen())
        {
            ClosePauseOptions();
            return;
        }

        TogglePause();
    }

    private void HandleXpChanged(object sender, EventArgs e)
    {
        RefreshLevelProgressBar();
    }

    private void HandleLevelUp(object sender, EventArgs e)
    {
        if (_isPaused)
        {
            ResumeGame();
        }

        Time.timeScale = 0f;
        SoundManager.Play(levelUpSound);
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
        if (_playerStats != null)
        {
            _playerStats.ApplyLevelUpChoice(choiceId);
        }
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

    public void ShowGameOver()
    {
        if (_isGameOver) return;

        _isGameOver = true;
        _isPaused = false;
        ClosePauseOptions();
        if (_pauseRoot != null)
        {
            _pauseRoot.style.display = DisplayStyle.None;
        }
        if (_gameOverRoot != null)
        {
            _gameOverRoot.style.display = DisplayStyle.Flex;
        }
    }

    private void TogglePause()
    {
        if (_levelUpRoot != null && _levelUpRoot.style.display == DisplayStyle.Flex)
        {
            return;
        }

        if (_isPaused)
        {
            ResumeGame();
            return;
        }

        PauseGame();
    }

    private void PauseGame()
    {
        if (_pauseRoot == null)
        {
            return;
        }

        _isPaused = true;
        Time.timeScale = 0f;
        _pauseRoot.style.display = DisplayStyle.Flex;
    }

    private void ResumeGame()
    {
        _isPaused = false;
        ClosePauseOptions();
        if (_pauseRoot != null)
        {
            _pauseRoot.style.display = DisplayStyle.None;
        }
        Time.timeScale = 1f;
    }

    private void OpenPauseOptions()
    {
        if (_optionsMenuView == null)
        {
            return;
        }

        if (_pauseRoot != null)
        {
            _pauseRoot.style.display = DisplayStyle.None;
        }

        _optionsMenuView.ShowSoundsTab();
        _optionsMenuView.Show();
    }

    private void ClosePauseOptions()
    {
        if (_optionsMenuView != null)
        {
            _optionsMenuView.Hide();
        }

        if (_isPaused && !_isGameOver && _pauseRoot != null)
        {
            _pauseRoot.style.display = DisplayStyle.Flex;
        }
    }

    private bool IsPauseOptionsOpen()
    {
        return _optionsMenuView != null && _optionsMenuView.Root.style.display == DisplayStyle.Flex;
    }

    public void AddRunCoins(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        _currentRunCoins += amount;
        RefreshRunCoinDisplay();
    }

    private static void RetryRun()
    {
        Time.timeScale = 1f;
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }

    private static void QuitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadSceneAsync("MainMenu");
    }

    private static string FormatRunTime(float runTimeSeconds)
    {
        var totalSeconds = Mathf.FloorToInt(runTimeSeconds);
        var minutes = totalSeconds / 60;
        var seconds = totalSeconds % 60;

        return $"{minutes:00}:{seconds:00}";
    }

    private void RefreshRunCoinDisplay()
    {
        if (_runCoinLabel != null)
        {
            _runCoinLabel.text = _currentRunCoins.ToString();
        }
    }
}
