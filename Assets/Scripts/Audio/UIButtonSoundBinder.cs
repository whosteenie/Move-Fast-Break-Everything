using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class UIButtonSoundBinder : MonoBehaviour
{
    [SerializeField] private SoundDefinition hoverSound;
    [SerializeField] private SoundDefinition clickSound;

    private readonly HashSet<Button> _boundButtons = new();
    private readonly HashSet<Button> _pressedButtons = new();

    private UIDocument _uiDocument;

    private void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
    }

    private void Start()
    {
        BindButtons();
    }

    private void BindButtons()
    {
        if (_uiDocument == null)
        {
            return;
        }

        var root = _uiDocument.rootVisualElement;
        if (root == null)
        {
            return;
        }

        var buttons = root.Query<Button>().ToList();
        foreach (var button in buttons)
        {
            if (button == null || !_boundButtons.Add(button))
            {
                continue;
            }

            button.RegisterCallback<MouseEnterEvent>(_ => PlayHover(button));
            button.RegisterCallback<MouseDownEvent>(_ => PlayClick(button), TrickleDown.TrickleDown);
            button.RegisterCallback<PointerDownEvent>(_ => PlayClick(button), TrickleDown.TrickleDown);
            button.RegisterCallback<ClickEvent>(_ => ReleaseClick(button));
            button.RegisterCallback<PointerUpEvent>(_ => ReleaseClick(button));
            button.RegisterCallback<PointerCancelEvent>(_ => ReleaseClick(button));
        }
    }

    private void PlayHover(Button button)
    {
        if (button == null || !button.enabledInHierarchy)
        {
            return;
        }

        SoundManager.Play(hoverSound);
    }

    private void PlayClick(Button button)
    {
        if (button == null || !_pressedButtons.Add(button))
        {
            return;
        }

        SoundManager.Play(clickSound);
    }

    private void ReleaseClick(Button button)
    {
        if (button == null)
        {
            return;
        }

        _pressedButtons.Remove(button);
    }
}
