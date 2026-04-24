using UnityEngine.UIElements;

public sealed class OptionsMenuView
{
    private const string RootName = "options-menu-root";
    private const string SoundsTabButtonName = "options-tab-sounds";
    private const string ControlsTabButtonName = "options-tab-controls";
    public const string CloseButtonName = "options-close-button";
    private const string SoundsContentName = "options-content-sounds";
    private const string ControlsContentName = "options-content-controls";
    private const string SfxSliderName = "options-sfx-slider";
    private const string MusicSliderName = "options-music-slider";
    private const string ContentScrollViewName = "options-content-scrollview";

    public VisualElement Root { get; }

    private readonly Button _soundsTabButton;
    private readonly Button _controlsTabButton;
    private readonly VisualElement _soundsContent;
    private readonly VisualElement _controlsContent;

    public OptionsMenuView()
    {
        Root = new VisualElement { name = RootName };
        Root.AddToClassList("options-menu");

        var panel = new VisualElement();
        panel.AddToClassList("options-menu__panel");
        Root.Add(panel);

        var header = new VisualElement();
        header.AddToClassList("options-menu__header");
        panel.Add(header);

        var titleBlock = new VisualElement();
        titleBlock.AddToClassList("options-menu__title-block");
        header.Add(titleBlock);

        var title = new Label("Options");
        title.AddToClassList("options-menu__title");
        titleBlock.Add(title);

        var closeButton = new Button { name = CloseButtonName, text = "Back" };
        closeButton.AddToClassList("options-menu__close-button");
        header.Add(closeButton);

        var tabs = new VisualElement();
        tabs.AddToClassList("options-menu__tabs");
        panel.Add(tabs);

        _soundsTabButton = new Button { name = SoundsTabButtonName, text = "Sounds" };
        _soundsTabButton.AddToClassList("options-menu__tab");
        tabs.Add(_soundsTabButton);

        _controlsTabButton = new Button { name = ControlsTabButtonName, text = "Controls" };
        _controlsTabButton.AddToClassList("options-menu__tab");
        tabs.Add(_controlsTabButton);

        var contentScrollView = new ScrollView {
            name = ContentScrollViewName,
            mode = ScrollViewMode.Vertical
        };
        contentScrollView.AddToClassList("options-menu__scrollview");
        panel.Add(contentScrollView);

        _soundsContent = BuildSoundsContent();
        _soundsContent.name = SoundsContentName;
        contentScrollView.Add(_soundsContent);

        _controlsContent = BuildControlsContent();
        _controlsContent.name = ControlsContentName;
        contentScrollView.Add(_controlsContent);

        _soundsTabButton.clicked += ShowSoundsTab;
        _controlsTabButton.clicked += ShowControlsTab;

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
        SetActiveTab(true);
    }

    private void ShowControlsTab()
    {
        SetActiveTab(false);
    }

    private void SetActiveTab(bool showSounds)
    {
        _soundsContent.style.display = showSounds ? DisplayStyle.Flex : DisplayStyle.None;
        _controlsContent.style.display = showSounds ? DisplayStyle.None : DisplayStyle.Flex;
        _soundsTabButton.EnableInClassList("options-menu__tab--active", showSounds);
        _controlsTabButton.EnableInClassList("options-menu__tab--active", !showSounds);
    }

    private static VisualElement BuildSoundsContent()
    {
        var content = new VisualElement();
        content.AddToClassList("options-menu__content");

        content.Add(CreateSliderRow("SFX", SfxSliderName, 85f, true));
        content.Add(CreateSliderRow("Music", MusicSliderName, 70f));

        return content;
    }

    private static VisualElement BuildControlsContent()
    {
        var content = new VisualElement();
        content.AddToClassList("options-menu__content");

        content.Add(CreateBindingRow("Up", "W", true));
        content.Add(CreateBindingRow("Down", "S"));
        content.Add(CreateBindingRow("Left", "A"));
        content.Add(CreateBindingRow("Right", "D"));
        content.Add(CreateBindingRow("Jump", "Space"));
        content.Add(CreateBindingRow("Dash", "Left Shift"));
        content.Add(CreateBindingRow("Charge", "Mouse 1"));
        content.Add(CreateBindingRow("Slide", "Left Ctrl"));

        return content;
    }

    private static VisualElement CreateSliderRow(string labelText, string sliderName, float initialValue, bool isFirstRow = false)
    {
        var row = new VisualElement();
        row.AddToClassList("options-menu__row");
        if (isFirstRow)
        {
            row.AddToClassList("options-menu__row--first");
        }

        var label = new Label(labelText);
        label.AddToClassList("options-menu__row-label");
        row.Add(label);

        var sliderWrap = new VisualElement();
        sliderWrap.AddToClassList("options-menu__slider-wrap");
        row.Add(sliderWrap);

        var slider = new Slider(0f, 100f) { name = sliderName, value = initialValue };
        slider.AddToClassList("options-menu__slider");
        sliderWrap.Add(slider);

        return row;
    }

    private static VisualElement CreateBindingRow(string actionText, string bindingText, bool isFirstRow = false)
    {
        var row = new VisualElement();
        row.AddToClassList("options-menu__row");
        if (isFirstRow)
        {
            row.AddToClassList("options-menu__row--first");
        }

        var action = new Label(actionText);
        action.AddToClassList("options-menu__row-label");
        row.Add(action);

        var bindingButton = new Button { text = bindingText };
        bindingButton.AddToClassList("options-menu__binding-button");
        row.Add(bindingButton);

        return row;
    }
}
