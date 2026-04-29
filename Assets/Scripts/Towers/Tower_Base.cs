using UnityEngine;

public abstract class Tower_Base : MonoBehaviour
{
    private static Tower_Base currentInteractable;

    protected bool playerInRange = false;
    protected Stats currentStats;

    public static void TryInteractCurrent()
    {
        if (currentInteractable != null)
        {
            currentInteractable.TryInteract();
        }
    }

    private void TryInteract()
    {
        if (!playerInRange || currentStats == null)
        {
            return;
        }

        ApplyEffect(currentStats);
    }

    protected abstract void ApplyEffect(Stats stats);

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            currentStats = collision.GetComponent<Stats>();
            currentInteractable = this;
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            currentStats = null;
            if (currentInteractable == this)
            {
                currentInteractable = null;
            }
        }
    }

    protected virtual void OnDisable()
    {
        if (currentInteractable == this)
        {
            currentInteractable = null;
        }
    }
}
