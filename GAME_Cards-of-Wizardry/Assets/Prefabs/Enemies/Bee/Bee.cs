using System.Collections;
using UnityEngine;

public class Bee : MonoBehaviour
{
    private EnemyStats stats;
    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private Renderer materialRenderer;

    public enum BeeState { Searching, Aggressive }
    public BeeState currentState = BeeState.Searching;

    [Header("Movement")]
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float dodgeSpeed = 5f;
    [SerializeField] private float dodgeCooldown = 1f;
    [SerializeField] private float dodgeDistance = 2f;
    private float dodgeTimer;
    private Vector2 dodgeDirection;

    [Header("Attack")]
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float initialShootingRange = 7f;
    [Header("Basic Attack")]
    [SerializeField] private float shootingCooldown = 2f;
    [SerializeField] private int bulletsPerShot = 1;
    [SerializeField] private int burstCount = 1;
    [SerializeField] private float burstDelay = 0.2f;
    [SerializeField] private float spreadAngle = 30f;
    [Header("Enraged Attack")]
    [SerializeField] private float enragedShootingCooldown = 2f;
    [SerializeField] private int enragedBulletsPerShot = 1;
    [SerializeField] private int enragedBurstCount = 1;
    [SerializeField] private float enragedBurstDelay = 0.2f;
    [SerializeField] private float enragedSpreadAngle = 30f;
    private float shootingRange;
    private float shootingTimer;
    private bool isFiring = false;
    private bool isEnraged = false;

    [Header("Enraged Material")]
    [SerializeField] protected Material enrageMaterial;

    [Header("UI")]
    [SerializeField] private Transform healthBarCanvas;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        stats = GetComponent<EnemyStats>();
        rb = GetComponent<Rigidbody2D>();
        materialRenderer = GetComponent<Renderer>();
    }

    private void Start()
    {
        player = GameManager.Instance.GetPlayerTransform();
        shootingRange = initialShootingRange;
    }

    private void Update()
    {
        if (!stats.IsAlive() || stats.IsFrozen()) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (!isEnraged && stats.GetHealth() <= stats.GetMaxHealth() * 0.33f)
        {
            EnterEnragedState();
        }

        switch (currentState)
        {
            case BeeState.Searching:
                HandleSearchingState(distanceToPlayer);
                break;

            case BeeState.Aggressive:
                HandleAggressiveState(distanceToPlayer);
                break;
        }
    }

    private void EnterEnragedState()
    {
        isEnraged = true;
        materialRenderer.material = enrageMaterial;
    }

    private void HandleSearchingState(float distanceToPlayer)
    {
        if (distanceToPlayer > shootingRange)
        {
            FlyTowardsPlayer();
        }
        else
        {
            EnterAggressiveMode();
        }
    }

    private void HandleAggressiveState(float distanceToPlayer)
    {
        if (distanceToPlayer > shootingRange * 2)
        {
            ExitAggressiveMode();
        }
        else if (!isFiring)
        {
            PrepareForAttack();
        }
    }

    private void FlyTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = direction * chaseSpeed;
        FlipGameObject();
    }

    private void EnterAggressiveMode()
    {
        currentState = BeeState.Aggressive;
        shootingRange *= 2;
    }

    private void ExitAggressiveMode()
    {
        currentState = BeeState.Searching;
        shootingRange = initialShootingRange;
    }

    private void PrepareForAttack()
    {
        FlipGameObject();
        rb.velocity = Vector2.zero;
        InitializeAttack();
        Dodge();
    }

    private void InitializeAttack()
    {
        if (shootingTimer > 0)
        {
            shootingTimer -= Time.deltaTime;
            return;
        }

        shootingTimer = isEnraged ? enragedShootingCooldown : shootingCooldown;
        StartCoroutine(FireBurst());
    }

    private IEnumerator FireBurst()
    {
        isFiring = true;
        int projectilesToShoot = isEnraged ? enragedBurstCount : burstCount;
        float delay = isEnraged ? enragedBurstDelay : burstDelay;

        for (int i = 0; i < projectilesToShoot; i++)
        {
            if (stats.IsFrozen()) break;

            animator.SetTrigger("Attack");
            yield return new WaitForSeconds(delay);
        }

        isFiring = false;
    }

    private void Dodge()
    {
        if (dodgeTimer > 0)
        {
            dodgeTimer -= Time.deltaTime;
            return;
        }

        dodgeTimer = dodgeCooldown;
        dodgeDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        Vector2 dodgeTarget = (Vector2)transform.position + dodgeDirection * dodgeDistance;
        StartCoroutine(DodgeMovement(dodgeTarget));
    }

    private IEnumerator DodgeMovement(Vector2 targetPosition)
    {
        while ((targetPosition - (Vector2)transform.position).magnitude > 0.1f)
        {
            if (stats.IsFrozen()) break;

            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            rb.velocity = direction * dodgeSpeed;
            yield return null;
        }
        rb.velocity = Vector2.zero;
    }

    private void FlipGameObject()
    {
        bool facingLeft = (player.position - transform.position).normalized.x < 0;
        transform.rotation = Quaternion.Euler(0, facingLeft ? 0 : 180, 0);

        // Flipping game object also flips the child canvas, so we need to flip canvas also to make a full rotation back to original rotation
        Vector3 healthBarEulerAngles = healthBarCanvas.localRotation.eulerAngles;
        healthBarEulerAngles.y = facingLeft ? 0 : 180;
        healthBarCanvas.localRotation = Quaternion.Euler(healthBarEulerAngles);
    }

    // Called from animation events
    public void FireProjectile()
    {
        if (bulletsPerShot > 1 || (isEnraged && enragedBulletsPerShot > 1))
        {
            FireShotgun();
        }
        else
        {
            FireSingleProjectile();
        }
    }

    private void FireSingleProjectile()
    {
        CreateProjectile((player.position - projectileSpawnPoint.position).normalized);
    }

    private void FireShotgun()
    {
        int bulletsToFire = isEnraged ? enragedBulletsPerShot : bulletsPerShot;
        float angleStep = isEnraged ? enragedSpreadAngle / (bulletsToFire - 1) : spreadAngle / (bulletsToFire - 1);
        float startAngle = isEnraged ? -enragedSpreadAngle / 2 : -spreadAngle / 2;

        for (int i = 0; i < bulletsToFire; i++)
        {
            float currentAngle = startAngle + (i * angleStep);
            Quaternion rotation = Quaternion.Euler(new Vector3(0, 0, currentAngle));
            Vector3 direction = rotation * (player.position - projectileSpawnPoint.position).normalized;
            CreateProjectile(direction);
        }
    }

    private void CreateProjectile(Vector2 direction)
    {
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
        projectile.GetComponent<EnemyFireball>().SetDirection(direction);
    }
}
