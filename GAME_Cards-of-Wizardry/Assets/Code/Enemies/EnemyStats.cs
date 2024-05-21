using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnemyStats : MonoBehaviour
{
    public int maxHealth = 100;
    public int health = 100;
    public int shieldHealth = 0;
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
    private Coroutine reflectCoroutine;
    private Coroutine freezeCoroutine;
    private Coroutine shieldCoroutine;
    private float burnEndTime;
    private bool isDead = false;
    private bool isFrozen = false;
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
        if (isDead) return;

        if (shieldHealth > 0)
        {
            shieldHealth -= amount;
            if (shieldHealth <= 0)
            {
                // Some damage went through the shield
                health -= shieldHealth;
                ReflectDamage(Mathf.RoundToInt(shieldHealth * -1f));

                shieldParticles.Stop();
                shieldHealth = 0;
            }
        }
        else
        {
            health -= amount;
            ReflectDamage(amount);
        }

        UpdateHealthBar();

        spriteRenderer.color = Color.red;
        Invoke(nameof(ResetSpriteColor), 0.33f);

        if (health <= 0 && !isDead)
        {
            Die();
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

    public void AddShield(int shieldAmount, float duration)
    {
        shieldHealth += shieldAmount;
        if (!shieldParticles.isPlaying)
        {
            shieldParticles.Play();
        }

        if (shieldCoroutine != null)
        {
            StopCoroutine(shieldCoroutine);
        }

        shieldCoroutine = StartCoroutine(RemoveShieldAfterTime(duration));
    }

    private IEnumerator RemoveShieldAfterTime(float duration)
    {
        yield return new WaitForSeconds(duration);
        shieldHealth = 0;
        shieldParticles.Stop();
        shieldCoroutine = null;
    }

    public void SetOnFire(float duration)
    {
        if (fireImmunity) return;

        if (!flameParticles.isPlaying)
        {
            flameParticles.Play();
        }

        duration = duration / GameManager.Instance.GetSurvivalModifier();

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
        freezeCoroutine = StartCoroutine(UnfreezeAfterTime(duration / GameManager.Instance.GetSurvivalModifier()));
    }

    private IEnumerator UnfreezeAfterTime(float duration)
    {
        yield return new WaitForSeconds(duration);
        freezeCoroutine = null;
        isFrozen = false;
        ResetSpriteColor();
    }
}
