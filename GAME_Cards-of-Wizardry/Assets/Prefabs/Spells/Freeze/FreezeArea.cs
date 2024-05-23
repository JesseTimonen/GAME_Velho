using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FreezeArea : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float duration = 5f;
    [SerializeField] private int minDamage = 60;
    [SerializeField] private int maxDamage = 85;
    [SerializeField] private float freezeDuration = 8f;
    [SerializeField] private float playerFreezeDuration = 2f;

    [Header("Light Animation")]
    [SerializeField] private float initialLightIntensity = 5f;
    [SerializeField] private float midLightIntensity = 15f;
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
        startTime = Time.time;
        materialRenderer = GetComponent<Renderer>();
        light2D = GetComponent<Light2D>();

        StartCoroutine(AnimateDissolveEffect());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        bool isEnemy = other.CompareTag("Enemy");
        bool isPlayer = other.CompareTag("Player");

        if (!isEnemy && !isPlayer) return;

        float damageModifier = 1 + Time.time - startTime;
        if (damageModifier > duration - 1f) return;

        PlayerController playerController = GameManager.Instance.GetPlayerController();
        float playerDamageModifier = playerController.GetDamageBoost();
        int finalDamage = CalculateDamage(playerDamageModifier, damageModifier);

        if (isEnemy)
        {
            ApplyDamageToEnemy(other, finalDamage, damageModifier);
        }
        else if (isPlayer)
        {
            ApplyDamageToPlayer(playerController, finalDamage, damageModifier);
        }
    }

    private int CalculateDamage(float playerDamageModifier, float damageModifier)
    {
        float baseDamage = Random.Range(minDamage, maxDamage);
        float adjustedDamage = baseDamage / damageModifier;
        return Mathf.RoundToInt(playerDamageModifier * adjustedDamage);
    }

    private void ApplyDamageToEnemy(Collider2D other, int finalDamage, float damageModifier)
    {
        EnemyStats enemy = other.GetComponent<EnemyStats>();
        if (enemy == null) return;

        enemy.TakeDamage(finalDamage);

        if (freezeDuration > 0)
        {
            float adjustedFreezeDuration = freezeDuration / damageModifier;
            enemy.Freeze(adjustedFreezeDuration);
        }
    }

    private void ApplyDamageToPlayer(PlayerController playerController, int finalDamage, float damageModifier)
    {
        playerController.TakeDamage(finalDamage);

        if (freezeDuration > 0)
        {
            float adjustedFreezeDuration = playerFreezeDuration / damageModifier;
            playerController.Freeze(adjustedFreezeDuration);
        }
    }

    private IEnumerator AnimateDissolveEffect()
    {
        float halfDuration = duration / 2;
        yield return StartCoroutine(AnimateFirstHalf(halfDuration));
        yield return StartCoroutine(AnimateSecondHalf(halfDuration));
        Destroy(gameObject);
    }

    private IEnumerator AnimateFirstHalf(float halfDuration)
    {
        float timer = 0;
        while (timer < halfDuration)
        {
            float progress = timer / halfDuration;
            materialRenderer.material.SetFloat("_Fade", Mathf.Lerp(initialFade, midFade, progress));
            materialRenderer.material.SetFloat("_BorderThickness", Mathf.Lerp(initialBorderThickness, finalBorderThickness, progress));
            light2D.intensity = Mathf.Lerp(initialLightIntensity, midLightIntensity, progress);
            timer += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator AnimateSecondHalf(float halfDuration)
    {
        float timer = 0;
        float lightOutDuration = halfDuration * 0.5f;
        float lightTimer = 0;

        while (timer < halfDuration)
        {
            float progress = timer / halfDuration;
            materialRenderer.material.SetFloat("_Fade", Mathf.Lerp(midFade, finalFade, progress));

            if (lightTimer < lightOutDuration)
            {
                float lightProgress = lightTimer / lightOutDuration;
                light2D.intensity = Mathf.Lerp(midLightIntensity, finalLightIntensity, lightProgress);
                lightTimer += Time.deltaTime;
            }
            else
            {
                light2D.intensity = finalLightIntensity;
            }

            timer += Time.deltaTime;
            yield return null;
        }
    }
}
