using UnityEngine;
using UnityEngine.UIElements;

public sealed class OptionsMenuView
{
    public const string RootName = "options-menu-root";
    public const string CloseButtonName = "options-close-button";

    private const string SoundsTabButtonName = "options-tab-sounds";
    private const string ControlsTabButtonName = "options-tab-controls";
    private const string SoundsContentName = "options-content-sounds";
    private const string ControlsContentName = "options-content-controls";
    private const string SfxSliderHostName = "options-sfx-slider-host";
    private const string MusicSliderHostName = "options-music-slider-host";
    private const string SliderControlClassName = "options-menu__slider";

    public VisualElement Root { get; }

    private readonly Button _soundsTabButton;
    private readonly Button _controlsTabButton;
    private readonly VisualElement _soundsContent;
    private readonly VisualElement _controlsContent;

    public OptionsMenuView(VisualElement root)
    {
        Root = root;

        _soundsTabButton = root.Q<Button>(SoundsTabButtonName);
        _controlsTabButton = root.Q<Button>(ControlsTabButtonName);
        _soundsContent = root.Q<VisualElement>(SoundsContentName);
        _controlsContent = root.Q<VisualElement>(ControlsContentName);

        BindSlider(
            root.Q<VisualElement>(SfxSliderHostName),
            "options-sfx-slider",
            SoundManager.GetSfxVolume() * 100f,
            value => SoundManager.SetSfxVolume(value / 100f));
        BindSlider(
            root.Q<VisualElement>(MusicSliderHostName),
            "options-music-slider",
            SoundManager.GetMusicVolume() * 100f,
            value => SoundManager.SetMusicVolume(value / 100f));

        if (_soundsTabButton != null)
        {
            _soundsTabButton.clicked += ShowSoundsTab;
        }

        if (_controlsTabButton != null)
        {
            _controlsTabButton.clicked += ShowControlsTab;
        }

        ShowSoundsTab();
        Hide();
    }

    public void Show()
    {
        Root.style.display = DisplayStyle.Flex;
    }

    public void Hide()
    {
        Root.style.display = DisplayStyle.None;
    }

    public void ShowSoundsTab()
    {
        SetActiveTab(showSounds: true);
    }

    private void ShowControlsTab()
    {
        SetActiveTab(showSounds: false);
    }

    private void SetActiveTab(bool showSounds)
    {
        if (_soundsContent != null)
        {
            _soundsContent.style.display = showSounds ? DisplayStyle.Flex : DisplayStyle.None;
        }

        if (_controlsContent != null)
        {
            _controlsContent.style.display = showSounds ? DisplayStyle.None : DisplayStyle.Flex;
        }

        _soundsTabButton?.EnableInClassList("options-menu__tab--active", showSounds);
        _controlsTabButton?.EnableInClassList("options-menu__tab--active", !showSounds);
    }

    private static void BindSlider(VisualElement host, string sliderName, float initialValue, System.Action<float> onValueChanged)
    {
        if (host == null)
        {
            return;
        }

        host.Clear();

        var slider = new OptionsSliderControl(0f, 100f, initialValue) { name = sliderName };
        slider.AddToClassList(SliderControlClassName);
        slider.ValueChanged += value => onValueChanged?.Invoke(value);
        host.Add(slider);
    }
}
