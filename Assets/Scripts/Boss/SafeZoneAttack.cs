using System.Collections;
using UnityEngine;

public class SafeZoneAttack : BossAttack
{
    [Header("References")]
    public BoxCollider2D arenaBounds;
    public GameObject safeZoneMask;
    public SpriteRenderer dangerOverlay;

    [Header("Settings")]
    public float safeZoneRadius = 2f;
    public float damage = 50f;
    public float warningDuration = 1f;
    public float displayDuration = 0.3f;
    public float minDistanceFromBoss = 3f;
    public Color warningColor = new Color(1f, 0.4f, 0f, 0.5f);
    public Color fireColor = new Color(1f, 0.1f, 0f, 1f);

    public override IEnumerator Execute(BossController boss, Transform player)
    {
        Vector2 spawnPos = GetSafePosition(boss.transform.position);

        safeZoneMask.transform.position = spawnPos;
        safeZoneMask.SetActive(true);

        dangerOverlay.color = warningColor;
        dangerOverlay.gameObject.SetActive(true);

        yield return new WaitForSeconds(warningDuration);

        dangerOverlay.color = fireColor;

        float dist = Vector2.Distance(player.position, spawnPos);
        if (dist > safeZoneRadius)
        {
            Player p = player.GetComponent<Player>();
            if (p != null) p.TakeDamage((int)damage);
        }

        yield return new WaitForSeconds(displayDuration);

        safeZoneMask.SetActive(false);
        dangerOverlay.gameObject.SetActive(false);
    }

    private Vector2 GetSafePosition(Vector2 bossPos)
    {
        Bounds bounds = arenaBounds.bounds;

        float minX = bounds.min.x + safeZoneRadius;
        float maxX = bounds.max.x - safeZoneRadius;
        float minY = bounds.min.y + safeZoneRadius;
        float maxY = bounds.max.y - safeZoneRadius;

        Vector2 pos;
        do
        {
            pos = new Vector2(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY)
            );
        } while (Vector2.Distance(pos, bossPos) < minDistanceFromBoss);

        return pos;
    }
}