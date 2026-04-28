using System.Collections;
using UnityEngine;

public abstract class BossAttack : MonoBehaviour
{
    public abstract IEnumerator Execute(BossController boss, Transform player);
}