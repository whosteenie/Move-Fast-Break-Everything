using UnityEngine;

public class PlayerHealthBar : MonoBehaviour
{
    private static Sprite _cachedSprite;
    private const float MinScaleComponent = 0.0001f;
    private const string VisualRootName = "VisualRoot";
    private const string SortingLayerName = "WorldUI";
    private const int BackgroundOrderOffset = 10;
    private const int FillOrderOffset = 11;

    [Header("Visuals")]
    [SerializeField] private Vector2 barSize = new(0.38f, 0.035f);
    [SerializeField] private Vector3 barOffset = new(0f, -0.4f, 0f);
    [SerializeField] private Color fillColor = new(0.2f, 0.9f, 0.3f, 1f);
    [SerializeField] private Color backgroundColor = new(0.1f, 0.1f, 0.1f, 0.85f);

    private Player _owner;
    private YSortRendererGroup _sortGroup;
    private Transform _visualRoot;
    private SpriteRenderer _backgroundRenderer;
    private SpriteRenderer _fillRenderer;

    private void Awake()
    {
        if (_owner == null)
        {
            _owner = GetComponentInParent<Player>();
        }

        if (_sortGroup == null)
        {
            _sortGroup = GetComponentInParent<YSortRendererGroup>();
        }

        EnsureRenderers();
        ApplyVisuals();
    }

    private void LateUpdate()
    {
        CompensateForParentScale();
        UpdateWorldPosition();
        ApplyVisuals();
    }

    public void Initialize(Player owner)
    {
        _owner = owner;
        _sortGroup = owner != null ? owner.GetComponent<YSortRendererGroup>() : null;
        EnsureRenderers();
        ApplyVisuals();
    }

    public void Refresh(int currentHealth, int maxHealth)
    {
        EnsureRenderers();

        var size = GetBarSize();
        var fillPercent = maxHealth <= 0 ? 0f : Mathf.Clamp01((float)currentHealth / maxHealth);
        var fillWidth = size.x * fillPercent;
        var leftEdge = -size.x * 0.5f;

        var tr = _fillRenderer.transform;
        tr.localScale = new Vector3(fillWidth, size.y, 1f);
        tr.localPosition = new Vector3(leftEdge + fillWidth * 0.5f, 0f, -0.01f);
        _fillRenderer.enabled = fillPercent > 0f;
    }

    private void EnsureRenderers()
    {
        if (_visualRoot == null)
        {
            _visualRoot = GetOrCreateVisualRoot();
        }

        if (_backgroundRenderer == null)
        {
            _backgroundRenderer = GetOrCreateRenderer("Background");
        }

        if (_fillRenderer == null)
        {
            _fillRenderer = GetOrCreateRenderer("Fill");
        }
    }

    private void ApplyVisuals()
    {
        if (_backgroundRenderer == null || _fillRenderer == null)
        {
            return;
        }

        var size = GetBarSize();
        CompensateForParentScale();
        UpdateWorldPosition();

        _backgroundRenderer.sprite = GetBarSprite();
        var tr = _backgroundRenderer.transform;
        tr.localScale = new Vector3(size.x, size.y, 1f);
        tr.localPosition = Vector3.zero;
        _backgroundRenderer.color = GetBackgroundColor();
        _backgroundRenderer.sortingLayerName = SortingLayerName;
        _backgroundRenderer.sortingOrder = GetSortingOrder(BackgroundOrderOffset);

        _fillRenderer.sprite = GetBarSprite();
        _fillRenderer.color = GetFillColor();
        _fillRenderer.sortingLayerName = SortingLayerName;
        _fillRenderer.sortingOrder = GetSortingOrder(FillOrderOffset);

        if (_owner != null)
        {
            Refresh(_owner.CurrentHealth, _owner.MaxHealth);
        }
    }

    private void UpdateWorldPosition()
    {
        if (_visualRoot == null)
        {
            return;
        }

        var anchorTransform = _owner != null ? _owner.transform : transform;
        var offset = GetBarOffset();
        var tr = _visualRoot;
        tr.position = anchorTransform.position + new Vector3(offset.x, offset.y, 0f);
        tr.rotation = Quaternion.identity;
        tr.position = new Vector3(tr.position.x, tr.position.y, anchorTransform.position.z + offset.z);
    }

    private void CompensateForParentScale()
    {
        if (_visualRoot == null)
        {
            return;
        }

        var anchorTransform = _owner != null ? _owner.transform : transform;
        var parentLossyScale = anchorTransform.lossyScale;
        _visualRoot.localScale = new Vector3(
            1f / Mathf.Max(Mathf.Abs(parentLossyScale.x), MinScaleComponent),
            1f / Mathf.Max(Mathf.Abs(parentLossyScale.y), MinScaleComponent),
            1f / Mathf.Max(Mathf.Abs(parentLossyScale.z), MinScaleComponent));
    }

    private Transform GetOrCreateVisualRoot()
    {
        var visualRoot = transform.Find(VisualRootName);
        if (visualRoot != null)
        {
            return visualRoot;
        }

        var visualRootObject = new GameObject(VisualRootName);
        visualRootObject.transform.SetParent(transform, false);
        return visualRootObject.transform;
    }

    private SpriteRenderer GetOrCreateRenderer(string objectName)
    {
        var child = _visualRoot.Find(objectName);
        GameObject childObject;

        if (child == null)
        {
            childObject = new GameObject(objectName);
            childObject.transform.SetParent(_visualRoot, false);
        }
        else
        {
            childObject = child.gameObject;
        }

        var spriteRenderer = childObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = childObject.AddComponent<SpriteRenderer>();
        }

        return spriteRenderer;
    }

    private Vector2 GetBarSize()
    {
        return barSize;
    }

    private Vector3 GetBarOffset()
    {
        return barOffset;
    }

    private int GetSortingOrder(int orderOffset)
    {
        return _sortGroup != null ? _sortGroup.GetOrderWithOffset(orderOffset) : orderOffset;
    }

    private Color GetFillColor()
    {
        return fillColor;
    }

    private Color GetBackgroundColor()
    {
        return backgroundColor;
    }

    private static Sprite GetBarSprite()
    {
        if (_cachedSprite != null)
        {
            return _cachedSprite;
        }

        Texture2D texture = new(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        _cachedSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        return _cachedSprite;
    }
}
