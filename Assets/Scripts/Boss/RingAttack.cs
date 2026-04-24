using System.Collections;
using UnityEngine;

public class RingAttack : BossAttack
{
    [Header("References")]
    public GameObject ringPrefab;

    [Header("Settings")]
    public float warningRadius = 1f;
    public float maxRadius = 15f;
    public float expandSpeed = 3f;
    public float damage = 20f;
    public float lineWidth = 0.2f;
    public int segments = 64;
    public Color warningColor = new Color(1f, 0.4f, 0f, 0.5f);
    public Color fireColor = new Color(1f, 0.1f, 0f, 1f);
    public float warningDuration = 1f;

    public override IEnumerator Execute(BossController boss, Transform player)
    {
        GameObject ring = Instantiate(ringPrefab, boss.transform.position, Quaternion.identity);
        LineRenderer lr = ring.GetComponent<LineRenderer>();
        EdgeCollider2D col = ring.GetComponent<EdgeCollider2D>();
        RingDamage ringDamage = ring.GetComponent<RingDamage>();

        // warning state
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.startColor = warningColor;
        lr.endColor = warningColor;
        lr.loop = true;
        ringDamage.enabled = false;
        DrawCircle(lr, col, warningRadius);

        yield return new WaitForSeconds(warningDuration);

        // fire
        lr.startColor = fireColor;
        lr.endColor = fireColor;
        ringDamage.enabled = true;
        ringDamage.damage = damage;

        float currentRadius = warningRadius;

        while (ring != null)
        {
            if (currentRadius >= maxRadius)
            {
                Destroy(ring);
                break;
            }

            currentRadius += expandSpeed * Time.deltaTime;
            DrawCircle(lr, col, currentRadius);

            yield return null;
        }
    }

    private void DrawCircle(LineRenderer lr, EdgeCollider2D col, float radius)
    {
        lr.positionCount = segments + 1;
        Vector2[] points = new Vector2[segments + 1];

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * 2f * Mathf.PI / segments;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
            lr.SetPosition(i, pos);
            points[i] = new Vector2(pos.x, pos.y);
        }

        col.SetPoints(new System.Collections.Generic.List<Vector2>(points));
    }
}