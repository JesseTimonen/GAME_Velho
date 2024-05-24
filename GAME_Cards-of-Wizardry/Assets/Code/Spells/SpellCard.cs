using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class SpellCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private SpellBook spellBook;
    [SerializeField] private SpellMastery spellMastery;
    [SerializeField] private Texture rechargeCardTexture;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private GameObject spellActionPromptText;
    [SerializeField] private Animator unableToPlayCardAnimator;
    [SerializeField] private TextMeshProUGUI unableToPlayCardText;
    [SerializeField] private GameObject discardPile;
    [SerializeField] private TextMeshProUGUI cooldownText;
    [SerializeField] private float discardDelayDuration = 5f;
    [SerializeField] private float showSpellPreviewDelay = 0.33f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip DrawCardAudio;
    [SerializeField] private AudioClip CancelDrawCardAudio;
    [SerializeField] private AudioClip noManaAudio;

    [Header("Card Stats")]
    [SerializeField] private TextMeshProUGUI manaCostText;
    [SerializeField] private TextMeshProUGUI cooldownDelayText;

    private DeckManager deckManager;
    private GameObject castingAreaIndicatorInstance;
    private int currentSpellIndex;
    private Vector3 startPosition;
    private Transform originalParent;
    private RawImage cardImage;
    private bool isDragging = false;
    private bool isRecharging = false;
    private int masteryLevelTier = 1;
    private bool spellCancelled = false;
    private bool spellDiscarded = false;
    private PointerEventData currentEventData;
    public float cooldownTimeRemaining = 0f;

    private Animator animator;
    private RuntimeAnimatorController originalController;

    private void Awake()
    {
        deckManager = GameManager.Instance.GetDeckManager();
        cardImage = GetComponent<RawImage>();
        animator = GetComponent<Animator>();
        startPosition = transform.position;
        originalParent = transform.parent;
        DrawNextCard();
    }

    private void OnEnable()
    {
        transform.position = startPosition;
        transform.SetParent(originalParent);
    }

    private void Update()
    {
        if (isDragging && castingAreaIndicatorInstance != null)
        {
            UpdateCastingAreaPosition();
        }

        if (isDragging)
        {
            if (Input.GetMouseButtonDown(1) || GameManager.Instance.UIPanelOpened)
            {
                spellCancelled = true;
                OnEndDrag(currentEventData);
            }
            else if (Input.GetMouseButtonDown(2))
            {
                spellDiscarded = true;
                OnEndDrag(currentEventData);
            }
        }

        if (isRecharging)
        {
            cooldownTimeRemaining -= Time.deltaTime;

            if (cooldownTimeRemaining <= 0)
            {
                FinishRecharge();
            }
            else
            {
                cooldownText.text = Mathf.Ceil(cooldownTimeRemaining).ToString();
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isRecharging) return;

        originalController = animator.runtimeAnimatorController;
        animator.runtimeAnimatorController = null;

        isDragging = true;
        transform.SetParent(transform.root);
        cardImage.raycastTarget = false;
        currentEventData = eventData;

        spellActionPromptText.SetActive(true);
        discardPile.SetActive(true);

        audioSource.clip = DrawCardAudio;
        audioSource.Play();

        // Activate spell preview only if the spell has one
        if ((masteryLevelTier == 1 && spellBook.spells[currentSpellIndex].basicSpellPreview != null) ||
            (masteryLevelTier == 2 && spellBook.spells[currentSpellIndex].flawlessSpellPreview != null) ||
            (masteryLevelTier == 3 && spellBook.spells[currentSpellIndex].masterfulSpellPreview != null))
        {
            StartCoroutine(ShowSpellPreview());
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isRecharging) return;

        if (castingAreaIndicatorInstance != null)
        {
            castingAreaIndicatorInstance.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));
        }
        else
        {
            transform.position = Input.mousePosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isRecharging) return;

        animator.runtimeAnimatorController = originalController;

        isDragging = false;
        transform.position = startPosition;
        transform.SetParent(originalParent);
        cardImage.enabled = true;
        spellActionPromptText.SetActive(false);
        discardPile.SetActive(false);
        cardImage.raycastTarget = true;

        if (castingAreaIndicatorInstance != null)
        {
            Destroy(castingAreaIndicatorInstance);
        }

        if (!spellCancelled)
        {
            manaCostText.text = "";
            cooldownDelayText.text = "";

            // Check if the drop was on the trash bin
            if (spellDiscarded || eventData.pointerCurrentRaycast.gameObject == discardPile)
            {
                StartRechargeSpell(discardDelayDuration);
                audioSource.clip = CancelDrawCardAudio;
                audioSource.Play();
            }
            else
            {
                TriggerCardEffect(eventData.position);
            }
        }
        else
        {
         if (masteryLevelTier == 1)
            {
                manaCostText.text = spellBook.spells[currentSpellIndex].basicManaCost.ToString();
                cooldownDelayText.text = spellBook.spells[currentSpellIndex].basicCooldownDelay.ToString();
            }
            else if (masteryLevelTier == 2)
            {
                manaCostText.text = spellBook.spells[currentSpellIndex].flawlessManaCost.ToString();
                cooldownDelayText.text = spellBook.spells[currentSpellIndex].flawlessCooldownDelay.ToString();
            }
            else
            {
                manaCostText.text = spellBook.spells[currentSpellIndex].masterfulManaCost.ToString();
                cooldownDelayText.text = spellBook.spells[currentSpellIndex].masterfulCooldownDelay.ToString();
            }

            audioSource.clip = CancelDrawCardAudio;
            audioSource.Play();
        }

        eventData.pointerDrag = null;
        spellCancelled = false;
        spellDiscarded = false;
    }

    private void DrawNextCard()
    {
        currentSpellIndex = deckManager.DrawNextCard();
        masteryLevelTier = spellMastery.GetSpellMasteryLevel(spellBook.spells[currentSpellIndex].name);

        if (masteryLevelTier == 1)
        {
            cardImage.texture = spellBook.spells[currentSpellIndex].basicCardSprite;
            manaCostText.text = spellBook.spells[currentSpellIndex].basicManaCost.ToString();
            cooldownDelayText.text = spellBook.spells[currentSpellIndex].basicCooldownDelay.ToString();
        }
        else if (masteryLevelTier == 2)
        {
            cardImage.texture = spellBook.spells[currentSpellIndex].flawlessCardSprite;
            manaCostText.text = spellBook.spells[currentSpellIndex].flawlessManaCost.ToString();
            cooldownDelayText.text = spellBook.spells[currentSpellIndex].flawlessCooldownDelay.ToString();
        }
        else
        {
            cardImage.texture = spellBook.spells[currentSpellIndex].masterfulCardSprite;
            manaCostText.text = spellBook.spells[currentSpellIndex].masterfulManaCost.ToString();
            cooldownDelayText.text = spellBook.spells[currentSpellIndex].masterfulCooldownDelay.ToString();
        }
    }

    private IEnumerator ShowSpellPreview()
    {
        yield return new WaitForSeconds(showSpellPreviewDelay);

        if (isDragging)
        {
            cardImage.enabled = false;
            manaCostText.text = "";
            cooldownDelayText.text = "";

            if (masteryLevelTier == 1)
            {
                castingAreaIndicatorInstance = Instantiate(spellBook.spells[currentSpellIndex].basicSpellPreview, transform.position, Quaternion.identity, transform.parent);
            }
            else if (masteryLevelTier == 2)
            {
                castingAreaIndicatorInstance = Instantiate(spellBook.spells[currentSpellIndex].flawlessSpellPreview, transform.position, Quaternion.identity, transform.parent);
            }
            else
            {
                castingAreaIndicatorInstance = Instantiate(spellBook.spells[currentSpellIndex].masterfulSpellPreview, transform.position, Quaternion.identity, transform.parent);
            }

            castingAreaIndicatorInstance.transform.SetAsLastSibling();
            UpdateCastingAreaPosition();
        }
    }

    private void TriggerCardEffect(Vector3 screenPosition)
    {
        if (isRecharging) return;

        SpellBook.Spell spell = spellBook.spells[currentSpellIndex];

        int manaCost;
        if (masteryLevelTier == 1)
        {
            manaCost = spell.basicManaCost;
        }
        else if (masteryLevelTier == 2)
        {
            manaCost = spell.flawlessManaCost;
        }
        else
        {
            manaCost = spell.masterfulManaCost;
        }

        if (!playerController.IsFrozen() && playerController.HasMana(manaCost))
        {
            // Convert screen position to world position
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, Camera.main.nearClipPlane));
            Vector3 spawnPosition = new Vector3(worldPosition.x, worldPosition.y, 0);

            // Check if the spell is "Teleport" and if the position overlaps with MapBorders
            if (spell.name == "Teleport")
            {
                Collider2D[] hitColliders = Physics2D.OverlapPointAll(spawnPosition);
                foreach (var hitCollider in hitColliders)
                {
                    if (hitCollider.gameObject.layer == LayerMask.NameToLayer("MapBorders"))
                    {
                        audioSource.clip = noManaAudio;
                        audioSource.Play();
                        return;
                    }
                }
            }

            playerController.UseMana(manaCost);
            playerAnimator.SetTrigger("Attack");

            if (masteryLevelTier == 1)
            {
                Instantiate(spell.basicSpellPrefab, spawnPosition, Quaternion.identity);
            }
            else if (masteryLevelTier == 2)
            {
                Instantiate(spell.flawlessSpellPrefab, spawnPosition, Quaternion.identity);
            }
            else
            {
                Instantiate(spell.masterfulSpellPrefab, spawnPosition, Quaternion.identity);
            }

            castingAreaIndicatorInstance = null;
            spellMastery.IncreaseSpellUsage(spell.name);
            StartRechargeSpell();
        }
        else
        {
            if (masteryLevelTier == 1)
            {
                manaCostText.text = spellBook.spells[currentSpellIndex].basicManaCost.ToString();
                cooldownDelayText.text = spellBook.spells[currentSpellIndex].basicCooldownDelay.ToString();
            }
            else if (masteryLevelTier == 2)
            {
                manaCostText.text = spellBook.spells[currentSpellIndex].flawlessManaCost.ToString();
                cooldownDelayText.text = spellBook.spells[currentSpellIndex].flawlessCooldownDelay.ToString();
            }
            else
            {
                manaCostText.text = spellBook.spells[currentSpellIndex].masterfulManaCost.ToString();
                cooldownDelayText.text = spellBook.spells[currentSpellIndex].masterfulCooldownDelay.ToString();
            }

            if (playerController.IsFrozen())
            {
                unableToPlayCardText.text = "Frozen";
                unableToPlayCardAnimator.SetTrigger("Show");
                audioSource.clip = noManaAudio;
                audioSource.Play();
            }
            else if (!playerController.HasMana(manaCost))
            {
                unableToPlayCardText.text = "Out of mana";
                unableToPlayCardAnimator.SetTrigger("Show");
                audioSource.clip = noManaAudio;
                audioSource.Play();
            }
        }
    }


    public void StartRechargeSpell(float cooldownDuration = -1f)
    {
        manaCostText.text = "";
        cooldownDelayText.text = "";

        if (cooldownDuration == -1f)
        {
            if (masteryLevelTier == 1)
            {
                cooldownDuration = spellBook.spells[currentSpellIndex].basicCooldownDelay;
            }
            else if (masteryLevelTier == 2)
            {
                cooldownDuration = spellBook.spells[currentSpellIndex].flawlessCooldownDelay;
            }
            else
            {
                cooldownDuration = spellBook.spells[currentSpellIndex].masterfulCooldownDelay;
            }
        }

        cooldownTimeRemaining = cooldownDuration;
        cardImage.raycastTarget = false;
        isRecharging = true;
        cardImage.color = new Color(255f, 255f, 255f, 200 / 255f);
        cooldownText.gameObject.SetActive(true);
        cardImage.texture = rechargeCardTexture;
    }

    private void FinishRecharge()
    {
        cardImage.color = Color.white;
        cooldownText.gameObject.SetActive(false);
        cardImage.raycastTarget = true;
        isRecharging = false;

        DrawNextCard();
    }

    private void UpdateCastingAreaPosition()
    {
        Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane);
        castingAreaIndicatorInstance.transform.position = Camera.main.ScreenToWorldPoint(mousePos);
    }
}
