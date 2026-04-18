using UnityEngine;

public class TestMovement : MonoBehaviour
{
    private Stats stats;


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

    void Update()
    {


        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (movement != Vector2.zero)
        {
            facing = movement.normalized;
        }

        if (Input.GetKeyDown(dashKey) && !isDashing && dashCooldownTimer <= 0f)
        {
            isDashing = true;
            dashDurationTimer = dashDuration;
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

    private void Awake()
    {
        stats = GetComponent<Stats>();
    }
    //__________________________________________________________________________________________________
    void FixedUpdate()
    {
        float speed = (stats != null) ? stats.speedMultiplier : 5f;
        if (isDashing)
            rb.MovePosition(rb.position + facing * dashSpeed * Time.fixedDeltaTime);
        else
            rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
    }
    //__________________________________________________________________________________________________
}
