using UnityEngine;


public class EnemyIceball : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private int minDamage = 8;
    [SerializeField] private int maxDamage = 12;
    [SerializeField] private float lifetime = 10f;
    [SerializeField] private float freezeDuration = 3f;

    private Vector2 direction;


    private void Start()
    {
        Destroy(gameObject, lifetime);
    }


    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }


    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection;

        // Set the rotation to face the movement direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController playerController = GameManager.Instance.GetPlayerController();
            playerController.TakeDamage(Mathf.RoundToInt(Random.Range(minDamage, maxDamage) * GameManager.Instance.GetSurvivalModifier()));
            playerController.Freeze(freezeDuration);
            Destroy(gameObject);
        }
    }
}
