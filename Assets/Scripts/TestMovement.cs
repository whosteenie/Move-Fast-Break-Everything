using Unity.VisualScripting;
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

    public float baseMoveSpeed = 5f;

    private UnityEngine.Vector2 endPos;

    // [SerializeField] private MovementSO[] movementArray;
    public MovementSO slideMovementSO;
    public MovementSO chargeMovementSO;
    public MovementSO slideDashMovementSO;

    public MovementStateMachine movementStateMachine;

    public ParticleSystem failureParticle;

    [Header("Audio")]
    [SerializeField] private SoundDefinition slideSound;
    [SerializeField] private SoundDefinition dashSound;

    private bool IsSliding => HasMovementState(MovementStateMachine.State.slide);
    private bool IsSlideDecaying => HasMovementState(MovementStateMachine.State.slideDecay);
    private bool IsSlidingOrDecaying => IsSliding || IsSlideDecaying;
    private bool IsSlideDashing => HasMovementState(MovementStateMachine.State.slideDash);
    private bool IsSlideDashDecaying => HasMovementState(MovementStateMachine.State.slideDashDecay);
    private bool IsInSlideDashState => IsSlideDashing || IsSlideDashDecaying;
    private bool IsCharging => HasMovementState(MovementStateMachine.State.charge);
    private bool IsChargeDecaying => HasMovementState(MovementStateMachine.State.chargeDecay);
    private bool IsInChargeState => IsCharging || IsChargeDecaying;

    private bool HasMovementState(MovementStateMachine.State state)
    {
        return movementStateMachine.HasState(state);
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
        if (IsSliding && !IsInSlideDashState)
        {
            Debug.Log("Slide Dash Initiated");
            movementStateMachine.AddComboState(slideDashMovementSO, MovementStateMachine.State.slide, MovementStateMachine.State.dash);
            return;
        }
        else 
        {
            MoveFail();
        }

        if (IsDashing || IsDashDecaying || IsInSlideDashState || grappleHook.IsDashBlocked)
        {
            MoveFail();
            return;
        }

        movementStateMachine.AddState(dashMovementSO);
    }

    public void TrySlide()
    {
        if (IsSlidingOrDecaying)
        {
            MoveFail();
            return;
        }

        movementStateMachine.AddState(slideMovementSO);
        SoundManager.Play(slideSound);
    }

    public void TryCharge()
    {
        if (IsSlidingOrDecaying || IsInChargeState)
        {
            MoveFail();
            return;
        }

        movementStateMachine.AddState(chargeMovementSO);
    }

    //__________________________________________________________________________________________________
    void FixedUpdate()
    {
        if (grappleHook.IsGrappleControlling)
        {
            rb.MovePosition(grappleHook.GetOrbitPosition());
            return;
        }

        UnityEngine.Vector2 endPos = new UnityEngine.Vector2(0,0);
        float currentMoveSpeed = (stats != null) ? stats.GetSpeed(baseMoveSpeed) : moveSpeed;
        endPos += rb.position;
        // rb.MovePosition(rb.position + (moveInput * moveSpeed) * Time.fixedDeltaTime);
        endPos += moveInput * (currentMoveSpeed * Time.fixedDeltaTime);
        if (IsDashing)
        {
            // rb.MovePosition(rb.position + facing * dashSpeed * Time.fixedDeltaTime);
            endPos += Dash();
        }
        if (IsSlideDashing)
        {
            endPos += SlideDash();
        }  

        if (IsSliding)
        {
            transform.localScale = new Vector3(.25f, .25f, .25f);
            endPos += Slide();
        }
        if (IsSlideDecaying)
        {
            endPos += SlideDecay();
        }
        if (IsCharging && !IsSlidingOrDecaying)
        {
            endPos += Charge();
        }
        if (IsChargeDecaying && !IsSlidingOrDecaying)
        {
            endPos += ChargeDecay();
        }

        // new stuff
        if (IsGrappleWhipping)
        {
            endPos += grappleHook.GetWhipDiff();
        }
        endPos = grappleHook.ConstrainPosition(rb.position, endPos);

        rb.MovePosition(endPos);
    }

    private Vector2 Dash()
    {
        failureParticle.startColor = Color.blue;
        failureParticle.Play();
        return facing * (dashMovementSO.movePower * (slideDashMovementSO.agilityScale*stats.speedMultiplier * Time.fixedDeltaTime));
    }

    private Vector2 Slide()
    {
        //Shrink the Player
        // Debug.Log("In Slide");
        failureParticle.startColor = Color.red;
        failureParticle.Play();
        transform.localScale = new Vector3(.25f, .25f, .25f);
        // rb.MovePosition(rb.position + facing*slideMovementSO.movePower*Time.fixedDeltaTime);
        return facing.normalized * (slideMovementSO.movePower * (slideMovementSO.agilityScale*stats.speedMultiplier) * Time.fixedDeltaTime);
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

        //Just for testing play the failure particle
        failureParticle.startColor = Color.green;
        failureParticle.Play();
        
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
        return facing.normalized * (slideMovementSO.movePower * (chargeMovementSO.strengthScale*stats.damageMultiplier) * Time.fixedDeltaTime);
    }

    private Vector2 SlideDash()
    {
        failureParticle.startColor = Color.purple;
        failureParticle.Play();
        Debug.Log("In Slide Dash");
        return facing.normalized * (slideDashMovementSO.movePower * (slideDashMovementSO.agilityScale*stats.speedMultiplier) * Time.fixedDeltaTime);
    }

    public void MoveFail()
    {
        failureParticle.startColor = Color.black;
        failureParticle.Play();
    }
    //__________________________________________________________________________________________________

    // new stuff
    public MovementSO dashMovementSO;
    public GrapplingHook grappleHook;

    private bool IsDashing => HasMovementState(MovementStateMachine.State.dash);
    private bool IsDashDecaying => HasMovementState(MovementStateMachine.State.dashDecay);
    private bool IsGrappleWhipping => HasMovementState(MovementStateMachine.State.grappleWhipping);

    public Vector2 GetFacing() => facing;
    public Vector2 GetMoveInput() => moveInput;
}