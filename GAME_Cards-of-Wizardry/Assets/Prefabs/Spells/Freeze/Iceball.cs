using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Iceball : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float explosionRadius = 1f;
    [SerializeField] private int minDamage = 80;
    [SerializeField] private int maxDamage = 105;
    [SerializeField] private float freezeDuration = 0f;
    [SerializeField] private float maxLifetime = 10f;

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

        SetInitialDirection();
        Invoke(nameof(DestroyGameObject), maxLifetime);
    }

    private void Update()
    {
        if (!hasExploded)
        {
            MoveIceball();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!hasExploded)
        {
            Explode();
        }
    }

    private void SetInitialDirection()
    {
        Vector3 targetPosition = transform.position;
        transform.position = GameManager.Instance.GetPlayerTransform().position;
        direction = (targetPosition - transform.position).normalized;
    }

    private void MoveIceball()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void Explode()
    {
        audioSource.Play();
        DealDamageToEnemies();
        hasExploded = true;
        boxCollider.enabled = false;
        CancelInvoke(nameof(DestroyGameObject));
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
        if (freezeDuration > 0)
        {
            enemy.Freeze(freezeDuration);
        }
    }

    public void SetDirection(Vector3 newDirection)
    {
        direction = newDirection;
    }

    private IEnumerator AnimateLightAndDestroy()
    {
        yield return StartCoroutine(AnimateLight(0.2f, 1.0f, 0.25f));
        yield return StartCoroutine(AnimateLight(1.0f, 0.0f, 0.25f));

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
