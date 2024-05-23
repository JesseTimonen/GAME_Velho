using System.Collections;
using UnityEngine;

public class Meteor : MonoBehaviour
{
    [SerializeField] private GameObject timerCircle;
    [SerializeField] private float warningDuration = 2f;
    [SerializeField] private GameObject explosionPrefab;

    private void Start()
    {
        StartCoroutine(WarningAndExplosionSequence());
    }

    private IEnumerator WarningAndExplosionSequence()
    {
        // Ensure the timer circle starts at zero scale
        timerCircle.transform.localScale = Vector3.zero;

        // Scale up the timer circle over the warning duration
        float elapsed = 0f;
        while (elapsed < warningDuration)
        {
            timerCircle.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, elapsed / warningDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        timerCircle.transform.localScale = Vector3.one;

        // Instantiate the explosion and destroy the meteor
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
