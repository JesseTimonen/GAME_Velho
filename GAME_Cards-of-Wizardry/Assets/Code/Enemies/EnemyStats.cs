using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class EnemyStats : MonoBehaviour
{
    public int maxHealth = 100;
    public int health = 100;
    public int experienceGain = 50;
    [SerializeField] private bool fireImmunity = false;
    [SerializeField] private bool freezeImmunity = false;
    [SerializeField] private GameObject healthBar;
    [SerializeField] private Slider healthBarSlider;
    [SerializeField] private ParticleSystem flameParticles;
    [SerializeField] private ParticleSystem shieldParticles;

    [HideInInspector] public Animator animator;
    private SpriteRenderer spriteRenderer;
    private LevelUpManager levelUpManager;

    private Coroutine burnCoroutine;
    private Coroutine damageReductionCoroutine;
    private Coroutine reflectCoroutine;
    private Coroutine freezeCoroutine;
    private float burnEndTime;
    private bool isDead = false;
    private bool isFrozen = false;
    private float damageReductionStrength = 0f;
    private float reflectIntensity = 0f;


    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }


    private void Start()
    {
        levelUpManager = GameManager.Instance.GetLevelUpManager();

        maxHealth = Mathf.RoundToInt(maxHealth * GameManager.Instance.GetSurvivalModifier());
        health = maxHealth;
    }


    public void TakeDamage(int amount)
    {
        if (isDead || isFrozen) return;

        int reducedDamage = Mathf.CeilToInt(amount * (1 - damageReductionStrength));
        health -= reducedDamage;

        UpdateHealthBar();

        if (health <= 0 && !isDead)
        {
            Die();
        }
        else
        {
            spriteRenderer.color = Color.red;
            Invoke(nameof(ResetSpriteColor), 0.33f);
            ReflectDamage(reducedDamage);
        }
    }


    public void AddHealth(int amount)
    {
        health = Mathf.Min(health + amount, maxHealth);
        UpdateHealthBar();

        spriteRenderer.color = Color.green;
        Invoke(nameof(ResetSpriteColor), 0.33f);
    }


    private void UpdateHealthBar()
    {
        if (!healthBar.activeSelf)
        {
            healthBar.SetActive(true);
        }

        healthBarSlider.maxValue = Mathf.Ceil(maxHealth);
        healthBarSlider.value = Mathf.Ceil(health);
    }


    private void ResetSpriteColor()
    {
        if (isFrozen)
        {
            spriteRenderer.color = Color.blue;
        }
        else
        {
            spriteRenderer.color = Color.white;
        }
    }


    private void Die()
    {
        isDead = true;
        healthBar.SetActive(false);
        levelUpManager.AddExperience(experienceGain);
        animator.SetTrigger("Die");
    }


    public bool IsAlive()
    {
        return !isDead;
    }


    public bool IsFrozen()
    {
        return isFrozen;
    }


    public void SetOnFire(float duration)
    {
        if (fireImmunity) return;

        if (!flameParticles.isPlaying)
        {
            flameParticles.Play();
        }

        if (burnCoroutine != null)
        {
            burnEndTime = Mathf.Max(burnEndTime, Time.time + duration);
        }
        else
        {
            burnEndTime = Time.time + duration;
            burnCoroutine = StartCoroutine(ApplyBurn());
        }
    }


    private IEnumerator ApplyBurn()
    {
        while (Time.time < burnEndTime)
        {
            TakeDamage(10);
            yield return new WaitForSeconds(1f);
        }

        burnCoroutine = null;
        flameParticles.Stop();
    }


    public void AddDamageReduction(float reductionStrength, float duration)
    {
        if (!shieldParticles.isPlaying)
        {
            shieldParticles.Play();
        }

        if (damageReductionCoroutine != null)
        {
            if (reductionStrength < damageReductionStrength)
            {
                return;
            }

            StopCoroutine(damageReductionCoroutine);
        }

        damageReductionStrength = reductionStrength;
        damageReductionCoroutine = StartCoroutine(RemoveDamageReductionAfterTime(duration));
    }


    private IEnumerator RemoveDamageReductionAfterTime(float duration)
    {
        yield return new WaitForSeconds(duration);
        damageReductionStrength = 0f;
        damageReductionCoroutine = null;
        shieldParticles.Stop();
    }


    public void AddReflect(float intensity, float duration)
    {
        if (reflectCoroutine != null)
        {
            if (intensity < reflectIntensity)
            {
                return;
            }

            StopCoroutine(reflectCoroutine);
        }

        reflectIntensity = intensity;
        reflectCoroutine = StartCoroutine(RemoveReflectAfterTime(duration));
    }


    private IEnumerator RemoveReflectAfterTime(float duration)
    {
        yield return new WaitForSeconds(duration);
        reflectIntensity = 0f;
        reflectCoroutine = null;
    }


    private void ReflectDamage(int damage)
    {
        if (reflectIntensity > 0)
        {
            int reflectDamage = Mathf.CeilToInt(damage * reflectIntensity);
            GameManager.Instance.GetPlayerController().TakeDamage(reflectDamage);
        }
    }


    public void Freeze(float duration)
    {
        if (freezeImmunity) return;

        if (freezeCoroutine != null)
        {
            StopCoroutine(freezeCoroutine);
        }

        spriteRenderer.color = Color.blue;
        isFrozen = true;
        freezeCoroutine = StartCoroutine(UnfreezeAfterTime(duration));
    }


    private IEnumerator UnfreezeAfterTime(float duration)
    {
        yield return new WaitForSeconds(duration);
        freezeCoroutine = null;
        isFrozen = false;
        ResetSpriteColor();
    }
}
