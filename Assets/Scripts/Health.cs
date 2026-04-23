using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    public int def = 0;
    private int currentHealth;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    public void TakeDamage(int damage)
    {
        int effectiveDamage = Mathf.Max(damage - def, 0);
        currentHealth -= effectiveDamage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // Handle death logic here
        Debug.Log(gameObject.name + " has died.");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
