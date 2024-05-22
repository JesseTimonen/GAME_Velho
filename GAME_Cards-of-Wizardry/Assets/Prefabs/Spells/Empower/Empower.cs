using UnityEngine;

public class Empower : MonoBehaviour
{
    [SerializeField] private float damageBuff = 25f;
    [SerializeField] private float duration = 10f;

    void Start()
    {
        GameManager.Instance.GetPlayerController().ActivateEmpower(damageBuff, duration);


        // Give audio source time to play spell audio
        Invoke(nameof(DestroyGameObject), 3f);
    }

    private void DestroyGameObject()
    {
        Destroy(gameObject);
    }
}
