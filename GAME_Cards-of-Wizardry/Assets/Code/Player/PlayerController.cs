using DamageNumbersPro;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("MOVEMENT")]
    private float moveSpeed = 3f;
    private bool isKnockedBack = false;
    private Vector2 knockbackVelocity;

    [Header("STATS")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth = 100;
    [SerializeField] private int tempMaxHealth = 0;
    [SerializeField] private int maxMana = 100;
    [SerializeField] private float currentMana = 100;
    [SerializeField] private int tempMaxMana = 0;
    [SerializeField] private float manaRechargeRate = 3f;
    [SerializeField] private int strength = 1;
    [SerializeField] private int intelligence = 1;
    [SerializeField] private int wisdom = 1;
    [SerializeField] private int shieldAmount = 0;
    [SerializeField] private bool shieldDamageBuffEnabled = false;
    [SerializeField] private bool shieldHealEnabled = false;

    [Header("REFERENCES")]
    [SerializeField] private InputController inputController;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private MusicManager musicManager;
    private Rigidbody2D rb;
    private Camera mainCamera;

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
    [SerializeField] private GameObject empowerIcon;
    [SerializeField] private TextMeshProUGUI empowerIconTimer;
    [SerializeField] private TextMeshProUGUI empowerIconValue;

    [Header("SHIELDS")]
    [SerializeField] private ParticleSystem shieldParticles;
    [SerializeField] private ParticleSystem shieldInnerParticles;
    [SerializeField] private GameObject ShieldUIElement;
    [SerializeField] private TextMeshProUGUI ShieldValueText;
    [SerializeField] private GameObject ShieldDamageUIElement;
    [SerializeField] private TextMeshProUGUI ShieldDamageValueText;
    [SerializeField] private float shieldMaxDamageAt;
    [SerializeField] private ShieldTimer shieldTimer;
    [SerializeField] private ShieldDamageTimer shieldDamageTimer;

    [Header("Burning")]
    [SerializeField] private ParticleSystem burningParticles;

    [Header("UI")]
    [SerializeField] private Transform canvas;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI manaText;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider manaSlider;
    [SerializeField] private Transform additionalHealthBarCanvas;
    [SerializeField] private Slider additionalHealthSlider;
    [SerializeField] private Slider additionalManaSlider;

    [Header("Floating numbers")]
    [SerializeField] private Transform floatingDamageSpawnPoint;
    [SerializeField] private GameObject healFloatingPrefab;
    [SerializeField] private GameObject manaFloatingPrefab;
    [SerializeField] private GameObject damageFloatingPrefab;
    [SerializeField] private GameObject burnFloatingPrefab;
    [SerializeField] private GameObject freezeFloatingPrefab;
    [SerializeField] private GameObject absorbedFloatingPrefab;

    [Header("Death")]
    [SerializeField] private GameObject dieScreenPanel;
    [SerializeField] private GameObject immortalityBuff;
    private string lastPlayedSong = "";

    private Coroutine burnCoroutine;
    private float burnEndTime;
    private Coroutine healCoroutine;
    private float healEndTime;
    private Coroutine empowerCoroutine;
    private float empowerEndTime;
    private Coroutine tempMaxHealthCoroutine;
    private float tempMaxHealthEndTime;
    private Coroutine tempMaxManaCoroutine;
    private float tempMaxManaEndTime;
    private Coroutine freezeCoroutine;
    private float freezeEndTime;

    private bool isFrozen = false;
    private bool isLookingRight = true;
    private bool isImmortal = false;
    private bool isdead = false;
    private float empowerDamageBuff = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        UpdateStatsUI();

        if (isFrozen) return;

        if (currentMana < GetMaxMana())
        {
            RechargeMana();
        }

        RotatePlayer();
    }

    private void FixedUpdate()
    {
        if (isFrozen)
        {
            rb.velocity = Vector2.zero;
            playerAnimator.SetBool("IsMoving", false);
            return;
        }

        if (!isKnockedBack)
        {
            Move();
        }
    }

    private void UpdateStatsUI()
    {
        healthText.text = Mathf.Ceil(currentHealth).ToString();
        healthSlider.value = Mathf.Ceil(currentHealth);
        healthSlider.maxValue = Mathf.Ceil(GetMaxHealth());
        manaText.text = Mathf.Floor(currentMana).ToString();
        manaSlider.value = Mathf.Floor(currentMana);
        manaSlider.maxValue = Mathf.Floor(GetMaxMana());

        additionalHealthSlider.value = Mathf.Ceil(currentHealth);
        additionalHealthSlider.maxValue = Mathf.Ceil(GetMaxHealth());
        additionalManaSlider.value = Mathf.Floor(currentMana);
        additionalManaSlider.maxValue = Mathf.Floor(GetMaxMana());
    }

    private void Move()
    {
        if (isdead) return;

        playerAnimator.SetBool("IsMoving", (inputController.Move.x != 0 || inputController.Move.y != 0));

        rb.velocity = inputController.Move * GetRunSpeed();
    }

    public float GetRunSpeed()
    {
        // Strength increases movement speed by 2% per rank
        return moveSpeed * (1 + 0.02f * (strength - 1));
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
            Vector3 healthBarEulerAngles = additionalHealthBarCanvas.localRotation.eulerAngles;
            Vector3 floatingTextEulerAngles = floatingDamageSpawnPoint.localRotation.eulerAngles;

            if (distance > 0 && !isLookingRight)
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);

                healthBarEulerAngles.y = 180;
                additionalHealthBarCanvas.localRotation = Quaternion.Euler(healthBarEulerAngles);

                floatingTextEulerAngles.y = 180;
                floatingDamageSpawnPoint.localRotation = Quaternion.Euler(floatingTextEulerAngles);

                isLookingRight = true;
            }
            else if (distance < 0 && isLookingRight)
            {
                transform.rotation = Quaternion.Euler(0, 180, 0);

                healthBarEulerAngles.y = 0;
                additionalHealthBarCanvas.localRotation = Quaternion.Euler(healthBarEulerAngles);

                floatingTextEulerAngles.y = 0;
                floatingDamageSpawnPoint.localRotation = Quaternion.Euler(floatingTextEulerAngles);

                isLookingRight = false;
            }
        }
    }

    private void RechargeMana()
    {
        // Wisdom increases mana recharge by 5% per rank
        currentMana = Mathf.Min(GetMaxMana(), currentMana + GetCurrentManaRecharge() * Time.deltaTime);
    }

    public void TakeDamage(int amount, bool isFireDamage = false)
    {
        if (shieldAmount > 0)
        {
            HandleShield(ref amount);

            if (amount <= 0)
            {
                InstantiateFloatingText(absorbedFloatingPrefab);
                return;
            }
        }

        spriteRenderer.color = Color.red;
        Invoke(nameof(ResetSpriteColor), 0.33f);
        ApplyDamage(amount, isFireDamage);
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

    private void ApplyDamage(int amount, bool isFireDamage = false)
    {
        int damageTaken = Mathf.RoundToInt(amount * (1 - GetDamageReduction()));

        currentHealth -= damageTaken;

        GameObject instantiatedfloatingText = InstantiateFloatingText(isFireDamage ? burnFloatingPrefab : damageFloatingPrefab);
        instantiatedfloatingText.GetComponent<DamageNumberMesh>().number = damageTaken;

        if (currentHealth <= 0)
        {
            if (isImmortal)
            {
                currentHealth = 1;
            }
            else
            {
                currentHealth = 0;
                Die();
            }
        }
    }

    public float GetDamageReduction()
    {
        return (float)strength / (strength + 10);
    }

    public void SetOnFire(float duration)
    {
        fireIcon.SetActive(true);

        if (!burningParticles.isPlaying)
        {
            burningParticles.Play();
        }

        UpdateCoroutine(ref burnCoroutine, ApplyBurn(duration), ref burnEndTime, duration);

        fireIconTimer.text = $"{Mathf.CeilToInt(burnEndTime - Time.time)}s";
    }

    private IEnumerator ApplyBurn(float duration)
    {
        while (Time.time < burnEndTime)
        {
            TakeDamage(10, true);
            fireIconTimer.text = $"{Mathf.CeilToInt(burnEndTime - Time.time)}s";
            yield return new WaitForSeconds(1f);
        }

        burnCoroutine = null;
        fireIcon.SetActive(false);
        burningParticles.Stop();
    }

    public void Freeze(float duration)
    {
        StopCoroutineIfExists(ref freezeCoroutine);

        InstantiateFloatingText(freezeFloatingPrefab);

        isFrozen = true;
        freezeIcon.SetActive(true);
        spriteRenderer.color = new Color(0f, 125f, 255f, 1f);
        UpdateCoroutine(ref freezeCoroutine, ApplyFreeze(duration), ref freezeEndTime, duration);
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

    private void ResetSpriteColor()
    {
        spriteRenderer.color = isFrozen ? Color.blue : Color.white;
    }

    public void Die()
    {
        if (isImmortal) return;

        isImmortal = true;
        isdead = true;
        GameManager.Instance.hasPlayerDied = true;

        lastPlayedSong = musicManager.currentlyPlaying;
        musicManager.PlayMusic("Ending");

        playerAnimator.SetBool("isDead", true);
        GameManager.Instance.UIPanelOpened = true;
        GameManager.Instance.HideBasicUI();

        StopCoroutineIfExists(ref healCoroutine);

        Invoke(nameof(DieScreen), 1f);
    }

    public void DieScreen()
    {
        dieScreenPanel.SetActive(true);
    }

    public void DefyDeath()
    {
        musicManager.PlayMusic(lastPlayedSong);

        immortalityBuff.SetActive(true);

        playerAnimator.SetBool("isDead", false);
        dieScreenPanel.SetActive(false);

        GameManager.Instance.UIPanelOpened = false;
        GameManager.Instance.ShowBasicUI();

        SetCurrentHealth(1);

        isdead = false;
    }

    public void AddShield(int amount)
    {
        shieldAmount += amount;
        UpdateShieldUI();

        shieldTimer.InitializeTimer();
        shieldDamageTimer.InitializeTimer();

        shieldParticles.Play();
        shieldInnerParticles.Play();
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
            float shieldDamage = (GetShieldDamageBoost() - 1) * 100;
            ShieldDamageValueText.text = $"{Mathf.Round(shieldDamage)}%";
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

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public void SetCurrentHealth(int value)
    {
        if (currentHealth > value)
        {
            GameObject instantiatedfloatingText = InstantiateFloatingText(damageFloatingPrefab);
            instantiatedfloatingText.GetComponent<DamageNumberMesh>().number = currentHealth - value;
        }
        else if (currentHealth < value)
        {
            GameObject instantiatedfloatingText = InstantiateFloatingText(healFloatingPrefab);
            instantiatedfloatingText.GetComponent<DamageNumberMesh>().leftText = "+" + (value - currentHealth);
        }

        currentHealth = Mathf.Min(value, GetMaxHealth());
    }

    public void SetHealthFull()
    {
        currentHealth = GetMaxHealth();
    }

    public void AddHealth(int amount)
    {
        currentHealth += amount;

        GameObject instantiatedfloatingText = InstantiateFloatingText(healFloatingPrefab);
        instantiatedfloatingText.GetComponent<DamageNumberMesh>().leftText = "+" + amount;

        if (currentHealth > GetMaxHealth())
        {
            currentHealth = GetMaxHealth();
        }

        spriteRenderer.color = Color.green;
        Invoke(nameof(ResetSpriteColor), 0.33f);
    }

    public void AddHealthOverTime(float duration)
    {
        healIcon.SetActive(true);

        UpdateCoroutine(ref healCoroutine, ApplyHealOverTime(duration), ref healEndTime, duration);

        healIconTimer.text = $"{Mathf.CeilToInt(healEndTime - Time.time)}s";
    }

    private IEnumerator ApplyHealOverTime(float duration)
    {
        while (Time.time < healEndTime)
        {
            AddHealth(Mathf.RoundToInt(GetMaxHealth() * 0.05f));
            healIconTimer.text = $"{Mathf.CeilToInt(healEndTime - Time.time)}s";
            yield return new WaitForSeconds(1f);
        }

        healCoroutine = null;
        healIcon.SetActive(false);
    }

    public int GetMaxHealth()
    {
        return maxHealth + tempMaxHealth;
    }

    public int GetTemporaryHealth()
    {
        return tempMaxHealth;
    }

    public void SetMaxHealth(int value)
    {
        maxHealth = value;
    }

    public void AddMaxHealth(int amount)
    {
        maxHealth += amount;
    }

    public void AddTempMaxHealth(int additionalMaxHealth, float duration)
    {
        tempHealthIcon.SetActive(true);

        tempMaxHealth = Mathf.Max(tempMaxHealth, additionalMaxHealth);
        UpdateCoroutine(ref tempMaxHealthCoroutine, DisplayMaxHealthBuff(), ref tempMaxHealthEndTime, duration);

        tempHealthIconDuration.text = $"{Mathf.CeilToInt(tempMaxHealthEndTime - Time.time)}s";
        tempHealthIconAmount.text = tempMaxHealth.ToString();
    }

    private IEnumerator DisplayMaxHealthBuff()
    {
        while (Time.time < tempMaxHealthEndTime)
        {
            tempHealthIconDuration.text = $"{Mathf.CeilToInt(tempMaxHealthEndTime - Time.time)}s";
            yield return new WaitForSeconds(1f);
        }

        tempMaxHealth = 0;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        tempMaxHealthCoroutine = null;
        tempHealthIcon.SetActive(false);
    }

    public float GetCurrentMana()
    {
        return currentMana;
    }

    public float GetCurrentManaRecharge()
    {
        return manaRechargeRate * (1 + 0.05f * (wisdom - 1));
    }

    public void SetCurrentMana(float value)
    {
        currentMana = Mathf.Min(value, GetMaxMana());
    }

    public void SetManaFull()
    {
        currentMana = GetMaxMana();
    }

    public void AddMana(float amount)
    {
        currentMana = Mathf.Min(currentMana + amount, GetMaxMana());

        GameObject instantiatedfloatingText = InstantiateFloatingText(manaFloatingPrefab);
        instantiatedfloatingText.GetComponent<DamageNumberMesh>().leftText = "+" + amount;
    }

    public bool HasMana(float amount)
    {
        return currentMana >= amount;
    }

    public void UseMana(float amount)
    {
        currentMana = Mathf.Max(currentMana - amount, 0);
    }

    public int GetMaxMana()
    {
        return maxMana + tempMaxMana;
    }

    public int GetTemporaryMana()
    {
        return tempMaxMana;
    }

    public void SetMaxMana(int value)
    {
        maxMana = value;
    }

    public void AddMaxMana(int amount)
    {
        maxMana += amount;
    }

    public void AddTempMaxMana(int additionalMaxMana, float duration)
    {
        tempManaIcon.SetActive(true);

        tempMaxMana = Mathf.Max(tempMaxMana, additionalMaxMana);
        UpdateCoroutine(ref tempMaxManaCoroutine, DisplayMaxManaBuff(), ref tempMaxManaEndTime, duration);

        tempManaIconDuration.text = $"{Mathf.CeilToInt(tempMaxManaEndTime - Time.time)}s";
        tempManaIconAmount.text = tempMaxMana.ToString();
    }

    private IEnumerator DisplayMaxManaBuff()
    {
        while (Time.time < tempMaxManaEndTime)
        {
            tempManaIconDuration.text = $"{Mathf.CeilToInt(tempMaxManaEndTime - Time.time)}s";
            yield return new WaitForSeconds(1f);
        }

        tempMaxMana = 0;
        currentMana = Mathf.Min(currentMana, maxMana);
        tempMaxManaCoroutine = null;
        tempManaIcon.SetActive(false);
    }

    public bool IsFrozen()
    {
        return isFrozen;
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

    public void ActivateEmpower(float damageBuff, float duration)
    {
        empowerIcon.SetActive(true);

        empowerDamageBuff = Mathf.Max(empowerDamageBuff, damageBuff);
        empowerIconValue.text = $"{empowerDamageBuff}%";

        UpdateCoroutine(ref empowerCoroutine, EmpowerCountDown(duration), ref empowerEndTime, duration);

        empowerIconTimer.text = $"{Mathf.CeilToInt(empowerEndTime - Time.time)}s";
    }

    private IEnumerator EmpowerCountDown(float duration)
    {
        while (Time.time < empowerEndTime)
        {
            empowerIconTimer.text = $"{Mathf.CeilToInt(empowerEndTime - Time.time)}s";
            yield return new WaitForSeconds(1f);
        }

        empowerCoroutine = null;
        empowerIcon.SetActive(false);
        empowerDamageBuff = 0;
    }

    public float GetEmpowerDamageBoost()
    {
        return 1 + (empowerDamageBuff / 100f);
    }

    public float GetIntelligenceDamageBoost()
    {
        // Each point of intelligence increases damage by 5%
        return 1f + (intelligence * 0.05f);
    }

    public float GetShieldDamageBoost()
    {
        if (!shieldDamageBuffEnabled) return 1f;

        // Each point of shieldAmount increases damage by 0.5%
        return 1f + (Mathf.Min(shieldAmount, shieldMaxDamageAt) * 0.0005f);
    }

    public float GetDamageBoost()
    {
        float intBoost = GetIntelligenceDamageBoost();
        float shieldBoost = GetShieldDamageBoost();
        float empowerBoost = GetEmpowerDamageBoost();

        return 1 + (intBoost - 1) + (shieldBoost - 1) + (empowerBoost - 1);
    }

    private GameObject InstantiateFloatingText(GameObject prefab)
    {
        GameObject instantiatedfloatingText = Instantiate(prefab, floatingDamageSpawnPoint.position, Quaternion.identity);
        instantiatedfloatingText.transform.SetParent(floatingDamageSpawnPoint);
        return instantiatedfloatingText;
    }

    private void StopCoroutineIfExists(ref Coroutine coroutine)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }

    private void UpdateCoroutine(ref Coroutine coroutine, IEnumerator routine, ref float endTime, float duration)
    {
        endTime = Time.time + duration;

        if (coroutine != null)
        {
            return;
        }

        coroutine = StartCoroutine(routine);
    }
}
