using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public sealed class OptionsMenuView {
    public const string RootName = "options-menu-root";
    public const string CloseButtonName = "options-close-button";

    private const string SoundsTabButtonName = "options-tab-sounds";
    private const string ControlsTabButtonName = "options-tab-controls";
    private const string SoundsContentName = "options-content-sounds";
    private const string ControlsContentName = "options-content-controls";
    private const string SfxSliderHostName = "options-sfx-slider-host";
    private const string MusicSliderHostName = "options-music-slider-host";
    private const string SliderControlClassName = "options-menu__slider";
    private const string WasdCompositeName = "WASD";

    public VisualElement Root { get; }
    public static bool IsCapturingBindingInput { get; private set; }

    private readonly Button _soundsTabButton;
    private readonly Button _controlsTabButton;
    private readonly VisualElement _soundsContent;
    private readonly VisualElement _controlsContent;
    private readonly Button _moveUpButton;
    private readonly Button _moveDownButton;
    private readonly Button _moveLeftButton;
    private readonly Button _moveRightButton;
    private readonly Button _jumpButton;
    private readonly Button _dashButton;
    private readonly Button _chargeButton;
    private readonly Button _slideButton;
    private readonly Button _interactButton;
    private readonly InputActionAsset _actions;

    private InputActionRebindingExtensions.RebindingOperation _activeRebind;
    private InputAction _activeAction;
    private Button _activeButton;
    private bool _wasActionEnabled;

    public OptionsMenuView(VisualElement root, InputActionAsset inputActions, InputActionAsset runtimeActions = null) {
        Root = root;
        _actions = runtimeActions != null ? runtimeActions : inputActions;

        _soundsTabButton = root.Q<Button>(SoundsTabButtonName);
        _controlsTabButton = root.Q<Button>(ControlsTabButtonName);
        _soundsContent = root.Q<VisualElement>(SoundsContentName);
        _controlsContent = root.Q<VisualElement>(ControlsContentName);
        _moveUpButton = root.Q<Button>("options-binding-move-up");
        _moveDownButton = root.Q<Button>("options-binding-move-down");
        _moveLeftButton = root.Q<Button>("options-binding-move-left");
        _moveRightButton = root.Q<Button>("options-binding-move-right");
        _jumpButton = root.Q<Button>("options-binding-jump");
        _dashButton = root.Q<Button>("options-binding-dash");
        _chargeButton = root.Q<Button>("options-binding-charge");
        _slideButton = root.Q<Button>("options-binding-slide");
        _interactButton = root.Q<Button>("options-binding-interact");

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

        if(_soundsTabButton != null) {
            _soundsTabButton.clicked += ShowSoundsTab;
        }

        if(_controlsTabButton != null) {
            _controlsTabButton.clicked += ShowControlsTab;
        }

        BindControlButtons();
        RefreshBindingLabels();
        ShowSoundsTab();
        Hide();
    }

    public void Show() {
        RefreshBindingLabels();
        Root.style.display = DisplayStyle.Flex;
    }

    public void Hide() {
        CancelActiveRebind();
        Root.style.display = DisplayStyle.None;
    }

    public void ShowSoundsTab() {
        SetActiveTab(showSounds: true);
    }

    private void ShowControlsTab() {
        RefreshBindingLabels();
        SetActiveTab(showSounds: false);
    }

    private void SetActiveTab(bool showSounds) {
        if(_soundsContent != null) {
            _soundsContent.style.display = showSounds ? DisplayStyle.Flex : DisplayStyle.None;
        }

        if(_controlsContent != null) {
            _controlsContent.style.display = showSounds ? DisplayStyle.None : DisplayStyle.Flex;
        }

        _soundsTabButton?.EnableInClassList("options-menu__tab--active", showSounds);
        _controlsTabButton?.EnableInClassList("options-menu__tab--active", !showSounds);
    }

    private static void BindSlider(VisualElement host, string sliderName, float initialValue, System.Action<float> onValueChanged) {
        if(host == null) {
            return;
        }

        host.Clear();

        var slider = new OptionsSliderControl(0f, 100f, initialValue) { name = sliderName };
        slider.AddToClassList(SliderControlClassName);
        slider.ValueChanged += value => onValueChanged?.Invoke(value);
        host.Add(slider);
    }

    private void BindControlButtons() {
        BindMoveButton(_moveUpButton, "up");
        BindMoveButton(_moveDownButton, "down");
        BindMoveButton(_moveLeftButton, "left");
        BindMoveButton(_moveRightButton, "right");
        BindActionButton(_jumpButton, "Jump");
        BindActionButton(_dashButton, "Dash");
        BindActionButton(_chargeButton, "Charge");
        BindActionButton(_slideButton, "Slide");
        BindActionButton(_interactButton, "Interact");
    }

    private void BindMoveButton(Button button, string movePartName) {
        if(button == null) {
            return;
        }

        button.clicked += () => StartInteractiveRebind(button, "Move", GetMoveBindingIndex(movePartName));
    }

    private void BindActionButton(Button button, string actionName) {
        if(button == null) {
            return;
        }

        button.clicked += () => StartInteractiveRebind(button, actionName, GetActionBindingIndex(actionName));
    }

    private void StartInteractiveRebind(Button button, string actionName, int bindingIndex) {
        if(button == null || bindingIndex < 0 || _actions == null) {
            return;
        }

        var action = _actions.FindAction(actionName, throwIfNotFound: false);
        if(action == null) {
            return;
        }

        CancelActiveRebind();

        _activeAction = action;
        _activeButton = button;
        _wasActionEnabled = action.enabled;
        IsCapturingBindingInput = true;
        button.text = "...";

        if(_wasActionEnabled) {
            action.Disable();
        }

        _activeRebind = action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsHavingToMatchPath("<Keyboard>/*")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnCancel(_ => FinishInteractiveRebind())
            .OnComplete(_ => FinishInteractiveRebind());
        _activeRebind.Start();
    }

    private void FinishInteractiveRebind() {
        _activeRebind?.Dispose();
        _activeRebind = null;

        if(_activeAction != null && _wasActionEnabled) {
            _activeAction.Enable();
        }

        _activeAction = null;
        _activeButton = null;
        _wasActionEnabled = false;
        IsCapturingBindingInput = false;
        RefreshBindingLabels();
    }

    private void CancelActiveRebind() {
        if(_activeRebind == null) {
            return;
        }

        FinishInteractiveRebind();
    }

    private void RefreshBindingLabels() {
        SetButtonLabel(_moveUpButton, "Move", GetMoveBindingIndex("up"), "W");
        SetButtonLabel(_moveDownButton, "Move", GetMoveBindingIndex("down"), "S");
        SetButtonLabel(_moveLeftButton, "Move", GetMoveBindingIndex("left"), "A");
        SetButtonLabel(_moveRightButton, "Move", GetMoveBindingIndex("right"), "D");
        SetButtonLabel(_jumpButton, "Jump", GetActionBindingIndex("Jump"), "Space");
        SetButtonLabel(_dashButton, "Dash", GetActionBindingIndex("Dash"), "Left Shift");
        SetButtonLabel(_chargeButton, "Charge", GetActionBindingIndex("Charge"), "Z");
        SetButtonLabel(_slideButton, "Slide", GetActionBindingIndex("Slide"), "C");
        SetButtonLabel(_interactButton, "Interact", GetActionBindingIndex("Interact"), "F");
    }

    private void SetButtonLabel(Button button, string actionName, int bindingIndex, string fallbackLabel) {
        if(button == null) {
            return;
        }

        if(button == _activeButton) {
            button.text = "...";
            return;
        }

        if(_actions == null || bindingIndex < 0) {
            button.text = fallbackLabel;
            return;
        }

        var action = _actions.FindAction(actionName, throwIfNotFound: false);
        button.text = action == null ? fallbackLabel : action.GetBindingDisplayString(bindingIndex);
    }

    private int GetMoveBindingIndex(string movePartName) {
        var moveAction = _actions != null ? _actions.FindAction("Move", throwIfNotFound: false) : null;

        if(moveAction == null) {
            return -1;
        }

        for(var i = 0; i < moveAction.bindings.Count; i++) {
            var binding = moveAction.bindings[i];
            if(!binding.isComposite || binding.name != WasdCompositeName) {
                continue;
            }

            for(var partIndex = i + 1; partIndex < moveAction.bindings.Count; partIndex++) {
                var partBinding = moveAction.bindings[partIndex];
                if(!partBinding.isPartOfComposite) {
                    break;
                }

                if(partBinding.name == movePartName) {
                    return partIndex;
                }
            }
        }

        return -1;
    }

    private int GetActionBindingIndex(string actionName) {
        var action = _actions != null ? _actions.FindAction(actionName, throwIfNotFound: false) : null;

        if(action == null) {
            return -1;
        }

        for(var i = 0; i < action.bindings.Count; i++) {
            var binding = action.bindings[i];
            if(binding.isComposite || binding.isPartOfComposite) {
                continue;
            }

            if(binding.path.StartsWith("<Keyboard>/")) {
                return i;
            }
        }

        return -1;
    }
}
