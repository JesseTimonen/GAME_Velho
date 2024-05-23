using UnityEngine;

public class Shield : MonoBehaviour
{
    [SerializeField] private int shieldAmount = 20;
    [SerializeField] private bool enableShieldDamageBuff = false;
    [SerializeField] private bool enableShieldHeal = false;

    private void Start()
    {
        PlayerController playerController = GameManager.Instance.GetPlayerController();

        if (enableShieldDamageBuff)
        {
            playerController.EnableShieldDamageBuff();
        }

        if (enableShieldHeal)
        {
            playerController.EnableShieldHeal();
        }

        playerController.AddShield(shieldAmount);

        // Give time for audio to play
        Invoke(nameof(DestroyGameObject), 5f);
    }

    private void DestroyGameObject()
    {
        Destroy(gameObject);
    }
}
