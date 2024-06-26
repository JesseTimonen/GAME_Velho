using UnityEngine;
using UnityEngine.UI;
using DamageNumbersPro;
using System.Collections;
using System.Collections.Generic;

public class EnemyStats : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int health = 100;
    [SerializeField] private int shieldHealth = 0;
    [SerializeField] private int experienceGain = 50;
    [SerializeField] private bool fireImmunity = false;
    [SerializeField] private bool freezeImmunity = false;

    [Header("Particles")]
    [SerializeField] private ParticleSystem burningParticles;
    [SerializeField] private ParticleSystem shieldParticles;
    [SerializeField] private ParticleSystem reflectParticles;

    [Header("UI")]
    [SerializeField] private Transform canvas;
    [SerializeField] private GameObject healthBar;
    [SerializeField] private Slider healthBarSlider;

    [Header("Floating Damage Numbers")]
    [SerializeField] private GameObject healFloatingPrefab;
    [SerializeField] private GameObject damageFloatingPrefab;
    [SerializeField] private GameObject fireFloatingPrefab;
    [SerializeField] private GameObject freezeFloatingPrefab;
    [SerializeField] private GameObject absorbedFloatingPrefab;
    [SerializeField] private GameObject reflectedFloatingPrefab;

    [HideInInspector] public Animator animator;
    private SpriteRenderer spriteRenderer;
    private LevelUpManager levelUpManager;
    private Dictionary<string, Coroutine> activeCoroutines = new Dictionary<string, Coroutine>();
    private bool isDead = false;
    private bool isFrozen = false;
    private float reflectIntensity = 0f;

    public event System.Action OnFreezeEvent;
    public event System.Action OnUnfreezeEvent;

    private void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        levelUpManager = GameManager.Instance.GetLevelUpManager();

        maxHealth = Mathf.RoundToInt(maxHealth * GameManager.Instance.GetSurvivalModifier());
        health = maxHealth;
    }

    public void TakeDamage(int amount, bool isFireDamage = false)
    {
        if (isDead) return;

        if (shieldHealth > 0)
        {
            shieldHealth -= amount;
            if (shieldHealth <= 0)
            {
                int overflowDamage = -shieldHealth;
                health -= overflowDamage;

                GameObject instantiatedfloatingText = InstantiateFloatingText(isFireDamage ? fireFloatingPrefab : damageFloatingPrefab);
                instantiatedfloatingText.GetComponent<DamageNumberMesh>().number = overflowDamage;

                ReflectDamage(overflowDamage);

                shieldHealth = 0;
                shieldParticles.Stop();
            }
            else
            {
                InstantiateFloatingText(absorbedFloatingPrefab);
                return;
            }
        }
        else
        {
            GameObject instantiatedfloatingText = InstantiateFloatingText(isFireDamage ? fireFloatingPrefab : damageFloatingPrefab);
            instantiatedfloatingText.GetComponent<DamageNumberMesh>().number = amount;
            int preDamageHealth = health;
            health -= amount;

            ReflectDamage(Mathf.Min(amount, preDamageHealth));
        }

        UpdateHealthBar();

        spriteRenderer.color = Color.red;
        Invoke(nameof(ResetSpriteColor), 0.33f);

        if (health <= 0 && !isDead)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        healthBar.SetActive(false);
        levelUpManager.AddExperience(experienceGain);
        animator.SetTrigger("Die");

        StopAllCustomCoroutines();
        StopAllParticles();
    }

    public bool IsAlive()
    {
        return !isDead;
    }

    private void ResetSpriteColor()
    {
        spriteRenderer.color = isFrozen ? Color.blue : Color.white;
    }

    public void AddHealth(int amount)
    {
        if (isDead) return;

        health = Mathf.Min(health + amount, maxHealth);
        UpdateHealthBar();

        GameObject instantiatedfloatingText = InstantiateFloatingText(healFloatingPrefab);
        instantiatedfloatingText.GetComponent<DamageNumberMesh>().leftText = "+" + amount;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.green;
            Invoke(nameof(ResetSpriteColor), 0.33f);
        }
    }

    public void SetHealth(int newHealth)
    {
        if (isDead) return;

        health = Mathf.Min(newHealth, maxHealth);
    }

    public int GetHealth()
    {
        return health;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public void SetMaxHealth(int newMaxHealth)
    {
        maxHealth = newMaxHealth;
    }

    private void UpdateHealthBar()
    {
        if (!healthBar.activeSelf)
        {
            healthBar.SetActive(true);
        }

        healthBarSlider.maxValue = maxHealth;
        healthBarSlider.value = health;
    }

    #region Shield
    public void AddShield(int shieldAmount, float duration)
    {
        if (isDead || shieldParticles == null) return;

        shieldHealth = Mathf.Max(shieldHealth, shieldAmount);
        shieldParticles.Play();
        StartOrRestartCoroutine("shield", RemoveShieldAfterTime(duration));
    }

    private IEnumerator RemoveShieldAfterTime(float duration)
    {
        yield return new WaitForSeconds(duration);
        shieldHealth = 0;
        shieldParticles.Stop();
    }
    #endregion

    #region Burn
    public void SetOnFire(float duration)
    {
        if (isDead || fireImmunity || burningParticles == null) return;

        burningParticles.Play();
        duration /= GameManager.Instance.GetSurvivalModifier();
        StartOrRestartCoroutine("burn", ApplyBurn(duration));
    }

    private IEnumerator ApplyBurn(float duration)
    {
        float burnEndTime = Time.time + duration;

        while (Time.time < burnEndTime)
        {
            if (isDead) yield break;

            TakeDamage(10, true);
            yield return new WaitForSeconds(1f);
        }

        burningParticles.Stop();
    }
    #endregion

    #region Reflect
    public void AddReflect(float intensity, float duration)
    {
        if (isDead || reflectParticles == null) return;

        reflectIntensity = Mathf.Max(reflectIntensity, intensity);
        reflectParticles.Play();
        StartOrRestartCoroutine("reflect", RemoveReflectAfterTime(duration));
    }

    private IEnumerator RemoveReflectAfterTime(float duration)
    {
        yield return new WaitForSeconds(duration);
        reflectParticles.Stop();
        reflectIntensity = 0f;
    }

    private void ReflectDamage(int damage)
    {
        if (reflectIntensity > 0)
        {
            InstantiateFloatingText(reflectedFloatingPrefab);
            GameManager.Instance.GetPlayerController().TakeDamage(Mathf.RoundToInt(damage * reflectIntensity));
        }
    }
    #endregion

    #region Freeze
    public void Freeze(float duration)
    {
        if (isDead) return;

        GameObject instantiatedfloatingText = InstantiateFloatingText(freezeFloatingPrefab);
        instantiatedfloatingText.GetComponent<DamageNumberMesh>().leftText = freezeImmunity ? "Immune" : "Freeze";

        if (freezeImmunity) return;

        OnFreezeEvent?.Invoke();
        isFrozen = true;
        spriteRenderer.color = Color.blue;

        StartOrRestartCoroutine("freeze", UnfreezeAfterTime(duration / GameManager.Instance.GetSurvivalModifier()));
    }

    private IEnumerator UnfreezeAfterTime(float duration)
    {
        yield return new WaitForSeconds(duration);
        OnUnfreezeEvent?.Invoke();
        isFrozen = false;
        ResetSpriteColor();
    }

    public bool IsFrozen()
    {
        return isFrozen;
    }
    #endregion

    private GameObject InstantiateFloatingText(GameObject prefab)
    {
        GameObject instantiatedfloatingText = Instantiate(prefab, canvas.transform.position, Quaternion.identity);
        instantiatedfloatingText.transform.SetParent(canvas);
        return instantiatedfloatingText;
    }

    private void StartOrRestartCoroutine(string key, IEnumerator coroutine)
    {
        if (activeCoroutines.ContainsKey(key))
        {
            StopCoroutine(activeCoroutines[key]);
        }

        activeCoroutines[key] = StartCoroutine(coroutine);
    }

    private void StopAllCustomCoroutines()
    {
        foreach (var coroutine in activeCoroutines.Values)
        {
            StopCoroutine(coroutine);
        }

        activeCoroutines.Clear();
    }

    private void StopAllParticles()
    {
        if (burningParticles != null && burningParticles.isPlaying)
        {
            burningParticles.Stop();
        }

        if (shieldParticles != null && shieldParticles.isPlaying)
        {
            shieldParticles.Stop();
        }

        if (reflectParticles != null && reflectParticles.isPlaying)
        {
            reflectParticles.Stop();
        }
    }

    // Called from animation event
    public void DestroyGameObject()
    {
        Destroy(gameObject);
    }
}
