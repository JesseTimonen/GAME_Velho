using UnityEngine;


public class Boss_O : BossStageTwo
{
    [Header("Iceball Attack")]
    [SerializeField] private GameObject iceballPrefab;
    [SerializeField] private float iceballCooldown = 2f;
    private float iceballTimer;

    [Header("Shield Skill")]
    [SerializeField] private float shieldRadius = 10f;
    [SerializeField] private float shieldCooldown = 15f;
    [SerializeField] private float shieldDuration = 8f;
    [SerializeField][Range(0f, 1f)] private float shieldDamageReduction = 0.5f;
    private float shieldTimer;


    protected override void Start()
    {
        base.Start();
        iceballTimer = iceballCooldown;
        shieldTimer = shieldCooldown;
    }


    protected override void HandleAbilities()
    {
        iceballTimer -= Time.deltaTime;
        shieldTimer -= Time.deltaTime;

        if (iceballTimer <= 0)
        {
            LaunchIceball();
            iceballTimer = iceballCooldown;
        }

        if (shieldTimer <= 0)
        {
            ShieldAllNearbyEnemies();
            shieldTimer = shieldCooldown;
        }
    }


    private void LaunchIceball()
    {
        GameObject iceball = Instantiate(iceballPrefab, transform.position, Quaternion.identity);
        Vector2 direction = (player.position - iceball.transform.position).normalized;
        iceball.GetComponent<EnemyIceball>().SetDirection(direction);
    }


    private void ShieldAllNearbyEnemies()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, shieldRadius, LayerMask.GetMask("Enemy"));
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.GetComponent<EnemyStats>() != null)
            {
                hitCollider.GetComponent<EnemyStats>().AddDamageReduction(shieldDamageReduction, shieldDuration);
            }
        }
    }
}
