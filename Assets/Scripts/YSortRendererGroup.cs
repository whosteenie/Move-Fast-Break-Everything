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
    [SerializeField] private int sortOrderOffset = 1000;
    [SerializeField] private int minimumSortingOrder = 0;
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

        if (renderers == null || renderers.Length == 0)
        {
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
        BaseSortingOrder = Mathf.Max(minimumSortingOrder, BaseSortingOrder);

        if (renderers == null)
        {
            return;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            var renderer = renderers[i].renderer;
            if (renderer == null)
            {
                continue;
            }

            renderer.sortingOrder = BaseSortingOrder + renderers[i].orderOffset;
        }
    }
}
