using UnityEngine;

public class Jump : MonoBehaviour
{
    [Header("Jump Settings")]
    public float jumpDuration = 0.5f;
    public float jumpHeight = 1.5f;
    public float scale = 0.2f;
    public float zOffset = -1f;

    [Header("Shadow")]
    public Transform shadow;
    [SerializeField] private string groundedShadowSortingLayerName = "GroundShadow";
    [SerializeField] private string airborneShadowSortingLayerName = "AirborneShadow";
    [SerializeField] private int groundedShadowSortOffset = 650;
    [SerializeField] private int airborneShadowSortOffset = 850;
    [SerializeField] private int minimumShadowSortingOrder;

    [Header("Layers")]
    public string groundedLayer = "Grounded";
    public string airborneLayer = "Airborne";

    [Header("References")]
    public Transform spriteTransform;

    [Header("Audio")]
    [SerializeField] private SoundDefinition jumpSound;
    [SerializeField] private SoundDefinition landSound;

    private bool isJumping = false;
    private float jumpTimer = 0f;
    private Vector3 spriteBasePosition;
    private Vector3 spriteBaseScale;
    private float spriteBaseZ;
    private SpriteRenderer shadowRenderer;
    private YSortRendererGroup sortGroup;

    public bool IsJumping => isJumping;

    void Awake()
    {
        spriteBasePosition = spriteTransform.localPosition;
        spriteBaseScale = spriteTransform.localScale;
        spriteBaseZ = spriteTransform.position.z;
        shadowRenderer = shadow != null ? shadow.GetComponent<SpriteRenderer>() : null;
        sortGroup = GetComponent<YSortRendererGroup>();
        UpdateShadowSorting();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
            StartJump();

        if (isJumping)
            UpdateJump();

        UpdateShadowSorting();
    }

    void StartJump()
    {
        isJumping = true;
        jumpTimer = 0f;
        SetLayer(airborneLayer);
        SoundManager.Play(jumpSound);
    }

    void UpdateJump()
    {
        jumpTimer += Time.deltaTime;
        float t = jumpTimer / jumpDuration;

        if (t >= 1f)
        {
            EndJump();
            return;
        }

        // Parabola arc
        float arc = 4f * t * (1f - t);

        // We're moving sprites around, not actual position (so our location is still at the base)
        Vector3 localPos = spriteBasePosition;
        localPos.y += arc * jumpHeight;
        spriteTransform.localPosition = localPos;

        // Move z
        Vector3 worldPos = spriteTransform.position;
        worldPos.z = spriteBaseZ + arc * zOffset;
        spriteTransform.position = worldPos;

        // Scale up by scale
        spriteTransform.localScale = spriteBaseScale + Vector3.one * (arc * scale);
        if (shadow != null)
    {
        // Keep the shadow under us
        Vector3 shadowPos = shadow.localPosition;
        shadowPos.y = -0.5f;
        shadow.localPosition = shadowPos;
    }
    }

    void EndJump()
    {
        isJumping = false;
        spriteTransform.localPosition = spriteBasePosition;
        spriteTransform.localScale = spriteBaseScale;

        Vector3 worldPos = spriteTransform.position;
        worldPos.z = spriteBaseZ;
        spriteTransform.position = worldPos;

        SetLayer(groundedLayer);
        SoundManager.Play(landSound);
    }

    // Grounded / airborne layer so we can turn collisions on and off
    void SetLayer(string layerName)
    {
        gameObject.layer = LayerMask.NameToLayer(layerName);
    }

    private void UpdateShadowSorting()
    {
        if (shadowRenderer == null)
        {
            return;
        }

        var sortOrder = groundedShadowSortOffset;
        if (isJumping && sortGroup != null)
        {
            sortOrder = sortGroup.BaseSortingOrder - 1;
        }
        else if (isJumping)
        {
            sortOrder = airborneShadowSortOffset;
        }

        shadowRenderer.sortingLayerName = isJumping ? airborneShadowSortingLayerName : groundedShadowSortingLayerName;
        shadowRenderer.sortingOrder = Mathf.Max(minimumShadowSortingOrder, sortOrder);
    }
}
