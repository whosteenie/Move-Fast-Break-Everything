using System;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class OptionsSliderControl : VisualElement
{
    private readonly VisualElement _fill;
    private readonly VisualElement _knob;

    private float _lowValue;
    private float _highValue;
    private float _value;

    public float LowValue
    {
        get => _lowValue;
        set
        {
            _lowValue = value;
            _value = Mathf.Clamp(_value, _lowValue, _highValue);
            RefreshVisuals();
        }
    }

    public float HighValue
    {
        get => _highValue;
        set
        {
            _highValue = value;
            _value = Mathf.Clamp(_value, _lowValue, _highValue);
            RefreshVisuals();
        }
    }

    public float Value
    {
        get => _value;
        set
        {
            var clampedValue = Mathf.Clamp(value, _lowValue, _highValue);
            if (Mathf.Approximately(_value, clampedValue))
            {
                return;
            }

            _value = clampedValue;
            RefreshVisuals();
            ValueChanged?.Invoke(_value);
        }
    }

    public event Action<float> ValueChanged;

    public OptionsSliderControl(float lowValue, float highValue, float initialValue)
    {
        _lowValue = lowValue;
        _highValue = highValue;
        _value = Mathf.Clamp(initialValue, _lowValue, _highValue);

        AddToClassList("options-slider");

        var track = new VisualElement();
        track.AddToClassList("options-slider__track");
        Add(track);

        _fill = new VisualElement();
        _fill.AddToClassList("options-slider__fill");
        Add(_fill);

        _knob = new VisualElement();
        _knob.AddToClassList("options-slider__knob");
        Add(_knob);

        RegisterCallback<GeometryChangedEvent>(_ => RefreshVisuals());
        RegisterCallback<PointerDownEvent>(OnPointerDown);
        RegisterCallback<PointerMoveEvent>(OnPointerMove);
        RegisterCallback<PointerUpEvent>(OnPointerUp);
        RegisterCallback<PointerCaptureOutEvent>(_ => RefreshVisuals());

        RefreshVisuals();
    }

    private void OnPointerDown(PointerDownEvent evt)
    {
        this.CapturePointer(evt.pointerId);
        UpdateFromLocalPosition(evt.localPosition.x);
        evt.StopPropagation();
    }

    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (!this.HasPointerCapture(evt.pointerId))
        {
            return;
        }

        UpdateFromLocalPosition(evt.localPosition.x);
        evt.StopPropagation();
    }

    private void OnPointerUp(PointerUpEvent evt)
    {
        if (!this.HasPointerCapture(evt.pointerId))
        {
            return;
        }

        UpdateFromLocalPosition(evt.localPosition.x);
        this.ReleasePointer(evt.pointerId);
        evt.StopPropagation();
    }

    private void UpdateFromLocalPosition(float localX)
    {
        var width = contentRect.width;
        if (width <= 0f)
        {
            return;
        }

        var normalized = Mathf.Clamp01(localX / width);
        Value = Mathf.Lerp(_lowValue, _highValue, normalized);
    }

    private void RefreshVisuals()
    {
        var range = _highValue - _lowValue;
        var normalized = range <= 0f ? 0f : Mathf.InverseLerp(_lowValue, _highValue, _value);
        var percent = normalized * 100f;

        _fill.style.width = Length.Percent(percent);
        _knob.style.left = Length.Percent(percent);
    }
}
