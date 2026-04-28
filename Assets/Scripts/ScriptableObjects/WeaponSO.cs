using UnityEngine;
[CreateAssetMenu()]
public class WeaponSO : ScriptableObject
{
    public float fireRate = 2f;
    public float cooldown = 1f;
    public float bulletSpeed = 10f;
    public int baseDamage = 1;
    public GameObject bulletPrefab;
 
}
