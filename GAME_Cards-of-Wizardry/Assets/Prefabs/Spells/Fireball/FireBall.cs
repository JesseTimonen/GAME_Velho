using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FireBall : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float explosionRadius = 1f;
    [SerializeField] private int minDamage = 80;
    [SerializeField] private int maxDamage = 105;
    [SerializeField] private float burnDuration = 0f;
    [SerializeField] private float maxLifetime = 10f;

    [Header("Fragments")]
    [SerializeField] private bool spawnFragments = false;
    [SerializeField] private GameObject fireBallFragment;
    [SerializeField] private bool isFragment = false;

    private bool customDirectionSet = false;
    private AudioSource audioSource;
    private Vector3 direction;
    private Light2D light2D;
    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;
    private bool hasExploded = false;
    private HashSet<EnemyStats> damagedEnemies = new HashSet<EnemyStats>();

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        light2D = GetComponent<Light2D>();

        if (!isFragment)
        {
            InitializeFireball();
        }
        Invoke(nameof(DestroyGameObject), maxLifetime);
    }

    private void Update()
    {
        if (!hasExploded)
        {
            MoveFireball();
        }
    }

    private void InitializeFireball()
    {
        Vector3 targetPosition = transform.position;
        transform.position = GameManager.Instance.GetPlayerTransform().position;
        if (!customDirectionSet)
        {
            direction = (targetPosition - transform.position).normalized;
        }
    }

    private void MoveFireball()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!hasExploded && !isFragment)
        {
            Explode();
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (isFragment && collider.CompareTag("Enemy"))
        {
            HandleFragmentCollision(collider);
        }
    }

    private void HandleFragmentCollision(Collider2D collider)
    {
        EnemyStats enemy = collider.GetComponent<EnemyStats>();
        if (damagedEnemies.Contains(enemy)) return;

        damagedEnemies.Add(enemy);
        ApplyDamageToEnemy(enemy);
        Destroy(gameObject);
    }

    private void Explode()
    {
        hasExploded = true;
        audioSource.Play();
        DealDamageToEnemies();
        if (spawnFragments)
        {
            SpawnFireFragments();
        }
        boxCollider.enabled = false;
        StartCoroutine(AnimateLightAndDestroy());
    }

    private void DealDamageToEnemies()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                EnemyStats enemy = hitCollider.GetComponent<EnemyStats>();
                if (damagedEnemies.Contains(enemy)) continue;

                damagedEnemies.Add(enemy);
                ApplyDamageToEnemy(enemy);
            }
        }
    }

    private void ApplyDamageToEnemy(EnemyStats enemy)
    {
        float playerDamageModifier = GameManager.Instance.GetPlayerController().GetDamageBoost();
        float damageDealt = Random.Range(minDamage, maxDamage);
        int finalDamage = Mathf.RoundToInt(damageDealt * playerDamageModifier);

        enemy.TakeDamage(finalDamage);
        if (burnDuration > 0)
        {
            enemy.SetOnFire(burnDuration);
        }
    }

    private void SpawnFireFragments()
    {
        int numberOfFireballs = 5;
        float angleStep = 360f / numberOfFireballs;

        for (int i = 0; i < numberOfFireballs; i++)
        {
            float fireballDirectionAngle = angleStep * i;
            Vector3 fireballDirection = Quaternion.Euler(0, 0, fireballDirectionAngle) * Vector3.up;
            GameObject smallFireball = Instantiate(fireBallFragment, transform.position, Quaternion.identity);
            smallFireball.GetComponent<FireBall>().SetDirection(fireballDirection);
        }
    }

    public void SetDirection(Vector3 newDirection)
    {
        customDirectionSet = true;
        direction = newDirection;
    }

    private IEnumerator AnimateLightAndDestroy()
    {
        yield return AnimateLight(0.2f, 1.0f, 0.25f);
        yield return AnimateLight(1.0f, 0.0f, 0.25f);

        spriteRenderer.enabled = false;
        light2D.enabled = false;

        // Give time for audio to play
        Destroy(gameObject, 2f);
    }

    private IEnumerator AnimateLight(float startRadius, float endRadius, float duration)
    {
        float time = 0;
        while (time < duration)
        {
            float phase = time / duration;
            light2D.pointLightOuterRadius = Mathf.Lerp(startRadius, endRadius, phase);
            time += Time.deltaTime;
            yield return null;
        }
    }

    private void DestroyGameObject()
    {
        Destroy(gameObject);
    }
}
