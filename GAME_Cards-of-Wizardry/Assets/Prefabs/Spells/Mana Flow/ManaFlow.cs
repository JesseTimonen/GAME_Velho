using UnityEngine;

public class ManaFlow : MonoBehaviour
{
    [SerializeField][Range(0, 1)] private float healthSacrifice = 0.5f;
    [SerializeField][Range(0, 1)] private float manaReplenish = 0.5f;
    [SerializeField] private float tempManaMultiplier = 2f;
    [SerializeField] private float tempManaDuration = 10f;

    private void Start()
    {
        PlayerController playerController = GameManager.Instance.GetPlayerController();
        int currentHealth = playerController.GetCurrentHealth();
        int maxMana = playerController.GetMaxMana();
        int tempMana = playerController.GetTemporaryMana();

        playerController.SetCurrentHealth(Mathf.CeilToInt(currentHealth - (currentHealth * healthSacrifice)));

        if (tempManaMultiplier > 1)
        {
            playerController.AddTempMaxMana(maxMana - tempMana, tempManaDuration);
        }

        playerController.AddMana(maxMana * manaReplenish);

        // Give time for audio to play
        Invoke(nameof(DestroyGameObject), 5f);
    }

    private void DestroyGameObject()
    {
        Destroy(gameObject);
    }
}
