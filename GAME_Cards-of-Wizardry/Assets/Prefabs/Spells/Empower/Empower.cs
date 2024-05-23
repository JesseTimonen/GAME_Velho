using UnityEngine;

public class Empower : MonoBehaviour
{
    [SerializeField] private float damageBuff = 25f;
    [SerializeField] private float duration = 10f;

    private void Start()
    {
        GameManager.Instance.GetPlayerController().ActivateEmpower(damageBuff, duration);

        // Give time for audio to play
        Invoke(nameof(DestroyGameObject), 5f);
    }

    private void DestroyGameObject()
    {
        Destroy(gameObject);
    }
}
