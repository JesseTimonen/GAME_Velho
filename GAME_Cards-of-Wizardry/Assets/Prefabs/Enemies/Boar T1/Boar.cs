using UnityEngine;


public class Boar : MonoBehaviour
{
    private EnemyStats stats;
    private Transform player;
    private PlayerController playerController;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Animator animator;
    private CircleCollider2D attackCollider;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float rushSpeed = 10f;
    [SerializeField] private float rushRadius = 5f;
    [SerializeField] private float preRushTime = 1f;
    [SerializeField] private float rushDuration = 3f;
    [SerializeField] private float recoveryTime = 5f;

    [Header("Damage")]
    [SerializeField] private int minDamage = 15;
    [SerializeField] private int maxDamage = 25;
    [SerializeField] private float knockbackForce = 2f;
    [SerializeField] private float knockbackDuration = 0.3f;

    private bool isRushing = false;
    private bool isRecovering = false;
    private bool isPreRushing = false;
    private float rushTimer;
    private float recoveryTimer;
    private float preRushTimer;
    private Vector2 rushDirection;


    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        stats = GetComponent<EnemyStats>();
        rb = GetComponent<Rigidbody2D>();
        attackCollider = GetComponent<CircleCollider2D>();
        attackCollider.isTrigger = true;
        attackCollider.enabled = false;
    }


    private void Start()
    {
        player = GameManager.Instance.GetPlayerTransform();
        playerController = GameManager.Instance.GetPlayerController();
    }


    private void Update()
    {
        if (stats.IsAlive() && !stats.IsFrozen())
        {
            if (isRecovering)
            {
                recoveryTimer -= Time.deltaTime;

                if (recoveryTimer <= 0)
                {
                    isRecovering = false;
                    animator.SetBool("isWalking", true);
                }
            }
            else if (isRushing)
            {
                rushTimer -= Time.deltaTime;
                RushTowardsPlayer();

                if (rushTimer <= 0)
                {
                    isRushing = false;
                    isRecovering = true;
                    recoveryTimer = recoveryTime;
                    animator.SetBool("isRushing", false);
                    attackCollider.enabled = false;
                }
            }
            else if (isPreRushing)
            {
                preRushTimer -= Time.deltaTime;
                spriteRenderer.flipX = (player.position - transform.position).normalized.x > 0;

                if (preRushTimer <= 0)
                {
                    isPreRushing = false;
                    isRushing = true;
                    rushDirection = (player.position - transform.position).normalized;
                    animator.SetBool("isRushing", true);
                    rushTimer = rushDuration;
                    spriteRenderer.flipX = rushDirection.x > 0;
                    attackCollider.enabled = true;
                }
            }
            else
            {
                if (Vector2.Distance(transform.position, player.position) <= rushRadius)
                {
                    isPreRushing = true;
                    preRushTimer = preRushTime;
                    animator.SetBool("isWalking", false);
                }
                else
                {
                    WalkTowardsPlayer();
                }
            }
        }
    }


    private void WalkTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = direction * walkSpeed;
        spriteRenderer.flipX = direction.x > 0;
    }


    private void RushTowardsPlayer()
    {
        rb.velocity = rushDirection * rushSpeed;
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isRushing && other.CompareTag("Player"))
        {
            // Damage player
            playerController.TakeDamage(Mathf.RoundToInt(Random.Range(minDamage, maxDamage) * GameManager.Instance.GetSurvivalModifier()));

            // Knockback player
            Vector2 knockbackDirection = (player.position - transform.position).normalized;
            playerController.Knockback(knockbackDirection, knockbackForce, knockbackDuration);
        }
    }


    // Called from animation event
    public void EnemyDied()
    {
        attackCollider.enabled = false;
        rb.velocity = Vector2.zero;
    }


    // Called from animation event
    public void DestroyGameobject()
    {
        Destroy(gameObject);
    }
}