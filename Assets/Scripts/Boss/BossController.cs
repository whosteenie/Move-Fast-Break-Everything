using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class BossController : MonoBehaviour
{
    public enum BossPhase { Idle, SlowStart, PhaseOne, Dead }

    [Header("Stats")]
    public float maxHealth = 300f;
    private float _health;
    private BossPhase _phase = BossPhase.Idle;

    [Header("SlowStart")]
    public float slowStartDuration = 3f;

    public CinemachineCamera virtualCamera;
    public Transform playerTransform;
    public BossHealthBar healthBar;

    // Can add more phase arrays, such that we pull from different pools at different phases of boss fight
    [Header("Phases")]
    public BossAttack[] phaseOneAttacks;

    private void Awake()
    {
        _health = maxHealth;
    }

    private void Start()
    {
        Activate();
    }

    public void Activate()
    {
        virtualCamera.Follow = transform;
        healthBar.Initialize(maxHealth, slowStartDuration);
        SetPhase(BossPhase.SlowStart);
    }

    private void SetPhase(BossPhase newPhase)
    {
        _phase = newPhase;
        StopAllCoroutines();

        switch (_phase)
        {
            case BossPhase.PhaseOne:
                StartCoroutine(PhaseOne());
                break;
            case BossPhase.Dead:
                StartCoroutine(DieCo());
                break;
            case BossPhase.SlowStart:
                StartCoroutine(SlowStart());
                break;
        }
    }

    public void TakeDamage(float amount)
    {
        _health -= amount;
        healthBar.Refresh(_health);
        if (_health <= 0 && _phase != BossPhase.Dead)
            SetPhase(BossPhase.Dead);
    }

    private IEnumerator SlowStart()
    {
        yield return new WaitForSeconds(slowStartDuration);
        SetPhase(BossPhase.PhaseOne);
    }

    private IEnumerator PhaseOne()
    {
        while (true)
        {
            yield return StartCoroutine(RunRandomAttack(phaseOneAttacks));
            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator DieCo()
    {
        virtualCamera.Follow = playerTransform;
        healthBar.gameObject.SetActive(false);
        Destroy(gameObject, 1f);
        yield break;
    }

    private IEnumerator RunRandomAttack(BossAttack[] attacks)
    {
        if (attacks.Length == 0) yield break;
        int index = Random.Range(0, attacks.Length);
        yield return StartCoroutine(attacks[index].Execute(this, playerTransform));
    }
}