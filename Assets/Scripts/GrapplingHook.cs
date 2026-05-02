using System.Collections;
using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
    public float projectileSpeed = 8f;
    public float maxDistance = 10f;
    public KeyCode grappleKey = KeyCode.G;

    public LineRenderer lineRenderer;
    public Color cableNormalColor = Color.white;
    public Color cableStretchColor = Color.red;
    public float cableNormalWidth = 0.05f;
    public float cableStretchWidth = 0.12f;

    public float maxLength = 8f;
    public float stretchExponent = 2f;
    public float tangentialThreshold = 2f;

    public float orbitReleaseBoost = 3f;

    public float whipPower = 30f;
    public float whipTowardRadius = 0.5f;
    public float whipSteerInfluence = 0.4f;
    public float whipDuration = 0.3f;

    public bool hasAimLock = false;
    public float aimLockRange = 10f;
    public float aimLockWidth = 3f;

    public float grappleCooldown = 1f;

    public LayerMask excludedLayers;

    public Rigidbody2D rb;
    public MovementStateMachine movementStateMachine;
    public TestMovement testMovement;

    public bool IsOrbiting => HasState(MovementStateMachine.State.grappleOrbiting);
    public bool IsWhipping => HasState(MovementStateMachine.State.grappleWhipping);
    public bool IsGrappleControlling => IsOrbiting;
    public bool IsDashBlocked => IsOrbiting;

    private GameObject projectile;
    private bool firing;
    private Vector2 firingDirection;

    private float cooldownTimer;

    private Transform grappleTarget;
    private Vector3 grappleLocalOffset;
    private Vector2 tempPoint;
    private Vector2 GrapplePoint => grappleTarget != null ? (Vector2)grappleTarget.TransformPoint(grappleLocalOffset) : tempPoint;

    private float whipTimer;
    private bool whipping;
    private Vector2 whipDirection;

    private float orbitRadius;
    private float orbitAngle;
    private float orbitSpeed;

    private void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (Input.GetKeyDown(grappleKey))
            HandleGrappleInput();

        if (firing)
            UpdateProjectile();

        UpdateCableVisual();
    }

    private void HandleGrappleInput()
    {
        if (cooldownTimer > 0f) {
            return;
        } 

        if (firing) { 
            RecallProjectile(); return; 
        }
        if (IsWhipping) {
            return;
        }
        if (IsOrbiting) { 
            ReleaseOrbit(); return;
        }
        if (HasState(MovementStateMachine.State.grappled)) { 
            StartWhip(); return; 
        }

        if (hasAimLock)
            FireAimLock();
        else
            FireProjectile();
    }

    // Stop when we hit something
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!whipping && !IsOrbiting) return;

        if ((~excludedLayers & (1 << collision.gameObject.layer)) != 0)
        {
            if (IsOrbiting)
                ReleaseOrbit();
            else
            {
                whipping = false;
                movementStateMachine.RemoveState(MovementStateMachine.State.grappleWhipping);
            }
        }
    }

    private void FireProjectile()
    {
        firingDirection = testMovement.GetFacing();
        projectile = new GameObject("GrappleHook");
        projectile.transform.position = rb.position;
        firing = true;
        movementStateMachine.AddStateNoTimer(MovementStateMachine.State.grappleFiring);
    }

    // Lock onto nearest object in the general direction we're looking
    private void FireAimLock()
    {
        Vector2 facing = testMovement.GetFacing();
        Vector2 boxCenter = rb.position + facing * (aimLockRange / 2f);
        float angle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;

        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, new Vector2(aimLockRange, aimLockWidth), angle, ~excludedLayers);

        Collider2D nearest = null;
        float nearestDist = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            if (hit.attachedRigidbody == rb) continue; // skip self
            float dist = Vector2.Distance(rb.position, hit.transform.position);
            if (dist < nearestDist) { nearestDist = dist; nearest = hit; }
        }

        if (nearest == null) { testMovement.MoveFail(); return; }

        AttachHook(nearest.ClosestPoint(rb.position), nearest.transform);
    }

    private void UpdateProjectile()
    {
        projectile.transform.position += (Vector3)(firingDirection * projectileSpeed * Time.deltaTime);

        if (Vector2.Distance(rb.position, projectile.transform.position) >= maxDistance)
        {
            RecallProjectile();
            return;
        }

        Collider2D hit = Physics2D.OverlapCircle(projectile.transform.position, 0.1f, ~excludedLayers);
        if (hit != null)
            AttachHook(projectile.transform.position, hit.transform);
    }

    private void AttachHook(Vector2 point, Transform target)
    {
        grappleTarget = target;
        grappleLocalOffset = target.InverseTransformPoint(point);
        tempPoint = point;
        firing = false;
        Destroy(projectile);
        movementStateMachine.RemoveState(MovementStateMachine.State.grappleFiring);
        movementStateMachine.AddStateNoTimer(MovementStateMachine.State.grappled);
    }

    private void RecallProjectile()
    {
        firing = false;
        Destroy(projectile);
        movementStateMachine.RemoveState(MovementStateMachine.State.grappleFiring);
        StartCooldown();
    }

    // We stay within range of grapple
    public Vector2 ConstrainPosition(Vector2 currentPos, Vector2 proposedPos)
    {
        if (!HasState(MovementStateMachine.State.grappled)) return proposedPos;

        Vector2 toGrapple = GrapplePoint - proposedPos;
        float dist = toGrapple.magnitude;
        float overflow = dist - maxLength;

        if (overflow <= 0f) return proposedPos;

        Vector2 radialDir = toGrapple.normalized;
        Vector2 moveDiff = proposedPos - currentPos;
        float outwardComponent = Vector2.Dot(moveDiff, -radialDir);

        Vector2 constrained;
        if (outwardComponent > 0f)
        {
            // Slow down when moving away
            float resistFactor = 1f / (1f + Mathf.Pow(overflow * stretchExponent, 2f));
            constrained = proposedPos + radialDir * outwardComponent * (1f - resistFactor);
        }
        else
        {
            // Speed up a lil when moving toward
            float boostFactor = Mathf.Pow(overflow, stretchExponent) * Time.fixedDeltaTime;
            constrained = proposedPos + radialDir * boostFactor;
        }

        // Start orbitting with fast tangential movement
        Vector2 tangentialDiff = moveDiff - Vector2.Dot(moveDiff, -radialDir) * (-radialDir);
        float tangentialSpeed = tangentialDiff.magnitude / Time.fixedDeltaTime;
        if (overflow > 0f && tangentialSpeed >= tangentialThreshold)
            EnterOrbit(currentPos, tangentialDiff);

        return constrained;
    }

    // Orbit around grapple point
    private void EnterOrbit(Vector2 currentPos, Vector2 tangentialDiff) {
        orbitRadius = Vector2.Distance(currentPos, GrapplePoint);
        Vector2 toPlayer = currentPos - GrapplePoint;
        orbitAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;

        float tangentialSpeed = tangentialDiff.magnitude / Time.fixedDeltaTime;
        orbitSpeed = (tangentialSpeed / (2f * Mathf.PI * orbitRadius)) * 360f;

        Vector2 radialDir = toPlayer.normalized;
        Vector2 tangentDirection = new Vector2(-radialDir.y, radialDir.x);
        if (Vector2.Dot(tangentDirection, tangentialDiff) < 0f)
            orbitSpeed = -orbitSpeed;

        movementStateMachine.RemoveState(MovementStateMachine.State.grappled);
        movementStateMachine.AddStateNoTimer(MovementStateMachine.State.grappleOrbiting);
    }

    public Vector2 GetOrbitPosition() {
        orbitAngle += orbitSpeed * Time.fixedDeltaTime;
        float rad = orbitAngle * Mathf.Deg2Rad;
        return GrapplePoint + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * orbitRadius;
    }

    // Fly out of orbit
    private void ReleaseOrbit() {
        tempPoint = GrapplePoint;
        grappleTarget = null;

        float rad = orbitAngle * Mathf.Deg2Rad;
        Vector2 radialDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        Vector2 tangentDirection = new Vector2(-radialDir.y, radialDir.x);
        if (orbitSpeed < 0f) tangentDirection = -tangentDirection;

        // Continue past
        whipping = true;
        whipDirection = tangentDirection;
        whipTimer = whipDuration;

        movementStateMachine.RemoveState(MovementStateMachine.State.grappleOrbiting);
        movementStateMachine.RemoveState(MovementStateMachine.State.grappled);
        movementStateMachine.AddStateNoTimer(MovementStateMachine.State.grappleWhipping);
        StartCooldown();
    }

    // Whip towards grapple point (when hitting the button again)
    private void StartWhip() {
        whipTimer = whipDuration;
        whipping = false;
        whipDirection = Vector2.zero;
        movementStateMachine.RemoveState(MovementStateMachine.State.grappled);
        movementStateMachine.AddStateNoTimer(MovementStateMachine.State.grappleWhipping);
    }

    public Vector2 GetWhipDiff() {
        if (whipping)
        {
            // Whip past with momentum
            whipTimer -= Time.fixedDeltaTime;
            if (whipTimer <= 0f)
            {
                movementStateMachine.RemoveState(MovementStateMachine.State.grappleWhipping);
                DetachHook();
                StartCooldown();
                return Vector2.zero;
            }
            return whipDirection * whipPower * Time.fixedDeltaTime;
        }

        // Pull towards grapple
        Vector2 dir = (GrapplePoint - rb.position).normalized;
        Vector2 blendedDir = (dir + testMovement.GetMoveInput() * whipSteerInfluence).normalized;

        if (Vector2.Distance(rb.position, GrapplePoint) <= whipTowardRadius)
        {
            // We save the direction so we can continue past the grapple point
            whipping = true;
            whipDirection = blendedDir;
        }

        return blendedDir * whipPower * Time.fixedDeltaTime;
    }

    private void DetachHook()
    {
        grappleTarget = null;
        movementStateMachine.RemoveState(MovementStateMachine.State.grappled);
        movementStateMachine.RemoveState(MovementStateMachine.State.grappleOrbiting);
        movementStateMachine.RemoveState(MovementStateMachine.State.grappleWhipping);
    }

    private void StartCooldown()
    {
        cooldownTimer = grappleCooldown;
        movementStateMachine.AddStateNoTimer(MovementStateMachine.State.grappleCooldown);
        StartCoroutine(RemoveCooldownState());
    }

    private IEnumerator RemoveCooldownState()
    {
        yield return new WaitForSeconds(grappleCooldown);
        movementStateMachine.RemoveState(MovementStateMachine.State.grappleCooldown);
    }

    // Grapple cable visual stuff

    private void UpdateCableVisual()
    {
        bool cableActive = firing
            || HasState(MovementStateMachine.State.grappled)
            || IsOrbiting;

        if (!cableActive)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        Vector2 endPoint = firing ? (Vector2)projectile.transform.position : GrapplePoint;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, rb.position);
        lineRenderer.SetPosition(1, endPoint);

        // stretch color and size to look cool
        if (HasState(MovementStateMachine.State.grappled) || IsOrbiting)
        {
            // Looked into lerp as recommended, seems OP
            float dist = Vector2.Distance(rb.position, GrapplePoint);
            float t = Mathf.Clamp01((dist - maxLength * 0.85f) / (maxLength * 0.15f));
            Color c = Color.Lerp(cableNormalColor, cableStretchColor, t);
            float w = Mathf.Lerp(cableNormalWidth, cableStretchWidth, t);
            lineRenderer.startColor = lineRenderer.endColor = c;
            lineRenderer.startWidth = lineRenderer.endWidth = w;
        }
        else
        {
            lineRenderer.startColor = lineRenderer.endColor = cableNormalColor;
            lineRenderer.startWidth = lineRenderer.endWidth = cableNormalWidth;
        }
    }

    private bool HasState(MovementStateMachine.State state) => movementStateMachine.HasState(state);
}