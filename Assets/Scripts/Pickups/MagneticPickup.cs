using UnityEngine;

public abstract class MagneticPickup : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D pickupCollider;
    [SerializeField] private SpriteRenderer visualRenderer;
    [SerializeField] private float launchSpeed = 5f;
    [SerializeField] private float launchDuration = 0.15f;
    [SerializeField] private float magnetSpeed = 10f;
    [SerializeField] private float magnetAcceleration = 30f;
    [SerializeField] private string groundedSortingLayerName = "GroundShadow";
    [SerializeField] private int groundedSortingOrder;
    [SerializeField] private string activeSortingLayerName = "Pickups";
    [SerializeField] private int activeSortingOrder = 800;

    private Transform _magnetTarget;
    private Vector2 _launchVelocity;
    private float _launchTimer;
    private float _currentMagnetSpeed;
    private bool _isMagnetized;
    private bool _hasStartedMagnet;
    private bool _isCollected;

    protected Transform MagnetTarget => _magnetTarget;
    protected bool IsMagnetized => _isMagnetized;

    protected virtual void Awake()
    {
        CacheReferences();
        ApplySorting();
    }

    protected virtual void OnValidate()
    {
        CacheReferences();
        ApplySorting();
    }

    private void FixedUpdate()
    {
        ApplySorting();

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
        if (_isCollected || collision.isTrigger || !collision.CompareTag("Player"))
        {
            return;
        }

        if (IsTouchingPickupCollider(collision))
        {
            Collect(collision.gameObject);
        }
    }

    protected abstract bool TryCollect(GameObject playerObject);

    public void TryStartMagnetSequence(Transform playerTransform)
    {
        if (_isCollected || _hasStartedMagnet || playerTransform == null)
        {
            return;
        }

        StartMagnetSequence(playerTransform);
    }

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
        _hasStartedMagnet = true;
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

        if (visualRenderer == null)
        {
            visualRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void ApplySorting()
    {
        if (visualRenderer == null)
        {
            return;
        }

        var isActive = _hasStartedMagnet && !_isCollected;
        visualRenderer.sortingLayerName = isActive ? activeSortingLayerName : groundedSortingLayerName;
        visualRenderer.sortingOrder = isActive ? activeSortingOrder : groundedSortingOrder;
    }
}
