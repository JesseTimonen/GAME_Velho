using UnityEngine;

public class Slime : MonoBehaviour
{
    private EnemyStats stats;
    private SpriteRenderer spriteRenderer;
    private Transform player;
    private PlayerController playerController;
    private Rigidbody2D rb;
    private CircleCollider2D attackCollider;

    [Header("Movement")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float jumpCooldownMin = 2f;
    [SerializeField] private float jumpCooldownMax = 3f;
    private float jumpTimer;

    [Header("Damage")]
    [SerializeField] private int minDamage = 15;
    [SerializeField] private int maxDamage = 25;
    [SerializeField] private float selfKnockbackForce = 5f;
    [SerializeField] private float knockbackForce = 2f;
    [SerializeField] private float knockbackDuration = 0.3f;

    [Header("Size Modifier")]
    [SerializeField] private bool enableSizeModifier;
    [SerializeField] private float sizeModifierMin;
    [SerializeField] private float sizeModifierMax;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        stats = GetComponent<EnemyStats>();
        rb = GetComponent<Rigidbody2D>();
        attackCollider = GetComponent<CircleCollider2D>();
        attackCollider.enabled = false;
    }

    private void Start()
    {
        player = GameManager.Instance.GetPlayerTransform();
        playerController = GameManager.Instance.GetPlayerController();

        ResetJumpTimer();

        if (enableSizeModifier)
        {
            ApplySizeModifier();
        }
    }

    private void ApplySizeModifier()
    {
        float sizeModifier = Random.Range(sizeModifierMin, sizeModifierMax);
        transform.localScale = new Vector3(sizeModifier, sizeModifier, sizeModifier);
        stats.SetMaxHealth(Mathf.RoundToInt(stats.GetMaxHealth() * sizeModifier));
        stats.SetHealth(stats.GetMaxHealth());
        minDamage = Mathf.RoundToInt(minDamage * sizeModifier);
        maxDamage = Mathf.RoundToInt(maxDamage * sizeModifier);
    }

    private void Update()
    {
        if (jumpTimer > 0)
        {
            jumpTimer -= Time.deltaTime;
        }
        else if (stats.IsAlive() && !stats.IsFrozen())
        {
            JumpTowardsPlayer();
        }
    }

    private void ResetJumpTimer()
    {
        jumpTimer = Random.Range(jumpCooldownMin, jumpCooldownMax);
    }

    private void JumpTowardsPlayer()
    {
        stats.animator.SetTrigger("Jump");

        Vector2 direction = (player.position - transform.position).normalized;
        spriteRenderer.flipX = direction.x < 0;

        rb.velocity = Vector2.zero;
        rb.AddForce(direction * jumpForce, ForceMode2D.Impulse);

        ResetJumpTimer();

        attackCollider.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (stats.IsFrozen() || !other.CompareTag("Player")) return;

        attackCollider.enabled = false;
        ApplyDamageToPlayer();
        ApplyKnockback();
    }

    private void ApplyDamageToPlayer()
    {
        int damage = Mathf.RoundToInt(Random.Range(minDamage, maxDamage) * GameManager.Instance.GetSurvivalModifier());
        playerController.TakeDamage(damage);
    }

    private void ApplyKnockback()
    {
        // Knockback player
        Vector2 knockbackDirection = (player.position - transform.position).normalized;
        playerController.Knockback(knockbackDirection, knockbackForce, knockbackDuration);

        // Knockback itself
        rb.AddForce(-knockbackDirection * selfKnockbackForce, ForceMode2D.Impulse);
    }

    // Called from animation event
    public void EndJump()
    {
        attackCollider.enabled = false;
    }

    // Called from animation event
    public void EnemyDied()
    {
        attackCollider.enabled = false;
        rb.velocity = Vector2.zero;
    }
}
