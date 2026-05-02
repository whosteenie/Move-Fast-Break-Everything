using System.Collections;
using UnityEngine;

public class CircleBullet : MonoBehaviour
{
    public WeaponSO weaponSO;
    
    public float damageDelay = 0.5f; 
    private bool canTakeDamage = true;
    private float pierce=1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!canTakeDamage)
        {
            return;
        }

        DestructibleObstacle obstacle = collision.GetComponentInParent<DestructibleObstacle>();
        Enemy enemy = collision.GetComponent<Enemy>();

        if (enemy != null)
        {
            enemy.TakeDamage(weaponSO.baseDamage,pierce);
            StartCoroutineCooldown();
        }
        else if (obstacle != null)
        {
            obstacle.TakeDamage(weaponSO.baseDamage);
            StartCoroutineCooldown();
        }
        
    }
    private IEnumerator StartCoroutineCooldown()
    {
        canTakeDamage = false;
        yield return new WaitForSeconds(damageDelay);
        canTakeDamage = true;
    }
}
