using System.Collections;
using UnityEngine;

public abstract class BossStageTwo : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected float closestRadius = 3f;
    [SerializeField] protected float furthestRadius = 8f;
    [SerializeField] protected float teleportRadius = 8f;
    [SerializeField] protected float teleportCooldownMin = 3f;
    [SerializeField] protected float teleportCooldownMax = 5f;
    protected float teleportTimer;
    protected bool isTeleporting = false;

    [Header("Materials")]
    [SerializeField] protected Material dissolveMaterial;
    [SerializeField] protected Material outlineMaterial;
    [SerializeField] protected float dissolveSpeed = 3f;
    [SerializeField] protected float materialSwapSpeed = 5f;
    private Renderer materialRenderer;

    [Header("Floating Text")]
    [SerializeField] private Transform floatingTextSpawnPoint;

    protected EnemyStats stats;
    protected Transform player;
    protected Rigidbody2D rb;
    protected Animator animator;
    protected BoxCollider2D boxCollider;

    protected enum BossState { Moving, UsingSkills }
    protected BossState bossState;

    private bool furthestRadiusExtended = false;
    private float originalFurthestRadius;

    protected virtual void Awake()
    {
        stats = GetComponent<EnemyStats>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        materialRenderer = GetComponent<Renderer>();
    }

    protected virtual void Start()
    {
        player = GameManager.Instance.GetPlayerTransform();
        teleportTimer = Random.Range(teleportCooldownMin, teleportCooldownMax);
        originalFurthestRadius = furthestRadius;
        bossState = BossState.Moving;
        animator.SetBool("isMoving", true);

        // Used to auto enable health bar on spawn
        stats.AddHealth(1);
    }

    protected virtual void Update()
    {
        if (!stats.IsAlive() || stats.IsFrozen()) return;

        HandleTeleportation();

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (bossState == BossState.Moving)
        {
            MoveToIdealRadius(distanceToPlayer);
            if (distanceToPlayer >= closestRadius && distanceToPlayer <= furthestRadius)
            {
                ExtendFurthestRadius();
                bossState = BossState.UsingSkills;
                animator.SetBool("isMoving", false);
            }
        }
        else if (bossState == BossState.UsingSkills)
        {
            if (distanceToPlayer < closestRadius || distanceToPlayer > furthestRadius)
            {
                ResetFurthestRadius();
                bossState = BossState.Moving;
                animator.SetBool("isMoving", true);
            }
            else
            {
                HandleAbilities();
            }
        }

        FlipGameObject();
    }

    protected virtual void MoveToIdealRadius(float distanceToPlayer)
    {
        Vector2 direction = Vector2.zero;

        if (distanceToPlayer < closestRadius)
        {
            direction = (transform.position - player.position).normalized;
        }
        else if (distanceToPlayer > furthestRadius)
        {
            direction = (player.position - transform.position).normalized;
        }

        rb.velocity = direction * moveSpeed;
    }

    protected abstract void HandleAbilities();

    protected void HandleTeleportation()
    {
        teleportTimer -= Time.deltaTime;
        if (teleportTimer <= 0 && !isTeleporting)
        {
            teleportTimer = Random.Range(teleportCooldownMin, teleportCooldownMax);
            StartCoroutine(Teleport());
        }
    }

    protected IEnumerator Teleport()
    {
        if (!isTeleporting)
        {
            isTeleporting = true;
            boxCollider.enabled = false;
            yield return StartCoroutine(DissolveEffect(false));
            transform.position = (Vector2)transform.position + Random.insideUnitCircle * teleportRadius;
            yield return StartCoroutine(DissolveEffect(true));
            boxCollider.enabled = true;
            isTeleporting = false;
        }
    }

    protected IEnumerator DissolveEffect(bool isAppearing)
    {
        if (!isAppearing)
        {
            // Step 1: Outline _Thickness from 0.4 to 0
            float outlineThickness = 0.4f;
            while (outlineThickness > 0f)
            {
                outlineThickness -= Time.unscaledDeltaTime * materialSwapSpeed;
                outlineMaterial.SetFloat("_Thickness", Mathf.Clamp(outlineThickness, 0f, 0.4f));
                yield return null;
            }

            // Step 2: Change material to dissolve
            materialRenderer.material = dissolveMaterial;

            // Step 3: Dissolve _Fade goes from 1 to 0
            float dissolveValue = 1f;
            while (dissolveValue > 0f)
            {
                dissolveValue -= Time.unscaledDeltaTime * dissolveSpeed;
                dissolveMaterial.SetFloat("_Fade", dissolveValue);
                yield return null;
            }
        }
        else
        {
            // Step 5: Dissolve _Fade goes from 0 to 1
            float dissolveValue = 0f;
            while (dissolveValue < 1f)
            {
                dissolveValue += Time.unscaledDeltaTime * dissolveSpeed;
                dissolveMaterial.SetFloat("_Fade", dissolveValue);
                yield return null;
            }

            // Step 6: Change material to outline
            materialRenderer.material = outlineMaterial;

            // Step 7: Outline _Thickness from 0 to 0.4
            float outlineThickness = 0f;
            while (outlineThickness < 0.4f)
            {
                outlineThickness += Time.unscaledDeltaTime * materialSwapSpeed;
                outlineMaterial.SetFloat("_Thickness", Mathf.Clamp(outlineThickness, 0f, 0.4f));
                yield return null;
            }
        }
    }

    protected void FlipGameObject()
    {
        bool facingLeft = (player.position - transform.position).normalized.x < 0;
        transform.rotation = Quaternion.Euler(0, facingLeft ? 0 : 180, 0);

        // Flipping game object also flips the child canvas, so we need to flip canvas also to make a full rotation back to original rotation
        Vector3 floatingTextEulerAngles = floatingTextSpawnPoint.localRotation.eulerAngles;
        floatingTextEulerAngles.y = facingLeft ? 0 : 180;
        floatingTextSpawnPoint.localRotation = Quaternion.Euler(floatingTextEulerAngles);
    }

    private void ExtendFurthestRadius()
    {
        if (!furthestRadiusExtended)
        {
            furthestRadius *= 1.75f;
            furthestRadiusExtended = true;
        }
    }

    private void ResetFurthestRadius()
    {
        if (furthestRadiusExtended)
        {
            furthestRadius = originalFurthestRadius;
            furthestRadiusExtended = false;
        }
    }
}
