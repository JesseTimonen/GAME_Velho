using UnityEngine;


public class Boss_E : BossStageTwo
{
    [Header("Shotgun Attack")]
    [SerializeField] private GameObject shotgunProjectilePrefab;
    [SerializeField] private float shotgunCooldown = 7f;
    [SerializeField] private float shotgunSpreadAngle = 30f;
    [SerializeField] private int shotgunBulletCount = 5;
    private float shotgunTimer;


    protected override void Start()
    {
        base.Start();
        shotgunTimer = shotgunCooldown;
    }


    protected override void HandleAbilities()
    {
        shotgunTimer -= Time.deltaTime;

        if (shotgunTimer <= 0)
        {
            LaunchShotgunBurst();
            shotgunTimer = shotgunCooldown;
        }
    }


    private void LaunchShotgunBurst()
    {
        float angleStep = shotgunSpreadAngle / (shotgunBulletCount - 1);
        float startAngle = -shotgunSpreadAngle / 2;

        for (int i = 0; i < shotgunBulletCount; i++)
        {
            float currentAngle = startAngle + (i * angleStep);
            Quaternion rotation = Quaternion.Euler(new Vector3(0, 0, currentAngle));
            Vector3 direction = rotation * (player.position - transform.position).normalized;

            GameObject projectile = Instantiate(shotgunProjectilePrefab, transform.position, Quaternion.identity);
            projectile.GetComponent<EnemyFireball>().SetDirection(direction);
        }
    }
}