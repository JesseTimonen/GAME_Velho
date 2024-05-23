using System.Collections;
using UnityEngine;

public class EnemyFireball : MonoBehaviour
{
    [SerializeField] private int minDamage = 8;
    [SerializeField] private int maxDamage = 12;
    [SerializeField] private float fireDuration = 3f;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float lifetime = 10f;

    [Header("Fireball Fragments")]
    [SerializeField] private bool spawnSmallFireballs = false;
    [SerializeField] private GameObject smallFireballPrefab;
    [SerializeField] private float smallFireballInterval = 1f;
    [SerializeField] private int smallFireballCount = 6;
    [SerializeField] private bool destroyOnContact = true;

    private Vector2 direction;

    private void Start()
    {
        if (spawnSmallFireballs)
        {
            StartCoroutine(SpawnSmallFireballs());
        }
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection;

        // Set the rotation to face the movement direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController playerController = GameManager.Instance.GetPlayerController();
            playerController.TakeDamage(Mathf.RoundToInt(Random.Range(minDamage, maxDamage) * GameManager.Instance.GetSurvivalModifier()));

            if (fireDuration > 0)
            {
                playerController.SetOnFire(fireDuration);
            }

            if (destroyOnContact)
            {
                Destroy(gameObject);
            }
        }
    }

    private IEnumerator SpawnSmallFireballs()
    {
        while (true)
        {
            yield return new WaitForSeconds(smallFireballInterval);
            for (int i = 0; i < smallFireballCount; i++)
            {
                float angle = i * (360f / smallFireballCount);
                Vector2 spawnDirection = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
                GameObject smallFireball = Instantiate(smallFireballPrefab, transform.position, Quaternion.identity);
                smallFireball.GetComponent<EnemyFireball>().SetDirection(spawnDirection);
            }
        }
    }
}
