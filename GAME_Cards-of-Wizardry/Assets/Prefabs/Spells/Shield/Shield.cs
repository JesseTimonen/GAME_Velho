using UnityEngine;

public class Shield : MonoBehaviour
{
    [SerializeField] private int shieldAmount = 20;
    [SerializeField] private bool enableShieldDamageBuff = false;
    [SerializeField] private bool enableShieldHeal = false;

    public void Start()
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

        Invoke("DestroyGameObject", 2f);
    }


    private void DestroyGameObject()
    {
        Destroy(gameObject);
    }
}
