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
        if (collision.CompareTag("Enemy") && !skipHit)
        {
            EnemyStats enemy = collision.gameObject.GetComponent<EnemyStats>();
            enemy.AddHealth(Mathf.RoundToInt(Random.Range(minHeal, maxHeal) * GameManager.Instance.GetSurvivalModifier()));
            Destroy(gameObject);
        }
        else
        {
            skipHit = false;
        }
    }
}
