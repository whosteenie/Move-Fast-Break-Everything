using System.Collections.Generic;
using UnityEngine;

public class Circle : MonoBehaviour
{
    

    public float radius = 2f;
    public float rotationSpeed = 100f;
    public GameObject bulletPrefab;

    private List<Transform> circlingBullets = new List<Transform>();
    private float currentRotation = 0f;
    public void Start()
    {
        AddWeapon();
        AddWeapon();
        AddWeapon();
    

    }
    void Update()
    {
        if (circlingBullets.Count == 0) return;

        
        currentRotation += rotationSpeed * Time.deltaTime;

        int count = circlingBullets.Count;

        for (int i = 0; i < count; i++)
        {
            float angle = currentRotation + i * (360f / count);
            float rad = angle * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * radius;

            Transform weapon = circlingBullets[i];
            Rigidbody2D rb = weapon.GetComponent<Rigidbody2D>();
            rb.MovePosition(transform.position + offset);

            
            weapon.right = (weapon.position - transform.position).normalized;
        }
    }

  
    public void AddWeapon()
    {
        GameObject bullet = Instantiate(bulletPrefab);
        circlingBullets.Add(bullet.transform);
    }

    

    

}
