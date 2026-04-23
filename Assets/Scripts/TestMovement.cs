using UnityEngine;

public class TestMovement : MonoBehaviour
{
    public Rigidbody2D rb;
    [SerializeField] private PlayerInputController inputController;

    private Vector2 movement;
    private Vector2 facing = Vector2.right;

    public float dashSpeed = 25f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    private float dashDurationTimer;
    private float dashCooldownTimer;
    private bool isDashing;
    public float moveSpeed = 5f;

    // [SerializeField] private MovementSO[] movementArray;
    public MovementSO slideMovementSO;
    public MovementSO chargeMovementSO;

    public MovementStateMachine movementStateMachine;

    private void Awake()
    {
        if (inputController == null)
        {
            inputController = GetComponent<PlayerInputController>();
        }
    }

    private void Update()
    {
        if (inputController != null && inputController.TryConsumePause())
        {
            GameManager.Instance.TogglePause();
        }

        if (GameManager.IsGameplayPaused)
        {
            movement = Vector2.zero;
            return;
        }

        movement = inputController != null ? inputController.MoveInput : Vector2.zero;

        if (movement != Vector2.zero)
        {
            facing = movement.normalized;
        }

        if (inputController != null && inputController.TryConsumeDash() && !isDashing && dashCooldownTimer <= 0f)
        {
            isDashing = true;
            dashDurationTimer = dashDuration;
        }

        if (inputController != null &&
            inputController.TryConsumeSlide() &&
            !HasMovementState(MovementStateMachine.State.slide) &&
            !HasMovementState(MovementStateMachine.State.slideDecay) &&
            slideMovementSO != null &&
            movementStateMachine != null)
        {
            // print("In Slide Key Press");
            movementStateMachine.AddState(slideMovementSO);
        }

        if (inputController != null &&
            inputController.TryConsumeCharge() &&
            !HasMovementState(MovementStateMachine.State.slide) &&
            !HasMovementState(MovementStateMachine.State.slideDecay) &&
            !HasMovementState(MovementStateMachine.State.charge) &&
            !HasMovementState(MovementStateMachine.State.chargeDecay) &&
            chargeMovementSO != null &&
            movementStateMachine != null)
        {
            // print("In Slide Key Press");
            movementStateMachine.AddState(chargeMovementSO);
        }

        if (inputController != null)
        {
            inputController.TryConsumeJump();
        }

        if (isDashing)
        {
            dashDurationTimer -= Time.deltaTime;
            if (dashDurationTimer <= 0f)
            {
                isDashing = false;
                dashCooldownTimer = dashCooldown;
            }
        }
        else if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        // rb.MovePosition(rb.position + (movement * moveSpeed) * Time.fixedDeltaTime);
        Vector2 endPos = rb.position;
        endPos += movement * (moveSpeed * Time.fixedDeltaTime);

        if (isDashing)
        {
            endPos += facing * (dashSpeed * Time.fixedDeltaTime);
        }

        if (HasMovementState(MovementStateMachine.State.slide))
        {
            transform.localScale = new Vector3(.25f, .25f, .25f);
            endPos += Slide();
        }

        if (HasMovementState(MovementStateMachine.State.slideDecay))
        {
            endPos += SlideDecay();
        }

        if (HasMovementState(MovementStateMachine.State.charge) &&
            !(HasMovementState(MovementStateMachine.State.slide) ||
              HasMovementState(MovementStateMachine.State.slideDecay)))
        {
            endPos += Charge();
        }

        if (HasMovementState(MovementStateMachine.State.chargeDecay) &&
            !(HasMovementState(MovementStateMachine.State.slide) ||
              HasMovementState(MovementStateMachine.State.slideDecay)))
        {
            endPos += ChargeDecay();
        }
        // print(endPos);

        rb.MovePosition(endPos);
    }

    private Vector2 Slide()
    {
        //Shrink the Player
        // Debug.Log("In Slide");
        transform.localScale = new Vector3(.25f, .25f, .25f);
        // rb.MovePosition(rb.position + facing*slideMovementSO.movePower*Time.fixedDeltaTime);
        return facing * (slideMovementSO.movePower * Time.fixedDeltaTime);
    }

    private Vector2 SlideDecay()
    {
        //Unshrink the player
        // Debug.Log("In Slide Decay");
        transform.localScale = new Vector3(.5f, .5f, .5f);
        rb.MovePosition(rb.position + facing * (slideMovementSO.movePower / 2 * Time.fixedDeltaTime));
        return facing * (-slideMovementSO.movePower / 4 * Time.fixedDeltaTime);
    }

    private Vector2 Charge()
    {
        //Bulk the Player
        transform.localScale = new Vector3(.75f, .75f, .75f);
        rb.MovePosition(rb.position + facing * slideMovementSO.movePower / 2 * Time.fixedDeltaTime);
        //Moves you backwards a bit which can be used to do chargeswitch tech! EEEE!
        return facing * (-slideMovementSO.movePower / 1.5f * Time.fixedDeltaTime);
    }

    private Vector2 ChargeDecay()
    {
        //Shrink the player
        transform.localScale = new Vector3(.5f, .5f, .5f);
        // Debug.Log("In Slide Decay");
        // rb.MovePosition(rb.position + facing*(slideMovementSO.movePower)*Time.fixedDeltaTime);
        return facing * (slideMovementSO.movePower * Time.fixedDeltaTime);
    }

    private bool HasMovementState(MovementStateMachine.State state)
    {
        return movementStateMachine != null && movementStateMachine.HasState(state);
    }
}
