using System.Linq;
using UnityEngine;


public class Boss_H : BossStageTwo
{
    [Header("Healing Bolt")]
    [SerializeField] private GameObject healingBoltPrefab;
    [SerializeField] private float healBoltCooldown = 2f;
    private float healBoltTimer;

    [Header("Healing Burst")]
    public ParticleSystem healingBurstParticles;
    [SerializeField] private float healBurstCooldown = 15f;
    [SerializeField] private int healBurstAmount = 100;
    [SerializeField] private float healBurstRadius = 10f;
    private float healBurstTimer;


    protected override void Start()
    {
        base.Start();
        healBurstTimer = healBurstCooldown;
        healBoltTimer = healBoltCooldown;
    }


    protected override void HandleAbilities()
    {
        healBurstTimer -= Time.deltaTime;
        healBoltTimer -= Time.deltaTime;

        if (healBoltTimer <= 0)
        {
            HealLowestHealthEnemy();
            healBoltTimer = healBoltCooldown;
        }

        if (healBurstTimer <= 0)
        {
            HealAllNearbyEnemies();
            healBurstTimer = healBurstCooldown;
        }
    }


    private void HealLowestHealthEnemy()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, healBurstRadius, LayerMask.GetMask("Enemy"));
        Collider2D selfCollider = GetComponent<Collider2D>();

        var enemies = hitColliders.Where(c => c.GetComponent<EnemyStats>() != null && c != selfCollider).ToList();

        if (enemies.Count > 0)
        {
            var lowestHealthEnemy = enemies.OrderBy(e => e.GetComponent<EnemyStats>().health / e.GetComponent<EnemyStats>().maxHealth).First();

            if (lowestHealthEnemy != null)
            {
                Vector2 direction = (lowestHealthEnemy.transform.position - transform.position).normalized;
                GameObject healingBolt = Instantiate(healingBoltPrefab, transform.position, Quaternion.identity);
                healingBolt.GetComponent<EnemyHealingBolt>().SetDirection(direction);
            }
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
}
