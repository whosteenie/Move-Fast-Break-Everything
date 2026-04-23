using System;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerInputController : MonoBehaviour
{
    private const string PlayerActionMapName = "Player";
    private const string MoveActionName = "Move";
    private const string JumpActionName = "Jump";
    private const string DashActionName = "Dash";
    private const string SlideActionName = "Slide";
    private const string ChargeActionName = "Charge";
    private const string PauseActionName = "Pause";

    [Header("Input Actions")]
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string actionMapName = PlayerActionMapName;

    private InputActionMap _playerActionMap;
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _dashAction;
    private InputAction _slideAction;
    private InputAction _chargeAction;
    private InputAction _pauseAction;

    private bool _jumpQueued;
    private bool _dashQueued;
    private bool _slideQueued;
    private bool _chargeQueued;
    private bool _pauseQueued;

    public Vector2 MoveInput { get; private set; }

    public event Action JumpPerformed;
    public event Action DashPerformed;
    public event Action PausePerformed;

    private void Awake()
    {
        CacheActions();
    }

    private void OnEnable()
    {
        CacheActions();

        if (_playerActionMap == null)
        {
            return;
        }

        _playerActionMap.Enable();
        BindCallbacks();
        RefreshMoveInput();
    }

    private void OnDisable()
    {
        UnbindCallbacks();

        if (_playerActionMap != null)
        {
            _playerActionMap.Disable();
        }

        MoveInput = Vector2.zero;
        _jumpQueued = false;
        _dashQueued = false;
        _slideQueued = false;
        _chargeQueued = false;
        _pauseQueued = false;
    }

    public bool TryConsumeJump()
    {
        if (!_jumpQueued)
        {
            return false;
        }

        _jumpQueued = false;
        return true;
    }

    public bool TryConsumeDash()
    {
        if (!_dashQueued)
        {
            return false;
        }

        _dashQueued = false;
        return true;
    }

    public bool TryConsumePause()
    {
        if (!_pauseQueued)
        {
            return false;
        }

        _pauseQueued = false;
        return true;
    }

    public bool TryConsumeSlide()
    {
        if (!_slideQueued)
        {
            return false;
        }

        _slideQueued = false;
        return true;
    }

    public bool TryConsumeCharge()
    {
        if (!_chargeQueued)
        {
            return false;
        }

        _chargeQueued = false;
        return true;
    }

    private void CacheActions()
    {
        if (inputActions == null)
        {
            Debug.LogWarning($"{nameof(PlayerInputController)} on {name} is missing an InputActionAsset reference.", this);
            _playerActionMap = null;
            _moveAction = null;
            _jumpAction = null;
            _dashAction = null;
            _slideAction = null;
            _chargeAction = null;
            _pauseAction = null;
            return;
        }

        _playerActionMap = inputActions.FindActionMap(actionMapName);
        _moveAction = _playerActionMap?.FindAction(MoveActionName);
        _jumpAction = _playerActionMap?.FindAction(JumpActionName);
        _dashAction = _playerActionMap?.FindAction(DashActionName);
        _slideAction = _playerActionMap?.FindAction(SlideActionName);
        _chargeAction = _playerActionMap?.FindAction(ChargeActionName);
        _pauseAction = _playerActionMap?.FindAction(PauseActionName);

        if (_playerActionMap == null)
        {
            Debug.LogWarning(
                $"{nameof(PlayerInputController)} could not find action map '{actionMapName}' in '{inputActions.name}'.",
                this);
        }
    }

    private void BindCallbacks()
    {
        if (_moveAction != null)
        {
            _moveAction.performed += OnMovePerformed;
            _moveAction.canceled += OnMovePerformed;
        }

        if (_jumpAction != null)
        {
            _jumpAction.performed += OnJumpPerformed;
        }

        if (_dashAction != null)
        {
            _dashAction.performed += OnDashPerformed;
        }

        if (_slideAction != null)
        {
            _slideAction.performed += OnSlidePerformed;
        }

        if (_chargeAction != null)
        {
            _chargeAction.performed += OnChargePerformed;
        }

        if (_pauseAction != null)
        {
            _pauseAction.performed += OnPausePerformed;
        }
    }

    private void UnbindCallbacks()
    {
        if (_moveAction != null)
        {
            _moveAction.performed -= OnMovePerformed;
            _moveAction.canceled -= OnMovePerformed;
        }

        if (_jumpAction != null)
        {
            _jumpAction.performed -= OnJumpPerformed;
        }

        if (_dashAction != null)
        {
            _dashAction.performed -= OnDashPerformed;
        }

        if (_slideAction != null)
        {
            _slideAction.performed -= OnSlidePerformed;
        }

        if (_chargeAction != null)
        {
            _chargeAction.performed -= OnChargePerformed;
        }

        if (_pauseAction != null)
        {
            _pauseAction.performed -= OnPausePerformed;
        }
    }

    private void RefreshMoveInput()
    {
        MoveInput = _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero;
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
    }

    private void OnJumpPerformed(InputAction.CallbackContext _)
    {
        _jumpQueued = true;
        JumpPerformed?.Invoke();
    }

    private void OnDashPerformed(InputAction.CallbackContext _)
    {
        _dashQueued = true;
        DashPerformed?.Invoke();
    }

    private void OnSlidePerformed(InputAction.CallbackContext _)
    {
        _slideQueued = true;
    }

    private void OnChargePerformed(InputAction.CallbackContext _)
    {
        _chargeQueued = true;
    }

    private void OnPausePerformed(InputAction.CallbackContext _)
    {
        _pauseQueued = true;
        PausePerformed?.Invoke();
    }
}
