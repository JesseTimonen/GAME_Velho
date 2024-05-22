using UnityEngine;

public class Empower : MonoBehaviour
{
    [SerializeField] private float damageBuff = 25f;
    [SerializeField] private float duration = 10f;

    void Start()
    {
        GameManager.Instance.GetPlayerController().ActivateEmpower(damageBuff, duration);
        Destroy(gameObject);
    }
}
