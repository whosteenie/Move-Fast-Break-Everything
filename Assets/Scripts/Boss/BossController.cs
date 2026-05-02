using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class BossController : MonoBehaviour
{
    public enum BossPhase { Idle, SlowStart, PhaseOne, Dead }

    [Header("Stats")]
    public float maxHealth = 300f;
    public float contactDamage = 3f;
    [SerializeField] private SoundDefinition hurtSound;
    private float damageCooldown = 1f;
    private float damageTimer = 0f;
    private float health;
    private BossPhase phase = BossPhase.Idle;

    [Header("SlowStart")]
    public float slowStartDuration = 3f;

    private CinemachineCamera virtualCamera;
    private Transform player;
    public BossHealthBar healthBar;

    // Can add more phase arrays, such that we pull from different pools at different phases of boss fight
    [Header("Phases")]
    public BossAttack[] phaseOneAttacks;

    private EnemySpawner spawner;

    private void Awake()
    {
        health = maxHealth;
    }

    public void Initialize(EnemySpawner spawner, Transform player, CinemachineCamera virtualCamera)
    {
        this.spawner = spawner;
        this.player = player;
        this.virtualCamera = virtualCamera;
        Activate();
    }

    public void Activate()
    {
        virtualCamera.Follow = transform;
        spawner.ClearAllEnemies();
        healthBar.Initialize(maxHealth, slowStartDuration);
        SetPhase(BossPhase.SlowStart);
    }

    private void SetPhase(BossPhase newPhase)
    {
        phase = newPhase;
        StopAllCoroutines();

        switch (phase)
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
        if (amount <= 0f)
        {
            return;
        }

        health -= amount;
        health = Mathf.Max(health, 0f);
        SoundManager.Play(hurtSound);
        healthBar.Refresh(health);
        if (health <= 0 && phase != BossPhase.Dead)
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
        virtualCamera.Follow = player;
        healthBar.gameObject.SetActive(false);
        if (spawner != null) {
            spawner.OnBossDied();
        }

        Destroy(gameObject, 1f);
        yield break;
    }

    private IEnumerator RunRandomAttack(BossAttack[] attacks)
    {
        if (attacks.Length == 0) yield break;
        int index = Random.Range(0, attacks.Length);
        yield return StartCoroutine(attacks[index].Execute(this, player));
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject != null && collision.gameObject.CompareTag("Player"))
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                damageTimer -= Time.deltaTime;
                if (damageTimer <= 0f)
                {
                    Player player = collision.gameObject.GetComponent<Player>();
                    if (player != null)
                    {
                        Debug.Log("TAKE DAMAGE");
                        player.TakeDamage((int)(contactDamage));
                        damageTimer = damageCooldown;
                    }
                }
            }
        }
    }
}
