using UnityEngine;

public class RingDamage : MonoBehaviour
{
    public float damage = 20f;

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
                player.TakeDamage((int)damage);
        }
    }
}