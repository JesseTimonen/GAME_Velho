using System.Collections;
using UnityEngine;

public class BossStageOne : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float closestRadius = 3f;
    [SerializeField] private float furthestRadius = 8f;
    [SerializeField] private float teleportCooldownMin = 3;
    [SerializeField] private float teleportCooldownMax = 5;
    private float teleportTimer;
    private float teleportRadius = 8f;
    private bool isTeleporting = false;

    [Header("Fireball Attack")]
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private float fireballCooldown = 5f;
    [SerializeField] private int fireballBurstCount = 3;
    [SerializeField] private float fireballBurstDelay = 0.4f;
    private float fireballTimer;

    [Header("Shotgun Attack")]
    [SerializeField] private GameObject shotgunProjectilePrefab;
    [SerializeField] private float shotgunCooldown = 7f;
    [SerializeField] private float shotgunSpreadAngle = 50f;
    [SerializeField] private float enragedShotgunSpreadAngle = 80f;
    [SerializeField] private int shotgunBulletCount = 5;
    [SerializeField] private int enragedShotgunBulletCount = 8;
    private float shotgunTimer;

    [Header("Meteor Shower Attack")]
    [SerializeField] private GameObject meteorPrefab;
    [SerializeField] private float meteorShowerCooldown = 15f;
    [SerializeField] private int meteorCount = 10;
    [SerializeField] private float meteorShowerDuration = 5f;
    [SerializeField] private float meteorSpawnRadius = 10f;
    private float meteorShowerTimer;

    [Header("Healing")]
    [SerializeField] private ParticleSystem healingBurstParticles;
    [SerializeField] private float healCooldown = 20f;
    [SerializeField] private int healingAmount = 50;
    private float healTimer;

    [Header("Reflect")]
    [SerializeField] private float reflectCooldown = 10f;
    [SerializeField] private float reflectDuration = 5f;
    [SerializeField] private float reflectIntensity = 0.5f;
    private float reflectTimer;

    [Header("Damage Reduction")]
    [SerializeField] private float damageReductionCooldown = 15f;
    [SerializeField] private float damageReductionDuration = 5f;
    [SerializeField] private float damageReductionStrength = 0.5f;
    private float damageReductionTimer;

    [Header("Enrage")]
    [Range(0f, 1f)] public float enrageThreshold = 0.4f;
    private bool isEnraged = false;

    [Header("Materials")]
    [SerializeField] protected Material dissolveMaterial;
    [SerializeField] protected Material outlineMaterial;
    [SerializeField] protected float dissolveSpeed = 3f;
    [SerializeField] protected float materialSwapSpeed = 5f;

    private EnemyStats stats;
    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private BoxCollider2D boxCollider;

    [Header("Phase 2 Bosses")]
    [SerializeField] private GameObject boss_V;
    [SerializeField] private GameObject boss_E;
    [SerializeField] private GameObject boss_L;
    [SerializeField] private GameObject boss_H;
    [SerializeField] private GameObject boss_O;

    private enum BossState { Moving, UsingSkills }
    private BossState bossState;

    private bool furthestRadiusExtended = false;
    private float originalFurthestRadius;

    private void Awake()
    {
        stats = GetComponent<EnemyStats>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        dissolveMaterial = GetComponent<Renderer>().material;
    }

    private void Start()
    {
        player = GameManager.Instance.GetPlayerTransform();

        fireballTimer = fireballCooldown;
        shotgunTimer = shotgunCooldown;
        reflectTimer = reflectCooldown;
        damageReductionTimer = damageReductionCooldown;
        meteorShowerTimer = meteorShowerCooldown;
        healTimer = healCooldown;
        teleportTimer = Random.Range(teleportCooldownMin, teleportCooldownMax);

        originalFurthestRadius = furthestRadius;

        bossState = BossState.Moving;
        animator.SetBool("isMoving", true);

        // Used to auto enable health bar on spawn
        stats.AddHealth(1);
    }

    private void Update()
    {
        if (!stats.IsAlive() || stats.IsFrozen()) return;

        CheckEnrageMode();
        HandleTeleportation();

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (bossState == BossState.Moving)
        {
            MoveToIdealRadius(distanceToPlayer);

            if (distanceToPlayer >= closestRadius && distanceToPlayer <= furthestRadius)
            {
                ExtendFurthestRadius();
                bossState = BossState.UsingSkills;
                animator.SetBool("isMoving", false);
            }
        }
        else if (bossState == BossState.UsingSkills)
        {
            if (distanceToPlayer < closestRadius || distanceToPlayer > furthestRadius)
            {
                ResetFurthestRadius();
                bossState = BossState.Moving;
                animator.SetBool("isMoving", true);
            }
            else
            {
                HandleAbilities();
            }
        }

        FlipGameObject();
    }

    private void CheckEnrageMode()
    {
        if (!isEnraged && stats.health <= stats.maxHealth * enrageThreshold)
        {
            isEnraged = true;

            Renderer renderer = GetComponent<Renderer>();
            renderer.material = outlineMaterial;
        }
    }

    private void MoveToIdealRadius(float distanceToPlayer)
    {
        Vector2 direction = Vector2.zero;

        if (distanceToPlayer < closestRadius)
        {
            direction = (transform.position - player.position).normalized;
        }
        else if (distanceToPlayer > furthestRadius)
        {
            direction = (player.position - transform.position).normalized;
        }

        rb.velocity = direction * moveSpeed;
    }

    private void HandleAbilities()
    {
        fireballTimer -= Time.deltaTime;
        shotgunTimer -= Time.deltaTime;
        reflectTimer -= Time.deltaTime;
        damageReductionTimer -= Time.deltaTime;
        healTimer -= Time.deltaTime;
        meteorShowerTimer -= Time.deltaTime;

        if (fireballTimer <= 0)
        {
            if (isEnraged)
            {
                StartCoroutine(FireballBurst());
            }
            else
            {
                LaunchFireball();
            }
            fireballTimer = fireballCooldown;
        }

        if (shotgunTimer <= 0)
        {
            LaunchShotgunBurst();
            shotgunTimer = shotgunCooldown;
        }

        if (reflectTimer <= 0)
        {
            ActivateReflectShield();
            reflectTimer = reflectCooldown;
        }

        if (damageReductionTimer <= 0)
        {
            ActivateDamageReductionShield();
            damageReductionTimer = damageReductionCooldown;
        }

        if (healTimer <= 0)
        {
            HealBoss();
            healTimer = healCooldown;
        }

        if (meteorShowerTimer <= 0)
        {
            StartCoroutine(LaunchMeteorShower());
            meteorShowerTimer = meteorShowerCooldown;
        }
    }

    private void HandleTeleportation()
    {
        teleportTimer -= Time.deltaTime;
        if (teleportTimer <= 0 && !isTeleporting)
        {
            StartCoroutine(Teleport());
        }
    }

    private void LaunchFireball()
    {
        animator.SetTrigger("Attack");
        GameObject fireball = Instantiate(fireballPrefab, transform.position, Quaternion.identity);
        fireball.GetComponent<EnemyFireball>().SetDirection((player.position - transform.position).normalized);
    }

    private IEnumerator FireballBurst()
    {
        for (int i = 0; i < fireballBurstCount; i++)
        {
            LaunchFireball();
            yield return new WaitForSeconds(fireballBurstDelay);
        }
    }

    private void LaunchShotgunBurst()
    {
        animator.SetTrigger("Attack");

        int bulletsToFire = isEnraged ? enragedShotgunBulletCount : shotgunBulletCount;
        float angleStep = isEnraged ? enragedShotgunSpreadAngle / (bulletsToFire - 1) : shotgunSpreadAngle / (bulletsToFire - 1);
        float startAngle = isEnraged ? -enragedShotgunSpreadAngle / 2 : -shotgunSpreadAngle / 2;

        for (int i = 0; i < bulletsToFire; i++)
        {
            float currentAngle = startAngle + (i * angleStep);
            Quaternion rotation = Quaternion.Euler(new Vector3(0, 0, currentAngle));
            Vector3 direction = rotation * (player.position - transform.position).normalized;

            GameObject projectile = Instantiate(shotgunProjectilePrefab, transform.position, Quaternion.identity);
            projectile.GetComponent<EnemyFireball>().SetDirection(direction);
        }
    }

    private IEnumerator LaunchMeteorShower()
    {
        float elapsed = 0f;
        float interval = meteorShowerDuration / meteorCount;

        while (elapsed < meteorShowerDuration)
        {
            SpawnMeteor();
            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }
    }

    private void SpawnMeteor()
    {
        animator.SetTrigger("Attack");
        Transform playerTransform = GameManager.Instance.GetPlayerTransform();
        Vector2 spawnPosition = (Vector2)playerTransform.position + Random.insideUnitCircle * meteorSpawnRadius;
        Instantiate(meteorPrefab, spawnPosition, Quaternion.identity);
    }

    private void ActivateReflectShield()
    {
        stats.AddReflect(reflectIntensity, reflectDuration);
    }

    private void ActivateDamageReductionShield()
    {
        stats.AddDamageReduction(damageReductionStrength, damageReductionDuration);
    }

    private void HealBoss()
    {
        stats.AddHealth(healingAmount);
        healingBurstParticles.Play();
    }

    private IEnumerator Teleport()
    {
        isTeleporting = true;
        boxCollider.enabled = false;
        yield return StartCoroutine(DissolveEffect(false));
        transform.position = (Vector2)transform.position + Random.insideUnitCircle * teleportRadius;
        yield return StartCoroutine(DissolveEffect(true));
        teleportTimer = Random.Range(teleportCooldownMin, teleportCooldownMax);
        boxCollider.enabled = true;
        isTeleporting = false;
    }

    private IEnumerator DissolveEffect(bool isAppearing)
    {
        Renderer renderer = GetComponent<Renderer>();

        if (isEnraged)
        {
            if (!isAppearing)
            {
                // Step 1: Outline _Thickness from 0.4 to 0
                float outlineThickness = 0.4f;
                while (outlineThickness > 0f)
                {
                    outlineThickness -= Time.deltaTime * materialSwapSpeed;
                    outlineMaterial.SetFloat("_Thickness", Mathf.Clamp(outlineThickness, 0f, 0.4f));
                    yield return null;
                }

                // Step 2: Change material to dissolve
                renderer.material = dissolveMaterial;
            }

            // Step 3 and 5: Dissolve _Fade from 1 to 0 or 0 to 1
            float dissolveValue = isAppearing ? 0f : 1f;
            float endValue = isAppearing ? 1f : 0f;
            float step = Time.deltaTime * dissolveSpeed * (isAppearing ? 1 : -1);

            while ((isAppearing && dissolveValue < endValue) || (!isAppearing && dissolveValue > endValue))
            {
                dissolveValue += step;
                dissolveMaterial.SetFloat("_Fade", dissolveValue);
                yield return null;
            }

            if (isAppearing)
            {
                // Step 6: Change material to outline
                renderer.material = outlineMaterial;

                // Step 7: Outline _Thickness from 0 to 0.4
                float outlineThickness = 0f;
                while (outlineThickness < 0.4f)
                {
                    outlineThickness += Time.deltaTime * materialSwapSpeed;
                    outlineMaterial.SetFloat("_Thickness", Mathf.Clamp(outlineThickness, 0f, 0.4f));
                    yield return null;
                }
            }
        }
        else
        {
            // Non-enraged dissolve effect
            float dissolveValue = isAppearing ? 0f : 1f;
            float endValue = isAppearing ? 1f : 0f;
            float step = Time.deltaTime * dissolveSpeed * (isAppearing ? 1 : -1);

            while ((isAppearing && dissolveValue < endValue) || (!isAppearing && dissolveValue > endValue))
            {
                dissolveValue += step;
                dissolveMaterial.SetFloat("_Fade", dissolveValue);
                yield return null;
            }
        }
    }

    private void FlipGameObject()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        bool facingLeft = direction.x < 0;
        transform.rotation = Quaternion.Euler(0, facingLeft ? 0 : 180, 0);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, closestRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, furthestRadius);
    }

    public void DestroyGameobject()
    {
        float spawnRadius = 5f;

        boss_V.SetActive(true);
        boss_V.transform.position = GetRandomPositionInBox(transform.position, spawnRadius, spawnRadius);

        boss_E.SetActive(true);
        boss_E.transform.position = GetRandomPositionInBox(transform.position, spawnRadius, spawnRadius);

        boss_L.SetActive(true);
        boss_L.transform.position = GetRandomPositionInBox(transform.position, spawnRadius, spawnRadius);

        boss_H.SetActive(true);
        boss_H.transform.position = GetRandomPositionInBox(transform.position, spawnRadius, spawnRadius);

        boss_O.SetActive(true);
        boss_O.transform.position = GetRandomPositionInBox(transform.position, spawnRadius, spawnRadius);

        Destroy(gameObject);
    }

    private Vector2 GetRandomPositionInBox(Vector2 center, float width, float height)
    {
        float randomX = Random.Range(center.x - width / 2, center.x + width / 2);
        float randomY = Random.Range(center.y - height / 2, center.y + height / 2);
        return new Vector2(randomX, randomY);
    }

    private void ExtendFurthestRadius()
    {
        if (!furthestRadiusExtended)
        {
            furthestRadius *= 1.75f;
            furthestRadiusExtended = true;
        }
    }

    private void ResetFurthestRadius()
    {
        if (furthestRadiusExtended)
        {
            furthestRadius = originalFurthestRadius;
            furthestRadiusExtended = false;
        }
    }
}