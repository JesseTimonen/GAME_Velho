using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ArcaneFury : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float explosionRadius = 1f;
    [SerializeField] private int minDamage = 80;
    [SerializeField] private int maxDamage = 105;
    [SerializeField] private float maxLifetime = 10f;
    [SerializeField] private float damageBoostPerActiveSpell = 0.5f;
    [SerializeField] private ParticleSystem particles;

    private bool customDirectionSet = false;
    private AudioSource audioSource;
    private Vector3 direction;
    private Light2D light2D;
    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;
    private bool hasExploded = false;
    private HashSet<EnemyStats> damagedEnemies = new HashSet<EnemyStats>();
    private float damageMultiplier = 1f;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        light2D = GetComponent<Light2D>();

        CalculateDamageMultiplier();
        SetInitialDirection();
        Invoke(nameof(DestroyGameObject), maxLifetime);
    }

    private void CalculateDamageMultiplier()
    {
        SpellCard[] cards = {
            GameObject.Find("UI/Cards/Card 1").GetComponent<SpellCard>(),
            GameObject.Find("UI/Cards/Card 2").GetComponent<SpellCard>(),
            GameObject.Find("UI/Cards/Card 3").GetComponent<SpellCard>(),
            GameObject.Find("UI/Cards/Card 4").GetComponent<SpellCard>(),
            GameObject.Find("UI/Cards/Card 5").GetComponent<SpellCard>(),
            GameObject.Find("UI/Cards/Wis 10 Card").GetComponent<SpellCard>(),
            GameObject.Find("UI/Cards/Wis 20 Card").GetComponent<SpellCard>()
        };

        foreach (SpellCard card in cards)
        {
            if (card.cooldownTimeRemaining <= 0 && card.gameObject.activeSelf)
            {
                damageMultiplier += damageBoostPerActiveSpell;
                card.StartRechargeSpell();
            }
        }
    }

    private void SetInitialDirection()
    {
        Vector3 targetPosition = transform.position;
        transform.position = GameManager.Instance.GetPlayerTransform().position;

        if (!customDirectionSet)
        {
            direction = (targetPosition - transform.position).normalized;
        }
    }

    private void Update()
    {
        if (!hasExploded)
        {
            MoveArcaneFury();
        }
    }

    private void MoveArcaneFury()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!hasExploded)
        {
            Explode();
        }
    }

    private void Explode()
    {
        hasExploded = true;
        audioSource.Play();
        DealDamageToEnemies();
        particles.Stop();
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
        int finalDamage = Mathf.RoundToInt(damageDealt * playerDamageModifier * damageMultiplier);

        enemy.TakeDamage(finalDamage);
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
