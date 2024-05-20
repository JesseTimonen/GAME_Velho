using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;


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


    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        light2D = GetComponent<Light2D>();
        Vector3 targetPosition = transform.position;
        transform.position = GameManager.Instance.GetPlayerTransform().position;
        direction = (targetPosition - transform.position).normalized;

        Invoke("DestroyGameObject", maxLifetime);
    }


    void Update()
    {
        if (!hasExploded)
        {
            transform.position += direction * speed * Time.deltaTime;
        }
    }



    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!hasExploded)
        {
            Explode();
            boxCollider.enabled = false;
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
                if (damagedEnemies.Contains(enemy)) continue;
                damagedEnemies.Add(enemy);

                float playerDamageModifier = GameManager.Instance.GetPlayerController().GetDamageBoost();
                float damageDealt = Random.Range(minDamage, maxDamage);
                enemy.TakeDamage(Mathf.RoundToInt(damageDealt * playerDamageModifier));

                if (freezeDuration > 0)
                {
                    enemy.Freeze(freezeDuration);
                }
            }
        }


        hasExploded = true;
        CancelInvoke();
        StartCoroutine(AnimateLightAndDestroy());
    }


    public void SetDirection(Vector3 newDirection)
    {
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
