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

        if (healOverTimeDuration > 0)
        {
            playerController.AddHealthOverTime(healOverTimeDuration);
        }

        if (initialHeal > 0)
        {
            playerController.AddHealth(initialHeal);
        }

        if (maximumHealthIncreas > 0)
        {
            playerController.AddTempMaxHealth(maximumHealthIncreas, healOverTimeDuration);
            // playerController.AddHealth(maximumHealthIncreas);
        }

        Invoke("DestoryGameObject", 2f);
    }


    private void DestoryGameObject()
    {
        Destroy(gameObject);
    }
}
