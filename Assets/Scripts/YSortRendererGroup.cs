using UnityEngine;

[DisallowMultipleComponent]
public class YSortRendererGroup : MonoBehaviour
{
    [System.Serializable]
    private struct RendererBinding
    {
        public SpriteRenderer renderer;
        public int orderOffset;
    }

    [SerializeField] private Transform sortAnchor;
    [SerializeField] private Vector3 sortAnchorLocalOffset;
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int sortOrderOffset = 1000;
    [SerializeField] private int minimumSortingOrder;
    [SerializeField] private int sortPrecision = 100;
    [SerializeField] private RendererBinding[] renderers;

    public int BaseSortingOrder { get; private set; }

    public int GetOrderWithOffset(int orderOffset)
    {
        return BaseSortingOrder + orderOffset;
    }

    private void Reset()
    {
        sortAnchor = transform;

        if(renderers != null && renderers.Length != 0) return;
        
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            renderers = new[]
            {
                new RendererBinding
                {
                    renderer = spriteRenderer,
                    orderOffset = 0
                }
            };
        }
    }

    private void LateUpdate()
    {
        ApplySorting();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (sortAnchor == null)
        {
            sortAnchor = transform;
        }

        ApplySorting();
    }
#endif

    private void ApplySorting()
    {
        var anchor = sortAnchor != null ? sortAnchor : transform;
        var sortPosition = anchor.TransformPoint(sortAnchorLocalOffset);
        BaseSortingOrder = sortOrderOffset - Mathf.RoundToInt(sortPosition.y * sortPrecision);

        if (renderers == null)
        {
            return;
        }

        for (var i = 0; i < renderers.Length; i++)
        {
            var spriteRenderer = renderers[i].renderer;
            if (spriteRenderer == null)
            {
                continue;
            }

            spriteRenderer.sortingLayerName = sortingLayerName;
            spriteRenderer.sortingOrder = BaseSortingOrder + renderers[i].orderOffset;
        }
    }
}
