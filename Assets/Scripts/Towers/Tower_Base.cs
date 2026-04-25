using UnityEngine;

public abstract class Tower_Base : MonoBehaviour
{
    protected bool playerInRange = false;
    protected Stats currentStats;

    protected virtual void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (currentStats != null)
            {
                ApplyEffect(currentStats);
            }
        }
    }

    protected abstract void ApplyEffect(Stats stats);

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            currentStats = collision.GetComponent<Stats>();
        }
    }

    protected virtual void OntriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            currentStats = null;
        }
    }
}
