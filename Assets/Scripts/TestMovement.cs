using UnityEngine;

public class TestMovement : MonoBehaviour
{
    private Stats stats;
    private Vector2 moveInput;

    public Rigidbody2D rb;
    UnityEngine.Vector2 facing = UnityEngine.Vector2.right;

    public float dashSpeed = 25f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    public float moveSpeed = 5f;

    float dashDurationTimer;
    float dashCooldownTimer;
    bool isDashing;
    public float baseMoveSpeed = 5f;

    private UnityEngine.Vector2 endPos;

    // [SerializeField] private MovementSO[] movementArray;
    public MovementSO slideMovementSO;
    public MovementSO chargeMovementSO;
    public MovementSO slideDashMovementSO;

    public MovementStateMachine movementStateMachine;

    [Header("Audio")]
    [SerializeField] private SoundDefinition slideSound;
    [SerializeField] private SoundDefinition dashSound;

    private void Update()
    {
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

    private void Awake()
    {
        stats = GetComponent<Stats>();
    }

    public void SetMoveInput(Vector2 input)
    {
        moveInput = Vector2.ClampMagnitude(input, 1f);

        if (moveInput != Vector2.zero)
        {
            facing = moveInput.normalized;
        }
    }

    public void TryDash()
    {
        if (movementStateMachine.HasState(MovementStateMachine.State.slide)
            && !movementStateMachine.HasState(MovementStateMachine.State.slideDash)
            && !movementStateMachine.HasState(MovementStateMachine.State.slideDashDecay))
        {
            Debug.Log("Slide Dash Initiated");
            movementStateMachine.AddComboState(slideDashMovementSO, MovementStateMachine.State.slide, MovementStateMachine.State.dash);
            return;
        }

        if (isDashing || dashCooldownTimer > 0f
            || movementStateMachine.HasState(MovementStateMachine.State.slideDash)
            || movementStateMachine.HasState(MovementStateMachine.State.slideDashDecay))
        {
            return;
        }

        isDashing = true;
        dashDurationTimer = dashDuration;
    }

    public void TrySlide()
    {
        if (movementStateMachine.HasState(MovementStateMachine.State.slide)
            || movementStateMachine.HasState(MovementStateMachine.State.slideDecay))
        {
            return;
        }

        movementStateMachine.AddState(slideMovementSO);
        SoundManager.Play(slideSound);
        SoundManager.Play(slideSound);
    }

    public void TryCharge()
    {
        if (movementStateMachine.HasState(MovementStateMachine.State.slide)
            || movementStateMachine.HasState(MovementStateMachine.State.slideDecay)
            || movementStateMachine.HasState(MovementStateMachine.State.charge)
            || movementStateMachine.HasState(MovementStateMachine.State.chargeDecay))
        {
            return;
        }

        movementStateMachine.AddState(chargeMovementSO);
    }
    //__________________________________________________________________________________________________
    void FixedUpdate()
    {
        UnityEngine.Vector2 endPos = new UnityEngine.Vector2(0,0);
        float currentMoveSpeed = (stats != null) ? stats.GetSpeed(baseMoveSpeed) : moveSpeed;
        endPos += rb.position;
        // rb.MovePosition(rb.position + (moveInput * moveSpeed) * Time.fixedDeltaTime);
        endPos += moveInput * (currentMoveSpeed * Time.fixedDeltaTime);
        if (isDashing)
        {
            // rb.MovePosition(rb.position + facing * dashSpeed * Time.fixedDeltaTime);
            endPos += Dash();
        }
        if (movementStateMachine.HasState(MovementStateMachine.State.slideDash))
        {
            endPos += SlideDash();
        }  

        if (movementStateMachine.HasState(MovementStateMachine.State.slide))
        {
            transform.localScale = new Vector3(.25f, .25f, .25f);
            endPos += Slide();
        }
        if (movementStateMachine.HasState(MovementStateMachine.State.slideDecay))
        {
            endPos += SlideDecay();
        }
        if (movementStateMachine.HasState(MovementStateMachine.State.charge) && !(movementStateMachine.HasState(MovementStateMachine.State.slide) || movementStateMachine.HasState(MovementStateMachine.State.slideDecay)))
        {
            endPos += Charge();
        }
        if (movementStateMachine.HasState(MovementStateMachine.State.chargeDecay) && !(movementStateMachine.HasState(MovementStateMachine.State.slide) || movementStateMachine.HasState(MovementStateMachine.State.slideDecay)))
        {
            endPos += ChargeDecay();
        }
        // print(endPos);
        rb.MovePosition(endPos);
    }

    private Vector2 Dash()
    {
        return facing * dashSpeed*(slideDashMovementSO.agilityScale*stats.speedMultiplier * Time.fixedDeltaTime);
    }

    private Vector2 Slide()
    {
        //Shrink the Player
        // Debug.Log("In Slide");
        transform.localScale = new Vector3(.25f, .25f, .25f);
        // rb.MovePosition(rb.position + facing*slideMovementSO.movePower*Time.fixedDeltaTime);
        return facing.normalized*slideMovementSO.movePower*(slideMovementSO.agilityScale*stats.speedMultiplier)*Time.fixedDeltaTime;
    }

    private Vector2 SlideDecay()
    {
        //Unshrink the player
        // Debug.Log("In Slide Decay");
        transform.localScale = new Vector3(.5f,.5f,.5f);
        rb.MovePosition(rb.position + facing * (slideMovementSO.movePower/2 * Time.fixedDeltaTime));
        return facing * (-slideMovementSO.movePower/4 * Time.fixedDeltaTime);
    }

    private Vector2 Charge()
    {
        //Bulk the Player
        transform.localScale = new UnityEngine.Vector3(.75f, .75f, .75f);
        rb.MovePosition(rb.position + facing * slideMovementSO.movePower / 2 * Time.fixedDeltaTime);
        //Moves you backwards a bit which can be used to do chargeswitch tech! EEEE!
        return facing * (-slideMovementSO.movePower/1.5f * Time.fixedDeltaTime);
    }

    private Vector2 ChargeDecay()
    {
        //Shrink the player
        transform.localScale = new UnityEngine.Vector3(.5f,.5f,.5f);
        // Debug.Log("In Slide Decay");
        // rb.MovePosition(rb.position + facing*(slideMovementSO.movePower)*Time.fixedDeltaTime);
        return facing.normalized*slideMovementSO.movePower*(chargeMovementSO.strengthScale*stats.damageMultiplier)*Time.fixedDeltaTime;
    }

    private Vector2 SlideDash()
    {
        Debug.Log("In Slide Dash");
        return facing.normalized*slideDashMovementSO.movePower*(slideDashMovementSO.agilityScale*stats.speedMultiplier)*Time.fixedDeltaTime;
    }
    //__________________________________________________________________________________________________
}
