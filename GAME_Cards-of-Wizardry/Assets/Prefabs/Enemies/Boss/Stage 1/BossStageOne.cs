using System.Collections;
using UnityEngine;

public class BossStageOne : MonoBehaviour
{
    private EnemyStats stats;
    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private BoxCollider2D boxCollider;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float closestRadius = 3f;
    [SerializeField] private float furthestRadius = 8f;
    [SerializeField] private float teleportRadius = 8f;
    [SerializeField] private float teleportCooldownMin = 3;
    [SerializeField] private float teleportCooldownMax = 5;
    private float teleportTimer;
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

    [Header("Shield")]
    [SerializeField] private float shieldCooldown = 15f;
    [SerializeField] private float shieldDuration = 8f;
    [SerializeField] private int shieldAmount = 500;
    private float shieldTimer;

    [Header("Enrage")]
    [Range(0f, 1f)] public float enrageThreshold = 0.4f;
    private bool isEnraged = false;

    [Header("Materials")]
    [SerializeField] protected Material dissolveMaterial;
    [SerializeField] protected Material outlineMaterial;
    [SerializeField] protected float dissolveSpeed = 3f;
    [SerializeField] protected float materialSwapSpeed = 5f;
    private Renderer materialRenderer;

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
        materialRenderer = GetComponent<Renderer>();
    }

    private void Start()
    {
        player = GameManager.Instance.GetPlayerTransform();

        ResetTimers();

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

    private void ResetTimers()
    {
        fireballTimer = fireballCooldown;
        shotgunTimer = shotgunCooldown;
        reflectTimer = reflectCooldown;
        shieldTimer = shieldCooldown;
        meteorShowerTimer = meteorShowerCooldown;
        healTimer = healCooldown;
        teleportTimer = Random.Range(teleportCooldownMin, teleportCooldownMax);
    }

    private void CheckEnrageMode()
    {
        if (!isEnraged && !isTeleporting && stats.GetHealth() <= stats.GetMaxHealth() * enrageThreshold)
        {
            isEnraged = true;
            materialRenderer.material = outlineMaterial;
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
        shieldTimer -= Time.deltaTime;
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

        if (shieldTimer <= 0)
        {
            ActivateShield();
            shieldTimer = shieldCooldown;
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
            teleportTimer = Random.Range(teleportCooldownMin, teleportCooldownMax);
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

    private void ActivateShield()
    {
        stats.AddShield(Mathf.RoundToInt(shieldAmount * GameManager.Instance.GetSurvivalModifier()), shieldDuration);
    }

    private void HealBoss()
    {
        stats.AddHealth(Mathf.RoundToInt(healingAmount * GameManager.Instance.GetSurvivalModifier()));
        healingBurstParticles.Play();
    }

    private IEnumerator Teleport()
    {
        if (!isTeleporting)
        {
            isTeleporting = true;
            boxCollider.enabled = false;
            yield return StartCoroutine(DissolveEffect(false));
            transform.position = (Vector2)transform.position + Random.insideUnitCircle * teleportRadius;
            yield return StartCoroutine(DissolveEffect(true));
            boxCollider.enabled = true;
            isTeleporting = false;
        }
    }

    private IEnumerator DissolveEffect(bool isAppearing)
    {
        if (isEnraged)
        {
            if (!isAppearing)
            {
                // Step 1: Outline _Thickness from 0.4 to 0
                float outlineThickness = 0.4f;
                while (outlineThickness > 0f)
                {
                    outlineThickness -= Time.unscaledDeltaTime * materialSwapSpeed;
                    outlineMaterial.SetFloat("_Thickness", Mathf.Clamp(outlineThickness, 0f, 0.4f));
                    yield return null;
                }

                // Step 2: Change material to dissolve
                materialRenderer.material = dissolveMaterial;

                // Step 3: Dissolve _Fade goes from 1 to 0
                float dissolveValue = 1f;
                while (dissolveValue > 0f)
                {
                    dissolveValue -= Time.unscaledDeltaTime * dissolveSpeed;
                    dissolveMaterial.SetFloat("_Fade", dissolveValue);
                    yield return null;
                }
            }
            else
            {
                // Step 5: Dissolve _Fade goes from 0 to 1
                float dissolveValue = 0f;
                while (dissolveValue < 1f)
                {
                    dissolveValue += Time.unscaledDeltaTime * dissolveSpeed;
                    dissolveMaterial.SetFloat("_Fade", dissolveValue);
                    yield return null;
                }

                // Step 6: Change material to outline
                materialRenderer.material = outlineMaterial;

                // Step 7: Outline _Thickness from 0 to 0.4
                float outlineThickness = 0f;
                while (outlineThickness < 0.4f)
                {
                    outlineThickness += Time.unscaledDeltaTime * materialSwapSpeed;
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
            float step = Time.unscaledDeltaTime * dissolveSpeed * (isAppearing ? 1 : -1);

            while ((isAppearing && dissolveValue < endValue) || (!isAppearing && dissolveValue > endValue))
            {
                dissolveValue += step;
                dissolveMaterial.SetFloat("_Fade", dissolveValue);
                yield return null;
            }

            dissolveMaterial.SetFloat("_Fade", endValue);
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

    // Called from animation event
    public void InitializePhaseTwo()
    {
        ActivateMiniBoss(boss_V);
        ActivateMiniBoss(boss_E);
        ActivateMiniBoss(boss_L);
        ActivateMiniBoss(boss_H);
        ActivateMiniBoss(boss_O);
        Destroy(gameObject);
    }

    private void ActivateMiniBoss(GameObject boss)
    {
        boss.SetActive(true);
        boss.transform.position = GetRandomPositionInBox(transform.position, 5f, 5f);
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
