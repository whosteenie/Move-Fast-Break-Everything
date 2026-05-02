using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private string playSceneName = "SampleScene";
    [SerializeField] private InputActionAsset playerInputActions;
    [SerializeField] private Sprite shopCoinSprite;
    [SerializeField] private ShopPowerUpDefinition[] shopPowerUps;
    [SerializeField] private SoundDefinition menuMusic;

    public event Action<string> ButtonPressed;

    private OptionsMenuView _optionsMenuView;
    private MainMenuShopView _shopView;

    private void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        SoundManager.Play(menuMusic);
        var optionsRoot = root.Q<VisualElement>(OptionsMenuView.RootName);
        _optionsMenuView = optionsRoot != null ? new OptionsMenuView(optionsRoot, playerInputActions) : null;
        var shopRoot = root.Q<VisualElement>(MainMenuShopView.RootName);
        _shopView = shopRoot != null ? new MainMenuShopView(shopRoot, shopPowerUps, shopCoinSprite) : null;

        BindButton(root, "play-button", () => HandleButtonPressed("play", PlayGame));
        BindButton(root, "shop-button", () => HandleButtonPressed("shop", OpenShop));
        BindButton(root, "options-button", () => HandleButtonPressed("options", OpenOptions));
        BindButton(root, "quit-button", () => HandleButtonPressed("quit", QuitGame));
        BindButton(root, OptionsMenuView.CloseButtonName, CloseOptions);
        BindButton(root, MainMenuShopView.BackButtonName, CloseShop);
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

    private void OpenShop()
    {
        _optionsMenuView?.Hide();
        _shopView?.Show();
    }

    private void CloseOptions()
    {
        _optionsMenuView?.Hide();
    }

    private void CloseShop()
    {
        _shopView?.Hide();
    }

    private static void BindButton(VisualElement root, string buttonName, Action action)
    {
        var button = root.Q<Button>(buttonName);
        if (button != null)
        {
            button.clicked += action;
        }
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
