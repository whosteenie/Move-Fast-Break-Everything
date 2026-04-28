using UnityEngine;

public abstract class MagneticPickup : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D pickupCollider;
    [SerializeField] private float launchSpeed = 5f;
    [SerializeField] private float launchDuration = 0.15f;
    [SerializeField] private float magnetSpeed = 10f;
    [SerializeField] private float magnetAcceleration = 30f;

    private Transform _magnetTarget;
    private Vector2 _launchVelocity;
    private float _launchTimer;
    private float _currentMagnetSpeed;
    private bool _isMagnetized;
    private bool _hasTriggeredPickupRange;
    private bool _isCollected;

    protected virtual void Awake()
    {
        CacheReferences();
    }

    protected virtual void OnValidate()
    {
        CacheReferences();
    }

    private void FixedUpdate()
    {
        if (_magnetTarget == null)
        {
            return;
        }

        if (_launchTimer > 0f)
        {
            _launchTimer -= Time.fixedDeltaTime;
            MovePickup(_launchVelocity * Time.fixedDeltaTime);
            return;
        }

        if (!_isMagnetized)
        {
            return;
        }

        _currentMagnetSpeed = Mathf.MoveTowards(_currentMagnetSpeed, magnetSpeed, magnetAcceleration * Time.fixedDeltaTime);

        var directionToPlayer = ((Vector2)_magnetTarget.position - GetCurrentPosition()).normalized;
        MovePickup(directionToPlayer * (_currentMagnetSpeed * Time.fixedDeltaTime));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isCollected || !collision.CompareTag("Player"))
        {
            return;
        }

        if (IsTouchingPickupCollider(collision))
        {
            Collect(collision.gameObject);
            return;
        }

        if (_hasTriggeredPickupRange)
        {
            return;
        }

        StartMagnetSequence(collision.transform);
    }

    protected abstract bool TryCollect(GameObject playerObject);

    private void Collect(GameObject playerObject)
    {
        if (!TryCollect(playerObject))
        {
            return;
        }

        _isCollected = true;
        Destroy(gameObject);
    }

    private void StartMagnetSequence(Transform playerTransform)
    {
        _magnetTarget = playerTransform;
        _hasTriggeredPickupRange = true;
        _isMagnetized = true;
        _launchTimer = launchDuration;
        _currentMagnetSpeed = 0f;

        var launchDirection = GetCurrentPosition() - (Vector2)playerTransform.position;
        if (launchDirection.sqrMagnitude < 0.0001f)
        {
            launchDirection = Random.insideUnitCircle.normalized;
        }
        else
        {
            launchDirection.Normalize();
        }

        _launchVelocity = launchDirection * launchSpeed;
    }

    private Vector2 GetCurrentPosition()
    {
        return rb != null ? rb.position : transform.position;
    }

    private void MovePickup(Vector2 delta)
    {
        if (rb != null)
        {
            rb.MovePosition(rb.position + delta);
            return;
        }

        transform.position += (Vector3)delta;
    }

    private bool IsTouchingPickupCollider(Collider2D otherCollider)
    {
        return pickupCollider != null && pickupCollider.Distance(otherCollider).isOverlapped;
    }

    private void CacheReferences()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (pickupCollider == null)
        {
            pickupCollider = GetComponent<Collider2D>();
        }
    }
}
