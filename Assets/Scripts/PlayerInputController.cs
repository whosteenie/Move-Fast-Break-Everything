using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerInputController : MonoBehaviour
{
    private TestMovement _testMovement;
    private Jump _jump;
    private Player _player;

    private void Awake()
    {
        _testMovement = GetComponent<TestMovement>();
        _jump = GetComponent<Jump>();
        _player = GetComponent<Player>();
    }

    private void OnDisable() {
        if(_testMovement != null) _testMovement.SetMoveInput(Vector2.zero);
    }

    private void OnMove(InputValue value)
    {
        if (IsInputBlocked())
        {
            if(_testMovement != null) _testMovement.SetMoveInput(Vector2.zero);

            return;
        }

        if(_testMovement != null) _testMovement.SetMoveInput(value.Get<Vector2>());
    }

    private void OnDash(InputValue value)
    {
        if (!value.isPressed || IsInputBlocked())
        {
            return;
        }

        if(_testMovement != null) _testMovement.TryDash();
    }

    private void OnSlide(InputValue value)
    {
        if (!value.isPressed || IsInputBlocked())
        {
            return;
        }

        if(_testMovement != null) _testMovement.TrySlide();
    }

    private void OnCharge(InputValue value)
    {
        if (!value.isPressed || IsInputBlocked())
        {
            return;
        }

        if(_testMovement != null) _testMovement.TryCharge();
    }

    private void OnJump(InputValue value)
    {
        if (!value.isPressed || IsInputBlocked())
        {
            return;
        }

        if(_jump != null) _jump.TryJump();
    }

    private void OnPause(InputValue value)
    {
        if (!value.isPressed || OptionsMenuView.IsCapturingBindingInput)
        {
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.HandlePauseInput();
        }
    }

    private void OnInteract(InputValue value)
    {
        if (!value.isPressed || IsInputBlocked())
        {
            return;
        }

        Tower_Base.TryInteractCurrent();
    }

    private bool IsInputBlocked()
    {
        if (_player != null && _player.IsDead)
        {
            return true;
        }

        return GameManager.Instance != null && GameManager.Instance.IsInputBlocked;
    }
}
