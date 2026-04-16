using Unity.VisualScripting;
using UnityEngine;

public class TestMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Rigidbody2D rb;
    Vector2 movement;

    Vector2 facing = Vector2.right;

    public float dashSpeed = 25f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    float dashDurationTimer;
    float dashCooldownTimer;
    bool isDashing;

    // Get rid of this once we implement GameInput or whatever
    public KeyCode dashKey = KeyCode.Space;
    public KeyCode slideKey = KeyCode.LeftShift;

    // [SerializeField] private MovementSO[] movementArray;
    public MovementSO slideMovementSO;

    public MovementStateMachine movementStateMachine;

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (movement != Vector2.zero) {
            facing = movement.normalized;
        }

        if (Input.GetKeyDown(dashKey) && !isDashing && dashCooldownTimer <= 0f)
        {
            isDashing = true;
            dashDurationTimer = dashDuration;
        }
        if(Input.GetKeyDown(slideKey) && !(movementStateMachine.HasState(MovementStateMachine.State.slide) || movementStateMachine.HasState(MovementStateMachine.State.slideDecay)))
        {
            transform.localScale = new Vector3(.25f,.25f,.25f);
            movementStateMachine.AddState(slideMovementSO);
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

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + (movement * moveSpeed) * Time.fixedDeltaTime);
        if (isDashing)
            rb.MovePosition(rb.position + facing * dashSpeed * Time.fixedDeltaTime);
        if (movementStateMachine.HasState(MovementStateMachine.State.slide))
        {
            transform.localScale = new Vector3(.75f,.75f,.75f);
            Slide();
        }
        if (movementStateMachine.HasState(MovementStateMachine.State.slideDecay))
        {
            SlideDecay();
        }
        if (!movementStateMachine.HasState(MovementStateMachine.State.slideDecay))
        {
            transform.localScale = new Vector3(.5f,.5f,.5f);
        }
    }

    private void Slide()
    {
        //Shrink the Player
        Debug.Log("In Slide");
        rb.MovePosition(rb.position + facing*slideMovementSO.movePower/2*Time.fixedDeltaTime);
    }

    private void SlideDecay()
    {
        //Unshrink the player
        transform.localScale = new Vector3(.5f,.5f,.5f);
        Debug.Log("In Slide Decay");
        rb.MovePosition(rb.position + facing*(slideMovementSO.movePower)*Time.fixedDeltaTime);
    }

    private void Charge()
    {
        //Bulk the Player
        transform.localScale = new Vector3(.75f,.75f,.75f);
        rb.MovePosition(rb.position + facing*slideMovementSO.movePower/2*Time.fixedDeltaTime);
    }

    private void ChargeDecay()
    {
        //Shrink the player
        transform.localScale = new Vector3(.25f,.25f,.25f);
        Debug.Log("In Slide Decay");
        rb.MovePosition(rb.position + facing*(slideMovementSO.movePower)*Time.fixedDeltaTime);
    }
}
