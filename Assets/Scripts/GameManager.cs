using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private Sprite runCoinSprite;
    [SerializeField] private SoundDefinition gameMusic;
    [SerializeField] private SoundDefinition levelUpSound;
    [SerializeField] private ShopPowerUpDefinition maxHealthPowerUp;
    [SerializeField] private ShopPowerUpDefinition mightPowerUp;
    [SerializeField] private ShopPowerUpDefinition hastePowerUp;
    [SerializeField] private ShopPowerUpDefinition moveSpeedPowerUp;
    [SerializeField] private ShopPowerUpDefinition defensePowerUp;
    [SerializeField] private ShopPowerUpDefinition piercePowerUp;
    [SerializeField] private ShopPowerUpDefinition thornsPowerUp;
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
    private const string PauseStatHealthLabelName = "pause-stat-health-label";
    private const string PauseStatDamageLabelName = "pause-stat-damage-label";
    private const string PauseStatFireRateLabelName = "pause-stat-fire-rate-label";
    private const string PauseStatMoveSpeedLabelName = "pause-stat-move-speed-label";
    private const string PauseStatDefenseLabelName = "pause-stat-defense-label";
    private const string PauseStatPierceLabelName = "pause-stat-pierce-label";
    private const string PauseStatThornsLabelName = "pause-stat-thorns-label";
    private const string PauseStatHealthIconName = "pause-stat-health-icon";
    private const string PauseStatDamageIconName = "pause-stat-damage-icon";
    private const string PauseStatFireRateIconName = "pause-stat-fire-rate-icon";
    private const string PauseStatMoveSpeedIconName = "pause-stat-move-speed-icon";
    private const string PauseStatDefenseIconName = "pause-stat-defense-icon";
    private const string PauseStatPierceIconName = "pause-stat-pierce-icon";
    private const string PauseStatThornsIconName = "pause-stat-thorns-icon";
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
    private Label _pauseStatHealthLabel;
    private Label _pauseStatDamageLabel;
    private Label _pauseStatFireRateLabel;
    private Label _pauseStatMoveSpeedLabel;
    private Label _pauseStatDefenseLabel;
    private Label _pauseStatPierceLabel;
    private Label _pauseStatThornsLabel;
    private Image _pauseStatHealthIcon;
    private Image _pauseStatDamageIcon;
    private Image _pauseStatFireRateIcon;
    private Image _pauseStatMoveSpeedIcon;
    private Image _pauseStatDefenseIcon;
    private Image _pauseStatPierceIcon;
    private Image _pauseStatThornsIcon;
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
        _optionsMenuView = optionsRoot != null ? new OptionsMenuView(optionsRoot) : null;
        SoundManager.Play(gameMusic);
        _runTimerLabel = root.Q<Label>(RunTimerLabelName);
        _runCoinLabel = root.Q<Label>(RunCoinLabelName);
        _runCoinIcon = root.Q<Image>(RunCoinIconName);
        _levelProgressFill = root.Q<VisualElement>(LevelProgressFillName);
        _levelUpRoot = root.Q<VisualElement>(LevelUpRootName);
        _pauseRoot = root.Q<VisualElement>(PauseRootName);
        _pauseStatHealthLabel = root.Q<Label>(PauseStatHealthLabelName);
        _pauseStatDamageLabel = root.Q<Label>(PauseStatDamageLabelName);
        _pauseStatFireRateLabel = root.Q<Label>(PauseStatFireRateLabelName);
        _pauseStatMoveSpeedLabel = root.Q<Label>(PauseStatMoveSpeedLabelName);
        _pauseStatDefenseLabel = root.Q<Label>(PauseStatDefenseLabelName);
        _pauseStatPierceLabel = root.Q<Label>(PauseStatPierceLabelName);
        _pauseStatThornsLabel = root.Q<Label>(PauseStatThornsLabelName);
        _pauseStatHealthIcon = root.Q<Image>(PauseStatHealthIconName);
        _pauseStatDamageIcon = root.Q<Image>(PauseStatDamageIconName);
        _pauseStatFireRateIcon = root.Q<Image>(PauseStatFireRateIconName);
        _pauseStatMoveSpeedIcon = root.Q<Image>(PauseStatMoveSpeedIconName);
        _pauseStatDefenseIcon = root.Q<Image>(PauseStatDefenseIconName);
        _pauseStatPierceIcon = root.Q<Image>(PauseStatPierceIconName);
        _pauseStatThornsIcon = root.Q<Image>(PauseStatThornsIconName);
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
        RefreshPauseStatIcons();
        RefreshPauseStatsCard();
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

        RefreshPauseStatsCard();
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

    private void RefreshPauseStatsCard()
    {
        if (_playerStats == null)
        {
            _playerStats = FindAnyObjectByType<Stats>();
        }

        if (_playerStats == null)
        {
            SetPauseStatLabels("--");
            return;
        }

        SetLabelText(_pauseStatHealthLabel, _playerStats.GetMaxHealth().ToString());
        SetLabelText(_pauseStatDamageLabel, FormatMultiplier(_playerStats.rangedDamageMultiplier));
        SetLabelText(_pauseStatFireRateLabel, FormatMultiplier(_playerStats.dexterityMultiplier));
        SetLabelText(_pauseStatMoveSpeedLabel, FormatMultiplier(_playerStats.speedMultiplier));
        SetLabelText(_pauseStatDefenseLabel, FormatPercent(_playerStats.defense));
        SetLabelText(_pauseStatPierceLabel, FormatPercent(_playerStats.GetPierce()));
        SetLabelText(_pauseStatThornsLabel, FormatPercent(_playerStats.thorns));
    }

    private void RefreshPauseStatIcons()
    {
        SetStatIcon(_pauseStatHealthIcon, maxHealthPowerUp);
        SetStatIcon(_pauseStatDamageIcon, mightPowerUp);
        SetStatIcon(_pauseStatFireRateIcon, hastePowerUp);
        SetStatIcon(_pauseStatMoveSpeedIcon, moveSpeedPowerUp);
        SetStatIcon(_pauseStatDefenseIcon, defensePowerUp);
        SetStatIcon(_pauseStatPierceIcon, piercePowerUp);
        SetStatIcon(_pauseStatThornsIcon, thornsPowerUp);
    }

    private static void SetStatIcon(Image iconElement, ShopPowerUpDefinition powerUp)
    {
        if (iconElement != null)
        {
            iconElement.sprite = powerUp != null ? powerUp.Icon : null;
        }
    }

    private void SetPauseStatLabels(string value)
    {
        SetLabelText(_pauseStatHealthLabel, value);
        SetLabelText(_pauseStatDamageLabel, value);
        SetLabelText(_pauseStatFireRateLabel, value);
        SetLabelText(_pauseStatMoveSpeedLabel, value);
        SetLabelText(_pauseStatDefenseLabel, value);
        SetLabelText(_pauseStatPierceLabel, value);
        SetLabelText(_pauseStatThornsLabel, value);
    }

    private static void SetLabelText(Label label, string value)
    {
        if (label != null)
        {
            label.text = value;
        }
    }

    private static string FormatMultiplier(float value)
    {
        return $"x{value:0.00}";
    }

    private static string FormatPercent(float value)
    {
        return $"{value * 100f:0}%";
    }

    private void RefreshRunCoinDisplay()
    {
        if (_runCoinLabel != null)
        {
            _runCoinLabel.text = _currentRunCoins.ToString();
        }
    }
}
