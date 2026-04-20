using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour {
    [SerializeField] private UIDocument uiDocument;

    private const string RunTimerLabelName = "run-timer-label";
    private const string GameOverRootName = "game-over-root";
    private const string RetryButtonName = "retry-button";
    private const string QuitButtonName = "quit-button";

    private Label _runTimerLabel;
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
        if(Instance == this) {
            Instance = null;
        }
    }

    private void Start() {
        if(uiDocument == null) return;

        var root = uiDocument.rootVisualElement;
        _runTimerLabel = root.Q<Label>(RunTimerLabelName);
        _gameOverRoot = root.Q<VisualElement>(GameOverRootName);

        root.Q<Button>(RetryButtonName).clicked += RetryRun;
        root.Q<Button>(QuitButtonName).clicked += QuitToMenu;

        _runTimerLabel.text = FormatRunTime(_currentRunTime);
        _gameOverRoot.style.display = DisplayStyle.None;
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

    public void ShowGameOver() {
        if(_isGameOver) return;

        _isGameOver = true;
        _gameOverRoot.style.display = DisplayStyle.Flex;
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
