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
    [SerializeField] private float initialLightIntensity = 5;
    [SerializeField] private float midLightIntensity = 15;
    private float finalLightIntensity = 0.0f;
    private float initialFade = 1.0f;
    private float midFade = 0.75f;
    private float finalFade = 0.0f;
    private float initialBorderThickness = 0.2f;
    private float finalBorderThickness = 1.0f;

    private Renderer ShaderRenderer;
    private Light2D light2D;
    private float startTime;


    private void Start()
    {
        startTime = Time.time;
        ShaderRenderer = GetComponent<Renderer>();
        light2D = GetComponent<Light2D>();

        StartCoroutine(AnimateDissolveEffect());
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        bool isEnemy = other.CompareTag("Enemy");
        bool isPlayer = other.CompareTag("Player");

        if (!isEnemy && !isPlayer)
        {
            return;
        }

        float damageModifier = 1 + Time.time - startTime;
        PlayerController playerController = GameManager.Instance.GetPlayerController();
        float playerDamageModifier = playerController.GetDamageBoost();
        float baseDamage = Random.Range(minDamage, maxDamage);
        float adjustedDamage = baseDamage / damageModifier;
        int finalDamage = Mathf.RoundToInt(playerDamageModifier * adjustedDamage);

        if (isEnemy)
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
        else if (isPlayer)
        {
            playerController.TakeDamage(finalDamage);

            if (freezeDuration > 0)
            {
                float adjustedFreezeDuration = playerFreezeDuration / damageModifier;
                playerController.Freeze(adjustedFreezeDuration);
            }
        }
    }



    private IEnumerator AnimateDissolveEffect()
    {
        float halfDuration = duration / 2;

        // 1st half of animation
        float timer = 0;
        while (timer < halfDuration)
        {
            float progress = timer / halfDuration;
            ShaderRenderer.material.SetFloat("_Fade", Mathf.Lerp(initialFade, midFade, progress));
            ShaderRenderer.material.SetFloat("_BorderThickness", Mathf.Lerp(initialBorderThickness, finalBorderThickness, progress));
            light2D.intensity = Mathf.Lerp(initialLightIntensity, midLightIntensity, progress);
            timer += Time.deltaTime;
            yield return null;
        }

        // 2nd half of animation
        timer = 0;
        float lightOutDuration = halfDuration * 0.5f;
        float lightTimer = 0;

        while (timer < halfDuration)
        {
            float progress = timer / halfDuration;
            float lightProgress = lightTimer / lightOutDuration;

            ShaderRenderer.material.SetFloat("_Fade", Mathf.Lerp(midFade, finalFade, progress));

            if (lightTimer < lightOutDuration)
            {
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

        Destroy(gameObject);
    }
}
