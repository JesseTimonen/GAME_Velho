using System.Collections;
using UnityEngine;


public class Boss_L : BossStageTwo
{
    [Header("Meteor Shower Attack")]
    [SerializeField] private GameObject meteorPrefab;
    [SerializeField] private float meteorShowerCooldown = 15f;
    [SerializeField] private int meteorCount = 10;
    [SerializeField] private float meteorShowerDuration = 5f;
    [SerializeField] private float meteorSpawnRadius = 10f;
    private float meteorShowerTimer;


    protected override void Start()
    {
        base.Start();
        meteorShowerTimer = meteorShowerCooldown;
    }


    protected override void HandleAbilities()
    {
        meteorShowerTimer -= Time.deltaTime;

        if (meteorShowerTimer <= 0)
        {
            StartCoroutine(LaunchMeteorShower());
            meteorShowerTimer = meteorShowerCooldown;
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
        Vector2 spawnPosition = (Vector2)player.position + Random.insideUnitCircle * meteorSpawnRadius;
        Instantiate(meteorPrefab, spawnPosition, Quaternion.identity);
    }
}
