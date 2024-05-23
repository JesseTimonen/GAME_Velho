using UnityEngine;

public class Heal : MonoBehaviour
{
    public int initialHeal = 0;
    public float healOverTimeDuration = 5;
    public int maximumHealthIncreas = 0;

    public void Start()
    {
        Transform player = GameManager.Instance.GetPlayerTransform();
        PlayerController playerController = GameManager.Instance.GetPlayerController();

        transform.position = player.position;
        transform.SetParent(player);

        if (maximumHealthIncreas > 0)
        {
            playerController.AddTempMaxHealth(maximumHealthIncreas, healOverTimeDuration);
        }

        if (initialHeal > 0)
        {
            playerController.AddHealth(initialHeal);
        }

        if (healOverTimeDuration > 0)
        {
            playerController.AddHealthOverTime(healOverTimeDuration);
        }

        // Give time for audio to play
        Invoke(nameof(DestroyGameObject), 5f);
    }

    private void DestroyGameObject()
    {
        Destroy(gameObject);
    }
}
