using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private string playSceneName = "SampleScene";

    public event Action<string> ButtonPressed;

    private OptionsMenuView _optionsMenuView;

    private void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        var optionsRoot = root.Q<VisualElement>(OptionsMenuView.RootName);
        _optionsMenuView = optionsRoot != null ? new OptionsMenuView(optionsRoot) : null;

        root.Q<Button>("play-button").clicked += () => HandleButtonPressed("play", PlayGame);
        root.Q<Button>("options-button").clicked += () => HandleButtonPressed("options", OpenOptions);
        root.Q<Button>("quit-button").clicked += () => HandleButtonPressed("quit", QuitGame);
        root.Q<Button>(OptionsMenuView.CloseButtonName).clicked += CloseOptions;
    }

    private void HandleButtonPressed(string buttonId, Action action)
    {
        ButtonPressed?.Invoke(buttonId);
        action();
    }

    private void PlayGame()
    {
        SceneManager.LoadScene(playSceneName);
    }

    private void OpenOptions()
    {
        _optionsMenuView?.ShowSoundsTab();
        _optionsMenuView?.Show();
    }

    private void CloseOptions()
    {
        _optionsMenuView?.Hide();
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
