using System.Collections;
using UnityEngine;

public class Boss_V : BossStageTwo
{
    [Header("Fireball Attack")]
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private float fireballCooldown = 5f;
    [SerializeField] private int fireballBurstCount = 3;
    [SerializeField] private float fireballBurstDelay = 0.4f;
    private float fireballTimer;

    protected override void Start()
    {
        base.Start();
        fireballTimer = fireballCooldown;
    }

    protected override void HandleAbilities()
    {
        fireballTimer -= Time.deltaTime;

        if (fireballTimer <= 0)
        {
            StartCoroutine(FireballBurst());
            fireballTimer = fireballCooldown;
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
}
