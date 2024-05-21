using UnityEngine;

public class MeteoriteStorm : MonoBehaviour
{
    [SerializeField] private GameObject meteorPrefab;
    [SerializeField] private float meteorSpawnRadius = 5f;
    [SerializeField] private int MeteoritesToSpawn = 5;
    private int meteoritesSpawned = 0;

    private void Start()
    {
        InvokeRepeating("SpawnMeteor", 0f, 1f);
    }

    private void SpawnMeteor()
    {
        meteoritesSpawned++;
        Transform playerTransform = GameManager.Instance.GetPlayerTransform();
        Vector2 spawnPosition = (Vector2)playerTransform.position + Random.insideUnitCircle * meteorSpawnRadius;
        Instantiate(meteorPrefab, spawnPosition, Quaternion.identity);

        if (meteoritesSpawned >= MeteoritesToSpawn)
        {
            CancelInvoke("SpawnMeteor");
            Destroy(gameObject);
        }
    }
}
