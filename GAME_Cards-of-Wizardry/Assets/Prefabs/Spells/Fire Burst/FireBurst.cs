using UnityEngine;

public class FireBurst : MonoBehaviour
{
    [SerializeField] private GameObject fireball;
    [SerializeField] private int numberOfFireballs = 4;

    private void Start()
    {
        transform.position = GameManager.Instance.GetPlayerTransform().position;
        FireInDirections();
        Invoke("DestroyGameObject", 3f);
    }


    private void FireInDirections()
    {
        float angle = 0f;
        float angleStep = 360 / numberOfFireballs;

        for (int i = 0; i < numberOfFireballs; i++)
        {
            float fireballDirectionAngle = angle + (i * angleStep);
            float dirX = Mathf.Cos(fireballDirectionAngle * Mathf.Deg2Rad);
            float dirY = Mathf.Sin(fireballDirectionAngle * Mathf.Deg2Rad);
            Vector3 fireballDirection = new Vector3(dirX, dirY, 0f);

            GameObject fireballInstance = Instantiate(fireball, transform.position, Quaternion.identity);
            fireballInstance.GetComponent<FireBall>().SetDirection(fireballDirection);
        }
    }

    private void DestroyGameObject()
    {
        Destroy(gameObject);
    }
}
