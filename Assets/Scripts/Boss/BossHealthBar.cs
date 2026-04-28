using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    [Header("References")]
    public Image fill;
    public Image background;
    public Image border;

    [Header("Colors")]
    public Color fillColor = new Color(0.9f, 0.2f, 0.2f, 1f);
    public Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.85f);
    public Color borderColor = new Color(0f, 0f, 0f, 1f);

    private float _maxHealth;
    private bool _initialized = false;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Initialize(float maxHealth, float slowStartDuration)
    {
        _maxHealth = maxHealth;
        _initialized = true;

        fill.color = fillColor;
        background.color = backgroundColor;
        border.color = borderColor;

        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillAmount = 0f;

        gameObject.SetActive(true);
        StartCoroutine(FillAnimation(slowStartDuration));
    }

    public void Refresh(float currentHealth)
    {
        if (!_initialized) return;
        fill.fillAmount = Mathf.Clamp01(currentHealth / _maxHealth);
    }

    private IEnumerator FillAnimation(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            fill.fillAmount = Mathf.Lerp(0f, 1f, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        fill.fillAmount = 1f;
    }
}