using UnityEngine;

public class SpeedTower : MonoBehaviour
{
    public int speedIncrease = 1;

    private bool playerInRange = false;
    private Stats stats;


    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (stats != null)
            {
                stats.speedMultiplier += speedIncrease;
                Debug.Log("Speed updated" + stats.speedMultiplier);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            stats = collision.GetComponent<Stats>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            stats = null;
        }
    }

}
