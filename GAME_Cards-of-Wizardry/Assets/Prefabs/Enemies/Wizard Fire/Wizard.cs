using System.Collections;
using UnityEditor;
using UnityEngine;

public class Wizard : MonoBehaviour
{
    public enum WizardType { Fire, Healing, Shielding }
    public enum WizardState { Moving, UsingSkills }
    public WizardType wizardType;
    public WizardState wizardState;

    private Transform player;
    private Rigidbody2D rb;
    private EnemyStats stats;
    private Animator animator;
    private BoxCollider2D boxCollider;
    private Material dissolveMaterial;
    private float dissolveSpeed = 2f;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float closestRadius = 3f;
    public float furthestRadius = 8f;
    public float teleportRadius = 10f;
    public float teleportCooldownMin = 3f;
    public float teleportCooldownMax = 5f;
    private float teleportTimer;
    private bool isTeleporting = false;
    private bool teleportDisabled = false;

    [Header("Cooldowns")]
    public float basicAttackCooldown = 2f;
    public float specialAbilityCooldown;
    public float survivalSkillHealthThreshold = 0.25f;
    private float specialAbilityTimer;
    private bool survivalSkillUsed = false;

    [Header("Fire Wizard Attributes")]
    public GameObject fireballPrefab;
    public GameObject smallFireballPrefab;
    public float reflectDuration = 10f;
    public float reflectIntensity = 0.25f;
    private float fireballTimer;

    [Header("Healing Wizard Attributes")]
    public GameObject healingProjectilePrefab;
    public float healBurstRadius = 10f;
    public int healBurstAmount = 100;
    public ParticleSystem healingBurstParticles;
    public float healingSurvivalHealAmount = 0.5f;
    public float healingSurvivalRunDuration = 5f;
    private float healTimer;
    private Collider2D selfCollider;

    [Header("Shielding Wizard Attributes")]
    public GameObject iceballPrefab;
    public float shieldRadius = 10f;
    public float shieldDuration = 8f;
    public int shieldAmount = 200;
    public int survivalShieldAmount = 500;
    private float iceballTimer;

    private void Start()
    {
        player = GameManager.Instance.GetPlayerTransform();
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<EnemyStats>();
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        dissolveMaterial = GetComponent<Renderer>().material;
        selfCollider = GetComponent<Collider2D>();

        teleportTimer = Random.Range(teleportCooldownMin, teleportCooldownMax);
        wizardState = WizardState.Moving;
    }

    private void Update()
    {
        if (!stats.IsAlive() || stats.IsFrozen()) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (!survivalSkillUsed && stats.GetHealth() <= stats.GetMaxHealth() * survivalSkillHealthThreshold)
        {
            ActivateSurvivalSkill();
            survivalSkillUsed = true;
        }

        HandleTeleportation(distanceToPlayer);
        HandleWizardBehavior(distanceToPlayer);
        specialAbilityTimer -= Time.deltaTime;
    }

    private void HandleTeleportation(float distanceToPlayer)
    {
        if (!isTeleporting)
        {
            teleportTimer -= Time.deltaTime;

            if (teleportTimer <= 0 && !teleportDisabled && distanceToPlayer <= furthestRadius)
            {
                teleportTimer = Random.Range(teleportCooldownMin, teleportCooldownMax);
                StartCoroutine(Teleport());
            }
        }
    }

    private void HandleWizardBehavior(float distanceToPlayer)
    {
        switch (wizardType)
        {
            case WizardType.Fire:
                HandleBehavior(distanceToPlayer, ref fireballTimer, LaunchFireball, LaunchSmallFireballs);
                break;
            case WizardType.Healing:
                HandleBehavior(distanceToPlayer, ref healTimer, HealLowestHealthEnemy, HealAllNearbyEnemies);
                break;
            case WizardType.Shielding:
                HandleBehavior(distanceToPlayer, ref iceballTimer, LaunchIceball, ShieldAllNearbyEnemies);
                break;
        }
    }

    private void HandleBehavior(float distanceToPlayer, ref float timer, System.Action basicAction, System.Action specialAction)
    {
        if (wizardState == WizardState.Moving)
        {
            MoveToIdealRadius(distanceToPlayer);

            if (distanceToPlayer >= closestRadius && distanceToPlayer <= furthestRadius)
            {
                wizardState = WizardState.UsingSkills;
                animator.SetBool("isMoving", false);
            }
        }
        else if (wizardState == WizardState.UsingSkills)
        {
            if (distanceToPlayer < closestRadius || distanceToPlayer > furthestRadius)
            {
                wizardState = WizardState.Moving;
                animator.SetBool("isMoving", true);
            }
            else
            {
                if (timer <= 0)
                {
                    basicAction();
                    timer = basicAttackCooldown;
                }

                if (specialAbilityTimer <= 0)
                {
                    specialAction();
                    specialAbilityTimer = specialAbilityCooldown;
                }

                timer -= Time.deltaTime;
            }
        }
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

    private void LaunchFireball()
    {
        GameObject fireball = Instantiate(fireballPrefab, transform.position, Quaternion.identity);
        Vector2 direction = (player.position - transform.position).normalized;
        fireball.GetComponent<EnemyFireball>().SetDirection(direction);
    }

    private void LaunchSmallFireballs()
    {
        for (int i = 0; i < 8; i++)
        {
            float angle = i * (360f / 8);
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
            GameObject smallFireball = Instantiate(smallFireballPrefab, transform.position, Quaternion.identity);
            smallFireball.GetComponent<EnemyFireball>().SetDirection(direction);
        }
    }

    private void LaunchIceball()
    {
        GameObject iceball = Instantiate(iceballPrefab, transform.position, Quaternion.identity);
        Vector2 direction = (player.position - iceball.transform.position).normalized;
        iceball.GetComponent<EnemyIceball>().SetDirection(direction);
    }

    private void HealLowestHealthEnemy()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, healBurstRadius, LayerMask.GetMask("Enemy"));
        EnemyStats lowestHealthEnemyStats = null;
        Collider2D lowestHealthCollider = null;
        float lowestHealthPercentage = float.MaxValue;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider != selfCollider)
            {
                EnemyStats enemyStats = hitCollider.GetComponent<EnemyStats>();
                if (enemyStats != null)
                {
                    float healthPercentage = (float)enemyStats.GetHealth() / enemyStats.GetMaxHealth();
                    if (healthPercentage < lowestHealthPercentage)
                    {
                        lowestHealthPercentage = healthPercentage;
                        lowestHealthEnemyStats = enemyStats;
                        lowestHealthCollider = hitCollider;
                    }
                }
            }
        }

        if (lowestHealthEnemyStats != null)
        {
            Vector2 direction = (lowestHealthCollider.transform.position - transform.position).normalized;
            GameObject healingProjectile = Instantiate(healingProjectilePrefab, transform.position, Quaternion.identity);
            healingProjectile.GetComponent<EnemyHealingBolt>().SetDirection(direction);
        }
    }

    private void HealAllNearbyEnemies()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, healBurstRadius, LayerMask.GetMask("Enemy"));
        foreach (var hitCollider in hitColliders)
        {
            EnemyStats enemyStats = hitCollider.GetComponent<EnemyStats>();

            if (enemyStats != null)
            {
                enemyStats.AddHealth(Mathf.RoundToInt(healBurstAmount * GameManager.Instance.GetSurvivalModifier()));
            }
        }

        healingBurstParticles.Play();
    }

    private void ShieldAllNearbyEnemies()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, shieldRadius, LayerMask.GetMask("Enemy"));
        foreach (var hitCollider in hitColliders)
        {
            EnemyStats enemyStats = hitCollider.GetComponent<EnemyStats>();

            if (enemyStats != null)
            {
                enemyStats.AddShield(Mathf.RoundToInt(shieldAmount * GameManager.Instance.GetSurvivalModifier()), shieldDuration);
            }
        }
    }

    private void ActivateSurvivalSkill()
    {
        switch (wizardType)
        {
            case WizardType.Fire:
                stats.AddReflect(reflectIntensity, reflectDuration);
                break;
            case WizardType.Healing:
                stats.AddHealth(Mathf.FloorToInt(stats.GetHealth() * healingSurvivalHealAmount));
                StartCoroutine(RunAwayFromPlayer(healingSurvivalRunDuration));
                break;
            case WizardType.Shielding:
                stats.AddShield(Mathf.RoundToInt(survivalShieldAmount * GameManager.Instance.GetSurvivalModifier()), shieldDuration);
                break;
        }
    }

    private IEnumerator RunAwayFromPlayer(float duration)
    {
        teleportDisabled = true;
        Vector2 direction = (transform.position - player.position).normalized;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            if (stats.IsFrozen())
            {
                rb.velocity = Vector2.zero;
                teleportDisabled = false;
                yield break;
            }

            rb.velocity = direction * moveSpeed;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rb.velocity = Vector2.zero;
        teleportDisabled = false;
    }

    public void DestroyGameobject()
    {
        Destroy(gameObject);
    }

    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, closestRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, furthestRadius);
    }
    #endif
}






#if UNITY_EDITOR
[CustomEditor(typeof(Wizard))]
public class WizardEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Wizard script = (Wizard)target;

        script.wizardType = (Wizard.WizardType)EditorGUILayout.EnumPopup("Wizard Type", script.wizardType);

        // Always draw the common controls
        EditorGUILayout.LabelField("Movement", EditorStyles.boldLabel);
        script.moveSpeed = EditorGUILayout.FloatField("Move Speed", script.moveSpeed);
        script.closestRadius = EditorGUILayout.FloatField("Closest Radius", script.closestRadius);
        script.furthestRadius = EditorGUILayout.FloatField("Furthest Radius", script.furthestRadius);
        script.teleportRadius = EditorGUILayout.FloatField("Teleport Radius", script.teleportRadius);
        script.teleportCooldownMin = EditorGUILayout.FloatField("Teleport Cooldown", script.teleportCooldownMin);
        script.teleportCooldownMax = EditorGUILayout.FloatField("Teleport Cooldown", script.teleportCooldownMax);

        EditorGUILayout.LabelField("Cooldowns", EditorStyles.boldLabel);
        script.basicAttackCooldown = EditorGUILayout.FloatField("Basic Attack Cooldown", script.basicAttackCooldown);
        script.specialAbilityCooldown = EditorGUILayout.FloatField("Special Ability Cooldown", script.specialAbilityCooldown);
        script.survivalSkillHealthThreshold = EditorGUILayout.Slider("Survival Skill Health Threshold", script.survivalSkillHealthThreshold, 0f, 1f);

        // Draw specific controls based on the selected wizard type
        switch (script.wizardType)
        {
            case Wizard.WizardType.Fire:
                EditorGUILayout.LabelField("Fire Wizard Attributes", EditorStyles.boldLabel);
                script.fireballPrefab = (GameObject)EditorGUILayout.ObjectField("Fireball Prefab", script.fireballPrefab, typeof(GameObject), false);
                script.smallFireballPrefab = (GameObject)EditorGUILayout.ObjectField("Small Fireball Prefab", script.smallFireballPrefab, typeof(GameObject), false);
                script.reflectDuration = EditorGUILayout.FloatField("Reflect Duration", script.reflectDuration);
                script.reflectIntensity = EditorGUILayout.Slider("Reflect Intensity", script.reflectIntensity, 0f, 1f);
                break;

            case Wizard.WizardType.Healing:
                EditorGUILayout.LabelField("Healing Wizard Attributes", EditorStyles.boldLabel);
                script.healingProjectilePrefab = (GameObject)EditorGUILayout.ObjectField("Healing Projectile Prefab", script.healingProjectilePrefab, typeof(GameObject), false);
                script.healBurstRadius = EditorGUILayout.FloatField("Heal Radius", script.healBurstRadius);
                script.healBurstAmount = EditorGUILayout.IntField("Healing Aura heal amount", script.healBurstAmount);
                script.healingBurstParticles = (ParticleSystem)EditorGUILayout.ObjectField("Healing Burst Particles", script.healingBurstParticles, typeof(ParticleSystem), true);
                script.healingSurvivalHealAmount = EditorGUILayout.Slider("Healing Survival Heal Amount", script.healingSurvivalHealAmount, 0f, 1f);
                script.healingSurvivalRunDuration = EditorGUILayout.FloatField("Healing Survival Run Duration", script.healingSurvivalRunDuration);
                break;

            case Wizard.WizardType.Shielding:
                EditorGUILayout.LabelField("Shielding Wizard Attributes", EditorStyles.boldLabel);
                script.iceballPrefab = (GameObject)EditorGUILayout.ObjectField("Iceball Prefab", script.iceballPrefab, typeof(GameObject), false);
                script.shieldRadius = EditorGUILayout.FloatField("Shield Radius", script.shieldRadius);
                script.shieldDuration = EditorGUILayout.FloatField("Shield Duration", script.shieldDuration);
                script.shieldAmount = EditorGUILayout.IntField("AoE Shield Amount", script.shieldAmount);
                script.survivalShieldAmount = EditorGUILayout.IntField("survival Shield Amount", script.survivalShieldAmount);
                break;
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(script);
        }
    }
}
#endif
