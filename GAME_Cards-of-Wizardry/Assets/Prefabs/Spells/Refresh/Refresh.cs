using UnityEngine;

public class Refresh : MonoBehaviour
{
    [SerializeField] private float manaGain = 25f;

    void Start()
    {
        GameManager.Instance.GetPlayerController().AddMana(manaGain);


        // Give audio source time to play spell audio
        Invoke(nameof(DestroyGameObject), 3f);
    }

    private void DestroyGameObject()
    {
        Destroy(gameObject);
    }
}
