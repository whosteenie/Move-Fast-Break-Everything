using UnityEngine;

public class PlayerHealthBar : MonoBehaviour
{
    private static Sprite cachedSprite;

    private SpriteRenderer backgroundRenderer;
    private SpriteRenderer fillRenderer;

    private Vector2 barSize = new(1.4f, 0.18f);
    private Vector3 barOffset = new(0f, -0.85f, 0f);
    private Color fillColor = new(0.2f, 0.9f, 0.3f, 1f);
    private Color backgroundColor = new(0.1f, 0.1f, 0.1f, 0.85f);

    private void Awake()
    {
        EnsureRenderers();
        ApplyVisuals();
    }

    public void Configure(Vector2 size, Vector3 offset, Color fill, Color background)
    {
        barSize = size;
        barOffset = offset;
        fillColor = fill;
        backgroundColor = background;

        EnsureRenderers();
        ApplyVisuals();
    }

    public void Refresh(int currentHealth, int maxHealth)
    {
        EnsureRenderers();

        float fillPercent = maxHealth <= 0 ? 0f : Mathf.Clamp01((float)currentHealth / maxHealth);
        float fillWidth = barSize.x * fillPercent;
        float leftEdge = -barSize.x * 0.5f;

        fillRenderer.transform.localScale = new Vector3(fillWidth, barSize.y, 1f);
        fillRenderer.transform.localPosition = new Vector3(leftEdge + (fillWidth * 0.5f), 0f, -0.01f);
        fillRenderer.enabled = fillPercent > 0f;
    }

    private void EnsureRenderers()
    {
        if (backgroundRenderer == null)
        {
            backgroundRenderer = GetOrCreateRenderer("Background");
        }

        if (fillRenderer == null)
        {
            fillRenderer = GetOrCreateRenderer("Fill");
        }
    }

    private void ApplyVisuals()
    {
        transform.localPosition = barOffset;

        backgroundRenderer.sprite = GetBarSprite();
        backgroundRenderer.transform.localScale = new Vector3(barSize.x, barSize.y, 1f);
        backgroundRenderer.transform.localPosition = Vector3.zero;
        backgroundRenderer.color = backgroundColor;
        backgroundRenderer.sortingOrder = 10;

        fillRenderer.sprite = GetBarSprite();
        fillRenderer.color = fillColor;
        fillRenderer.sortingOrder = 11;
    }

    private SpriteRenderer GetOrCreateRenderer(string objectName)
    {
        Transform child = transform.Find(objectName);
        GameObject childObject;

        if (child == null)
        {
            childObject = new GameObject(objectName);
            childObject.transform.SetParent(transform, false);
        }
        else
        {
            childObject = child.gameObject;
        }

        SpriteRenderer renderer = childObject.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = childObject.AddComponent<SpriteRenderer>();
        }

        return renderer;
    }

    private static Sprite GetBarSprite()
    {
        if (cachedSprite != null)
        {
            return cachedSprite;
        }

        Texture2D texture = new(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        cachedSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        return cachedSprite;
    }
}
