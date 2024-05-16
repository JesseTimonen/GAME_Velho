using UnityEngine;

public class CameraTargetController : MonoBehaviour
{
    public Transform player;
    public float maxDistance = 1.0f;
    public float maxInfluenceDistance = 10.0f;

    private void Update()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        Vector3 direction = (mousePosition - player.position).normalized;
        float effectiveDistance = maxDistance * Mathf.Clamp(Vector3.Distance(player.position, mousePosition) / maxInfluenceDistance, 0, 1);
        transform.position = player.position + direction * effectiveDistance;
    }
}
