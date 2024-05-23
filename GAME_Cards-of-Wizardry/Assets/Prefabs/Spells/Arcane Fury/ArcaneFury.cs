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

    private SpellCard card1;
    private SpellCard card2;
    private SpellCard card3;
    private SpellCard card4;
    private SpellCard card5;
    private SpellCard card6;
    private SpellCard card7;

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
        card1 = GameObject.Find("UI/Cards/Card 1").GetComponent<SpellCard>();
        card2 = GameObject.Find("UI/Cards/Card 2").GetComponent<SpellCard>();
        card3 = GameObject.Find("UI/Cards/Card 3").GetComponent<SpellCard>();
        card4 = GameObject.Find("UI/Cards/Card 4").GetComponent<SpellCard>();
        card5 = GameObject.Find("UI/Cards/Card 5").GetComponent<SpellCard>();
        card6 = GameObject.Find("UI/Cards/Wis 10 Card").GetComponent<SpellCard>();
        card7 = GameObject.Find("UI/Cards/Wis 20 Card").GetComponent<SpellCard>();

        SpellCard[] cards = new SpellCard[] { card1, card2, card3, card4, card5, card6, card7 };

        foreach (SpellCard card in cards)
        {
            if (card.cooldownTimeRemaining <= 0 && card.gameObject.activeSelf)
            {
                damageMultiplier += damageBoostPerActiveSpell;
                card.StartRechargeSpell();
            }
        }

        audioSource = GetComponent<AudioSource>();
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        light2D = GetComponent<Light2D>();
        Vector3 targetPosition = transform.position;
        transform.position = GameManager.Instance.GetPlayerTransform().position;

        if (!customDirectionSet)
        {
            direction = (targetPosition - transform.position).normalized;
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
                if (damagedEnemies.Contains(enemy)) continue;  // Avoid double damage due to double colliders on some enemies
                damagedEnemies.Add(enemy);
                enemy.TakeDamage(Mathf.RoundToInt(Random.Range(minDamage, maxDamage) * GameManager.Instance.GetPlayerController().GetDamageBoost() * damageMultiplier));
            }
        }

        particles.Stop();

        hasExploded = true;
        CancelInvoke(nameof(DestroyGameObject));
        StartCoroutine(AnimateLightAndDestroy());
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

        spriteRenderer.enabled = false;
        light2D.enabled = false;

        // Give time for audio to play
        Destroy(gameObject, 2f);
    }

    private void DestroyGameObject()
    {
        Destroy(gameObject);
    }
}
