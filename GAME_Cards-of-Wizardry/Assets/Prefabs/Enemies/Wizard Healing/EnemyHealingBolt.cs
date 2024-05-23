using UnityEngine;


public class EnemyHealingBolt : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private int minHeal = 35;
    [SerializeField] private int maxHeal = 42;
    [SerializeField] private float lifetime = 10f;

    private Vector2 direction;
    private bool skipHit = true;


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
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Make sure the healing projectile doesn't immidiatly collide with the healing wizard
        if (skipHit)
        {
            skipHit = false;
            return;
        }

        if (collision.CompareTag("Enemy"))
        {
            EnemyStats enemy = collision.gameObject.GetComponent<EnemyStats>();
            enemy.AddHealth(Mathf.RoundToInt(Random.Range(minHeal, maxHeal) * GameManager.Instance.GetSurvivalModifier()));
        }
        else if (collision.CompareTag("Player"))
        {
            PlayerController playerController = GameManager.Instance.GetPlayerController();
            playerController.AddHealth(Mathf.RoundToInt(Random.Range(minHeal, maxHeal) * GameManager.Instance.GetSurvivalModifier()));
        }

        Destroy(gameObject);
    }
}
