using UnityEngine;

public class Refresh : MonoBehaviour
{
    [SerializeField] private float manaGain = 25f;

    void Start()
    {
        GameManager.Instance.GetPlayerController().AddMana(manaGain);
        Destroy(gameObject);
    }
}
