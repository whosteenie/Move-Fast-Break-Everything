using System.Collections;
using UnityEngine;

public class BeamAttack : BossAttack
{
    [Header("Beam")]
    public float beamDamage = 50f;
    public float beamDistance = 30f;
    public float beamDisplayDuration = 0.3f;
    public Color beamWarningColor = new Color(1f, 0.4f, 0f, 0.5f);
    public Color beamFireColor = new Color(1f, 0.1f, 0f, 1f);

    private LineRenderer _lineRenderer;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.enabled = false;
    }

    public override IEnumerator Execute(BossController boss, Transform player)
    {
        Vector2 lockedDir = (player.position - boss.transform.position).normalized;
        Vector3 endPoint = boss.transform.position + (Vector3)(lockedDir * beamDistance);

        _lineRenderer.startWidth = 0.05f;
        _lineRenderer.endWidth = 0.05f;
        _lineRenderer.startColor = beamWarningColor;
        _lineRenderer.endColor = beamWarningColor;
        _lineRenderer.SetPosition(0, boss.transform.position);
        _lineRenderer.SetPosition(1, endPoint);
        _lineRenderer.enabled = true;

        yield return new WaitForSeconds(1f);

        _lineRenderer.startWidth = 0.6f;
        _lineRenderer.endWidth = 0.6f;
        _lineRenderer.startColor = beamFireColor;
        _lineRenderer.endColor = beamFireColor;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, lockedDir, beamDistance, ~LayerMask.GetMask("Boss"));
        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            Player player2 = hit.collider.GetComponent<Player>();
            if (player2 != null)
                player2.TakeDamage((int)beamDamage);
        }

        yield return new WaitForSeconds(beamDisplayDuration);
        _lineRenderer.enabled = false;
    }
}