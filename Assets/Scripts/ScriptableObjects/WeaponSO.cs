using UnityEngine;
[CreateAssetMenu()]
public class WeaponSO : ScriptableObject
{
    public float coolDown = 1f;
    public float bulletSpeed = 10f;
    public int baseDamage = 1;
    public GameObject bulletPrefab;
 
}
