using System.Collections;
using UnityEngine;

public class CrossBeamAttack : BossAttack
{
    public float beamDamage = 50f;
    public float beamDistance = 30f;
    public float beamDisplayDuration = 0.3f;
    public Color beamWarningColor = new Color(1f, 0.4f, 0f, 0.5f);
    public Color beamFireColor = new Color(1f, 0.1f, 0f, 1f);

    private LineRenderer[] _lineRenderers;

    private readonly Vector2[] _directions = {
        Vector2.up, Vector2.down, Vector2.left, Vector2.right
    };

    private void Awake()
    {
        _lineRenderers = GetComponentsInChildren<LineRenderer>();
        foreach (var lr in _lineRenderers)
            lr.enabled = false;
        Debug.Log("Line renderers found: " + _lineRenderers.Length);
    }

    public override IEnumerator Execute(BossController boss, Transform player)
    {
        for (int i = 0; i < 4; i++)
        {
            Vector3 endPoint = boss.transform.position + (Vector3)(_directions[i] * beamDistance);
            _lineRenderers[i].startWidth = 0.05f;
            _lineRenderers[i].endWidth = 0.05f;
            _lineRenderers[i].startColor = beamWarningColor;
            _lineRenderers[i].endColor = beamWarningColor;
            _lineRenderers[i].SetPosition(0, boss.transform.position);
            _lineRenderers[i].SetPosition(1, endPoint);
            _lineRenderers[i].enabled = true;
        }

        yield return new WaitForSeconds(1f);

        for (int i = 0; i < 4; i++)
        {
            _lineRenderers[i].startWidth = 0.6f;
            _lineRenderers[i].endWidth = 0.6f;
            _lineRenderers[i].startColor = beamFireColor;
            _lineRenderers[i].endColor = beamFireColor;

            RaycastHit2D hit = Physics2D.Raycast(boss.transform.position, _directions[i], beamDistance,~LayerMask.GetMask("Boss"));
            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                Player p = hit.collider.GetComponent<Player>();
                if (p != null) p.TakeDamage((int)beamDamage);
            }
        }

        yield return new WaitForSeconds(beamDisplayDuration);
        foreach (var lr in _lineRenderers)
            lr.enabled = false;
    }
}