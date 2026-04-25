using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private string playSceneName = "SampleScene";
    [SerializeField] private SoundDefinition menuMusic;

    public event Action<string> ButtonPressed;

    private void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        SoundManager.Play(menuMusic);

        root.Q<Button>("play-button").clicked += () => HandleButtonPressed("play", PlayGame);
        root.Q<Button>("options-button").clicked += () => HandleButtonPressed("options", OpenOptions);
        root.Q<Button>("quit-button").clicked += () => HandleButtonPressed("quit", QuitGame);
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
