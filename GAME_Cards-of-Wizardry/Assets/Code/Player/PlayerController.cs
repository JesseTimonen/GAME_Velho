using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] private InputController inputController;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator playerAnimator;


    [Header("BUFFS AND DEBUFFS")]
    [SerializeField] private GameObject tempHealthIcon;
    [SerializeField] private TextMeshProUGUI tempHealthIconDuration;
    [SerializeField] private TextMeshProUGUI tempHealthIconAmount;
    [SerializeField] private GameObject tempManaIcon;
    [SerializeField] private TextMeshProUGUI tempManaIconDuration;
    [SerializeField] private TextMeshProUGUI tempManaIconAmount;
    [SerializeField] private GameObject fireIcon;
    [SerializeField] private TextMeshProUGUI fireIconTimer;
    [SerializeField] private GameObject healIcon;
    [SerializeField] private TextMeshProUGUI healIconTimer;
    [SerializeField] private GameObject freezeIcon;
    [SerializeField] private TextMeshProUGUI freezeIconTimer;
    private Rigidbody2D rb;
    private Camera mainCamera;

    [Header("SHIELDS")]
    [SerializeField] private ParticleSystem shieldParticles;
    [SerializeField] private ParticleSystem shieldInnerParticles;
    [SerializeField] private GameObject ShieldUIElement;
    [SerializeField] private TextMeshProUGUI ShieldValueText;
    [SerializeField] private GameObject ShieldDamageUIElement;
    [SerializeField] private TextMeshProUGUI ShieldDamageValueText;
    [SerializeField] private ShieldTimer shieldTimer;
    [SerializeField] private ShieldDamageTimer shieldDamageTimer;

    [Header("Flames")]
    [SerializeField] private ParticleSystem flameParticles;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI manaText;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider manaSlider;

    [Header("MOVEMENT")]
    private float moveSpeed = 3f;
    private bool isKnockedBack = false;
    private Vector2 knockbackVelocity;

    [Header("STATS")]
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float tempMaxHealth = 0;
    [SerializeField] private float currentHealth = 100;
    [SerializeField] private float maxMana = 100;
    [SerializeField] private float tempMaxMana = 0;
    [SerializeField] private float currentMana = 100;
    [SerializeField] private float manaRechargeRate = 2;
    [SerializeField] private int shieldAmount = 0;
    [SerializeField] private bool shieldDamageBuffEnabled = false;
    [SerializeField] private bool shieldHealEnabled = false;
    [SerializeField] private int strength = 1;
    [SerializeField] private int intelligence = 1;
    [SerializeField] private int wisdom = 1;

    private Coroutine burnCoroutine;
    private float burnEndTime;

    private Coroutine healCoroutine;
    private float healEndTime;

    private Coroutine tempMaxHealthCoroutine;
    private float tempMaxHealthEndTime;

    private Coroutine tempMaxManaCoroutine;
    private float tempMaxManaEndTime;

    private Coroutine freezeCoroutine;
    private float freezeEndTime;
    private bool isFrozen = false;


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
    }


    private void Update()
    {
        UpdateStatsUI();

        if (isFrozen) return;

        if (currentMana < maxMana)
        {
            RechargeMana();
        }

        RotatePlayer();
    }


    private void FixedUpdate()
    {
        if (!isFrozen)
        {
            if (!isKnockedBack)
            {
                Move();
            }
        }
        else
        {
            rb.velocity = Vector2.zero;
            playerAnimator.SetBool("IsMoving", false);
        }
    }


    private void UpdateStatsUI()
    {
        healthText.text = Mathf.Ceil(currentHealth).ToString();
        healthSlider.value = Mathf.Ceil(currentHealth);
        healthSlider.maxValue = Mathf.Ceil(maxHealth);
        manaText.text = Mathf.Floor(currentMana).ToString();
        manaSlider.value = Mathf.Floor(currentMana);
        manaSlider.maxValue = Mathf.Floor(maxMana);
    }


    private void RechargeMana()
    {
        // Wisdom increases mana recharge by 10% per rank, adjustements might be needed when game balance becomes more clear
        currentMana = Mathf.Min(maxMana, currentMana + manaRechargeRate * (1 + 0.1f * (wisdom - 1)) * Time.deltaTime);
    }


    private void Move()
    {
        // Strength increases movement speed by 2% per rank, adjustements might be needed when game balance becomes more clear
        if (inputController.Move.x != 0 || inputController.Move.y != 0)
        {
            playerAnimator.SetBool("IsMoving", true);
        }
        else
        {
            playerAnimator.SetBool("IsMoving", false);
        }

        rb.velocity = new Vector2(inputController.Move.x, inputController.Move.y) * moveSpeed * (1 + 0.02f * (strength - 1));
    }


    public void Knockback(Vector2 direction, float force, float duration = 0.5f)
    {
        isKnockedBack = true;
        knockbackVelocity = direction * force;
        rb.velocity = knockbackVelocity;
        StartCoroutine(EndKnockback(duration));
    }


    private IEnumerator EndKnockback(float duration)
    {
        yield return new WaitForSeconds(duration);
        isKnockedBack = false;
    }


    private void RotatePlayer()
    {
        float distance = mainCamera.ScreenToWorldPoint(Input.mousePosition).x - transform.position.x;

        if (Mathf.Abs(distance) > 0.5f)
        {
            if (distance > 0)
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else
            {
                transform.rotation = Quaternion.Euler(0, 180, 0);
            }
        }
    }


    public void TakeDamage(int amount)
    {
        if (shieldAmount > 0)
        {
            HandleShield(ref amount);
            if (amount <= 0) return;
        }

        spriteRenderer.color = Color.red;
        Invoke("ResetSpriteColor", 0.33f);
        ApplyDamage(amount);
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


    public void SetOnFire(float duration)
    {
        fireIcon.SetActive(true);

        if (!flameParticles.isPlaying)
        {
            flameParticles.Play();
        }

        if (burnCoroutine != null)
        {
            if (Time.time + duration > burnEndTime)
            {
                burnEndTime = Time.time + duration;
            }
        }
        else
        {
            burnEndTime = Time.time + duration;
            burnCoroutine = StartCoroutine(ApplyBurn());
        }

        fireIconTimer.text = $"{Mathf.CeilToInt(burnEndTime - Time.time)}s";
    }


    private IEnumerator ApplyBurn()
    {
        while (Time.time < burnEndTime)
        {
            TakeDamage(8);
            fireIconTimer.text = $"{Mathf.CeilToInt(burnEndTime - Time.time)}s";
            yield return new WaitForSeconds(1f);
        }

        burnCoroutine = null;
        fireIcon.SetActive(false);
        flameParticles.Stop();
    }


    private void HandleShield(ref int amount)
    {
        int damageAfterShield = amount - shieldAmount;
        shieldAmount -= amount;

        if (shieldAmount <= 0)
        {
            RemoveShield();
        }

        UpdateShieldUI();

        amount = Mathf.Max(damageAfterShield, 0);
    }


    private void ApplyDamage(int amount)
    {
        // Strength decresed damage gradually down to -50%, adjustements might be needed when game balance becomes more clear
        float damageReductionFactor = strength / (strength + 24.44f);
        int damageTaken = Mathf.RoundToInt(amount * (1 - damageReductionFactor));

        currentHealth -= damageTaken;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }


    public void Die()
    {
        Debug.Log("You died, game over!");
        // TODO: Show Message and give option to reload scene
    }


    public void AddShield(int amount)
    {
        shieldAmount += amount;
        UpdateShieldUI();

        shieldTimer.InitializeTimer();
        shieldDamageTimer.InitializeTimer();

        if (!shieldParticles.isPlaying)
        {
            shieldParticles.Play();
            shieldInnerParticles.Play();
        }
    }


    public void RemoveShield()
    {
        if (shieldHealEnabled && shieldAmount > 0)
        {
            AddHealth(shieldAmount);
        }

        shieldAmount = 0;
        shieldParticles.Stop();
        shieldInnerParticles.Stop();
        ShieldUIElement.SetActive(false);
        ShieldDamageUIElement.SetActive(false);
    }


    private void UpdateShieldUI()
    {
        ShieldUIElement.SetActive(shieldAmount > 0);
        ShieldValueText.text = shieldAmount.ToString();

        if (shieldDamageBuffEnabled)
        {
            ShieldDamageUIElement.SetActive(shieldAmount > 0);
            float damage = shieldAmount * 0.1f;
            ShieldDamageValueText.text = Mathf.Ceil(damage).ToString() + "%";
        }
    }


    public void EnableShieldDamageBuff()
    {
        shieldDamageBuffEnabled = true;
    }


    public void EnableShieldHeal()
    {
        shieldHealEnabled = true;
    }


    public float GetCurrentHealth()
    {
        return currentHealth;
    }


    public void SetCurrentHealth(float value)
    {
        currentHealth = value;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }


    public void SetHealthFull()
    {
        currentHealth = maxHealth;
    }


    public void AddHealth(float amount)
    {
        currentHealth += amount;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        spriteRenderer.color = Color.green;
        Invoke("ResetSpriteColor", 0.33f);
    }


    public void AddHealthOverTime(float duration)
    {
        healIcon.SetActive(true);

        if (healCoroutine != null)
        {
            healEndTime += duration;
            healIconTimer.text = $"{Mathf.CeilToInt(healEndTime - Time.time)}s";
        }
        else
        {
            // Start a new healing coroutine
            healEndTime = Time.time + duration;
            healCoroutine = StartCoroutine(ApplyHealOverTime());
            healIconTimer.text = $"{Mathf.CeilToInt(healEndTime - Time.time)}s";
        }
    }


    private IEnumerator ApplyHealOverTime()
    {
        while (Time.time < healEndTime)
        {
            AddHealth(maxHealth * 0.05f);
            healIconTimer.text = $"{Mathf.CeilToInt(healEndTime - Time.time)}s";
            yield return new WaitForSeconds(1f);
        }

        healCoroutine = null;
        healIcon.SetActive(false);
    }


    public float GetMaxHealth()
    {
        return maxHealth;
    }


    public void SetMaxHealth(float value)
    {
        maxHealth = value;
    }


    public void AddMaxHealth(float amount)
    {
        maxHealth += amount;
    }


    public void AddTempMaxHealth(int additionalMaxHealth, float duration)
    {
        tempHealthIcon.SetActive(true);

        tempMaxHealth += additionalMaxHealth;
        maxHealth += additionalMaxHealth;

        if (tempMaxHealthCoroutine != null)
        {
            // Extend old coroutine
            tempMaxHealthEndTime += duration;
            tempHealthIconDuration.text = $"{Mathf.CeilToInt(tempMaxHealthEndTime - Time.time)}s";
            tempHealthIconAmount.text = tempMaxHealth.ToString();
        }
        else
        {
            // Start a new coroutine
            tempMaxHealthEndTime = Time.time + duration;
            tempMaxHealthCoroutine = StartCoroutine(DisplayMaxHealthBuff());
            tempHealthIconDuration.text = $"{Mathf.CeilToInt(tempMaxHealthEndTime - Time.time)}s";
            tempHealthIconAmount.text = tempMaxHealth.ToString();
        }
    }


    private IEnumerator DisplayMaxHealthBuff()
    {
        while (Time.time < tempMaxHealthEndTime)
        {
            tempHealthIconDuration.text = $"{Mathf.CeilToInt(tempMaxHealthEndTime - Time.time)}s";
            yield return new WaitForSeconds(1f);
        }

        maxHealth -= tempMaxHealth;
        tempMaxHealth = 0;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        tempMaxHealthCoroutine = null;
        tempHealthIcon.SetActive(false);
    }


    public float GetCurrentMana()
    {
        return currentMana;
    }


    public void SetCurrentMana(float value)
    {
        currentMana = value;

        if (currentMana > maxMana)
        {
            currentMana = maxMana;
        }
    }


    public void SetManaFull()
    {
        currentMana = maxMana;
    }


    public void AddMana(float amount)
    {
        currentMana += amount;

        if (currentMana > maxMana)
        {
            currentMana = maxMana;
        }
    }


    public bool HasMana(float amount)
    {
        return currentMana >= amount;
    }


    public void UseMana(float amount)
    {
        currentMana -= amount;

        if (currentMana < 0)
        {
            currentMana = 0;
        }
    }


    public float GetMaxMana()
    {
        return maxMana;
    }


    public void SetMaxMana(float value)
    {
        maxMana = value;
    }


    public void AddMaxMana(float amount)
    {
        maxMana += amount;
    }


    public void AddTempMaxMana(int additionalMaxMana, float duration)
    {
        tempManaIcon.SetActive(true);

        tempMaxMana += additionalMaxMana;
        maxMana += additionalMaxMana;

        if (tempMaxManaCoroutine != null)
        {
            // Extend old coroutine
            tempMaxManaEndTime += duration;
            tempManaIconDuration.text = $"{Mathf.CeilToInt(tempMaxManaEndTime - Time.time)}s";
            tempManaIconAmount.text = tempMaxMana.ToString();
        }
        else
        {
            // Start a new coroutine
            tempMaxManaEndTime = Time.time + duration;
            tempMaxManaCoroutine = StartCoroutine(DisplayMaxManaBuff());
            tempManaIconDuration.text = $"{Mathf.CeilToInt(tempMaxManaEndTime - Time.time)}s";
            tempManaIconAmount.text = tempMaxMana.ToString();
        }
    }


    private IEnumerator DisplayMaxManaBuff()
    {
        while (Time.time < tempMaxManaEndTime)
        {
            tempManaIconDuration.text = $"{Mathf.CeilToInt(tempMaxManaEndTime - Time.time)}s";
            yield return new WaitForSeconds(1f);
        }

        maxMana -= tempMaxMana;
        tempMaxMana = 0;

        if (currentMana > maxMana)
        {
            currentMana = maxMana;
        }

        tempMaxManaCoroutine = null;
        tempManaIcon.SetActive(false);
    }


    public void Freeze(float duration)
    {
        if (freezeCoroutine != null)
        {
            StopCoroutine(freezeCoroutine);
        }

        isFrozen = true;
        freezeIcon.SetActive(true);
        spriteRenderer.color = Color.blue;
        freezeEndTime = Time.time + duration;
        freezeCoroutine = StartCoroutine(ApplyFreeze(duration));
    }


    private IEnumerator ApplyFreeze(float duration)
    {
        while (Time.time < freezeEndTime)
        {
            freezeIconTimer.text = $"{Mathf.CeilToInt(freezeEndTime - Time.time)}s";
            yield return null;
        }

        freezeCoroutine = null;
        isFrozen = false;
        freezeIcon.SetActive(false);
        ResetSpriteColor();
    }


    public int GetStrength()
    {
        return strength;
    }


    public void SetStrength(int value)
    {
        strength = value;
    }


    public int GetIntelligence()
    {
        return intelligence;
    }


    public void SetIntelligence(int value)
    {
        intelligence = value;
    }


    public int GetWisdom()
    {
        return wisdom;
    }


    public void SetWisdom(int value)
    {
        wisdom = value;
    }

    public float GetDamageBoost()
    {
        float modifier = 1f;

        // Each point of intelligence increases damage by 10%, adjustements might be needed when game balance becomes more clear
        modifier += intelligence * 0.1f;

        // Each point of shieldAmount increases damage by 1%, adjustements might be needed when game balance becomes more clear
        modifier += shieldAmount * 0.01f;

        return modifier;
    }
}