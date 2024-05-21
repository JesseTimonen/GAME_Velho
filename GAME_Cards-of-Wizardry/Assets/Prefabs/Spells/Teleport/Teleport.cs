using UnityEngine;

public class Teleport : MonoBehaviour
{

    private void Start()
    {
        Transform player = GameManager.Instance.GetPlayerTransform();
        player.position = transform.position;

        // Give time for audio to play
        Invoke("DestroyGameObject", 3f);
    }

    private void DestroyGameObject()
    {
        Destroy(gameObject);
    }
}
