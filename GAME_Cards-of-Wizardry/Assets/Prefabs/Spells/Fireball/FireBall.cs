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

        if (!isFragment)
        {
            light2D = GetComponent<Light2D>();
            Vector3 targetPosition = transform.position;
            transform.position = GameManager.Instance.GetPlayerTransform().position;

            if (!customDirectionSet)
            {
                direction = (targetPosition - transform.position).normalized;
            }
        }

        Invoke(nameof(DestroyGameObject), maxLifetime);
    }

    private void Update()
    {
        if (!hasExploded)
        {
            transform.position += direction * speed * Time.deltaTime;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!hasExploded && !isFragment)
        {
            Explode();
            boxCollider.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Enemy") && isFragment)
        {
            EnemyStats enemy = collider.GetComponent<EnemyStats>();
            if (damagedEnemies.Contains(enemy)) return;  // Avoid double damage due to double colliders on some enemies
            damagedEnemies.Add(enemy);

            float playerDamageModifier = GameManager.Instance.GetPlayerController().GetDamageBoost();
            float damageDealt = Random.Range(minDamage, maxDamage);
            enemy.TakeDamage(Mathf.RoundToInt(playerDamageModifier * damageDealt));
            enemy.SetOnFire(burnDuration);
            Destroy(gameObject);
        }
    }

    private void Explode()
    {
        audioSource.Play();

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                EnemyStats enemy = hitCollider.GetComponent<EnemyStats>();
                if (damagedEnemies.Contains(enemy)) continue;  // Avoid double damage due to double colliders on some enemies
                damagedEnemies.Add(enemy);

                float playerDamageModifier = GameManager.Instance.GetPlayerController().GetDamageBoost();
                float damageDealt = Random.Range(minDamage, maxDamage);
                enemy.TakeDamage(Mathf.RoundToInt(damageDealt * playerDamageModifier));

                if (burnDuration > 0)
                {
                    enemy.SetOnFire(burnDuration);
                }
            }
        }

        if (spawnFragments)
        {
            SpawnFireFragments();
        }

        hasExploded = true;
        CancelInvoke();
        StartCoroutine(AnimateLightAndDestroy());
    }

    private void SpawnFireFragments()
    {
        int numberOfFireballs = 5;
        float angleStep = 360f / numberOfFireballs;
        float startAngle = 0f;

        for (int i = 0; i < numberOfFireballs; i++)
        {
            float fireballDirectionAngle = startAngle + (angleStep * i);
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
        float duration = 0.25f;
        float time = 0;

        while (time < duration)
        {
            float phase = time / duration;
            light2D.pointLightOuterRadius = Mathf.Lerp(0.2f, 1.0f, phase);
            time += Time.deltaTime;
            yield return null;
        }

        time = 0;
        while (time < duration)
        {
            float phase = time / duration;
            light2D.pointLightOuterRadius = Mathf.Lerp(1.0f, 0.0f, phase);
            time += Time.deltaTime;
            yield return null;
        }

        // Give time for audio to finish
        spriteRenderer.enabled = false;
        light2D.enabled = false;
        Destroy(gameObject, 2f);
    }


    private void DestroyGameObject()
    {
        Destroy(gameObject);
    }
}
