using System.Collections;
using UnityEngine;

public class SpinBeamAttack : BossAttack
{
    [Header("References")]
    public LineRenderer lineRenderer;

    [Header("Beam")]
    public float beamDistance = 30f;
    public float beamDamage = 30f;
    public float warningDuration = 1f;
    public float rotationSpeed = 90f;
    public float spinDuration = 3f;
    public Color beamWarningColor = new Color(1f, 0.4f, 0f, 0.5f);
    public Color beamFireColor = new Color(1f, 0.1f, 0f, 1f);

    public override IEnumerator Execute(BossController boss, Transform player)
    {
        float startAngle = Random.Range(0f, 360f);
        float direction = Random.value > 0.5f ? 1f : -1f;
        Vector2 dir = AngleToDir(startAngle);
        Vector3 endPoint = boss.transform.position + (Vector3)(dir * beamDistance);

        // warning
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.startColor = beamWarningColor;
        lineRenderer.endColor = beamWarningColor;
        lineRenderer.SetPosition(0, boss.transform.position);
        lineRenderer.SetPosition(1, endPoint);
        lineRenderer.enabled = true;

        yield return new WaitForSeconds(warningDuration);

        // fire
        lineRenderer.startWidth = 0.6f;
        lineRenderer.endWidth = 0.6f;
        lineRenderer.startColor = beamFireColor;
        lineRenderer.endColor = beamFireColor;

        float elapsed = 0f;
        float currentAngle = startAngle;

        while (elapsed < spinDuration)
        {
            currentAngle += rotationSpeed * direction * Time.deltaTime;
            dir = AngleToDir(currentAngle);
            endPoint = boss.transform.position + (Vector3)(dir * beamDistance);

            lineRenderer.SetPosition(0, boss.transform.position);
            lineRenderer.SetPosition(1, endPoint);

            RaycastHit2D[] hits = Physics2D.RaycastAll(boss.transform.position, dir, beamDistance);
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider.CompareTag("Player"))
                {
                    Player p = hit.collider.GetComponent<Player>();
                    if (p != null) p.TakeDamage((int)beamDamage);
                    break;
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        lineRenderer.enabled = false;
    }

    private Vector2 AngleToDir(float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }
}