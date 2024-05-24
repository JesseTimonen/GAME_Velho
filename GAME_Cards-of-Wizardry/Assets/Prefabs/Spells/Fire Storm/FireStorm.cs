using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using FirstGearGames.SmoothCameraShaker;

public class FireStorm : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float duration = 5f;
    [SerializeField] private int minDamage = 60;
    [SerializeField] private int maxDamage = 85;
    [SerializeField] private float burnDuration = 8f;
    [SerializeField] private bool castedByBoss = false;

    [Header("Shake")]
    [SerializeField] private ShakeData explosionShake;

    [Header("Light Animation")]
    [SerializeField] private float initialLightIntensity = 5f;
    [SerializeField] private float midLightIntensity = 25f;
    private const float finalLightIntensity = 0f;
    private const float initialFade = 1f;
    private const float midFade = 0.75f;
    private const float finalFade = 0f;
    private const float initialBorderThickness = 0.2f;
    private const float finalBorderThickness = 1f;

    private Renderer materialRenderer;
    private Light2D light2D;
    private float startTime;

    private void Start()
    {
        materialRenderer = GetComponent<Renderer>();
        light2D = GetComponent<Light2D>();

        startTime = Time.time;

        StartCoroutine(AnimateDissolveEffect());

        if (PlayerPrefs.GetInt("DisableScreenShake", 0) == 0)
        {
            CameraShakerHandler.Shake(explosionShake);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        bool isEnemy = other.CompareTag("Enemy");
        bool isPlayer = other.CompareTag("Player");

        if (!isEnemy && !isPlayer) return;

        float damageModifier = 1 + Time.time - startTime;
        if (damageModifier > duration - 1f) return;

        float playerDamageModifier = 1f;
        if (!castedByBoss)
        {
           playerDamageModifier = GameManager.Instance.GetPlayerController().GetDamageBoost();
        }

        int finalDamage = CalculateDamage(playerDamageModifier, damageModifier);
        float adjustedBurnDuration = burnDuration / damageModifier;

        if (isEnemy)
        {
            ApplyDamageToEnemy(other, finalDamage, adjustedBurnDuration);
        }
        else if (isPlayer)
        {
            ApplyDamageToPlayer(finalDamage, adjustedBurnDuration);
        }
    }

    private int CalculateDamage(float playerDamageModifier, float damageModifier)
    {
        float baseDamage = Random.Range(minDamage, maxDamage);
        float adjustedDamage = baseDamage / damageModifier;
        return Mathf.RoundToInt(playerDamageModifier * adjustedDamage);
    }

    private void ApplyDamageToEnemy(Collider2D other, int finalDamage, float adjustedBurnDuration)
    {
        EnemyStats enemy = other.GetComponent<EnemyStats>();
        if (enemy == null) return;

        enemy.TakeDamage(finalDamage);

        if (burnDuration > 0)
        {
            enemy.SetOnFire(adjustedBurnDuration);
        }
    }

    private void ApplyDamageToPlayer(int finalDamage, float adjustedBurnDuration)
    {
        PlayerController playerController = GameManager.Instance.GetPlayerController();

        float selfDamageReduction = castedByBoss ? 1 : 0.5f;
        playerController.TakeDamage(Mathf.RoundToInt(finalDamage * selfDamageReduction));

        if (burnDuration > 0)
        {
            playerController.SetOnFire(adjustedBurnDuration * selfDamageReduction);
        }
    }

    private IEnumerator AnimateDissolveEffect()
    {
        float halfDuration = duration / 2;

        yield return StartCoroutine(AnimateLightAndFade(initialFade, midFade, initialLightIntensity, midLightIntensity, halfDuration));
        yield return StartCoroutine(AnimateLightAndFade(midFade, finalFade, midLightIntensity, finalLightIntensity, halfDuration * 0.5f));

        Destroy(gameObject);
    }

    private IEnumerator AnimateLightAndFade(float startFade, float endFade, float startIntensity, float endIntensity, float duration)
    {
        float timer = 0;
        while (timer < duration)
        {
            float progress = timer / duration;
            materialRenderer.material.SetFloat("_Fade", Mathf.Lerp(startFade, endFade, progress));
            materialRenderer.material.SetFloat("_BorderThickness", Mathf.Lerp(initialBorderThickness, finalBorderThickness, progress));
            light2D.intensity = Mathf.Lerp(startIntensity, endIntensity, progress);
            timer += Time.deltaTime;
            yield return null;
        }
    }
}
