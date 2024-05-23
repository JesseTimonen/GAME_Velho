using UnityEngine;

public class Refresh : MonoBehaviour
{
    [SerializeField] private float manaGain = 25f;

    private void Start()
    {
        GameManager.Instance.GetPlayerController().AddMana(manaGain);

        // Give time for audio to play
        Invoke(nameof(DestroyGameObject), 5f);
    }

    private void DestroyGameObject()
    {
        Destroy(gameObject);
    }
}
