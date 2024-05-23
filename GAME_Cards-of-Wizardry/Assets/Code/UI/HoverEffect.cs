using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class HoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Color hoverColor = Color.red;
    [SerializeField] private float hoverSize = 30f;
    private TMP_Text text;
    private Color originalColor;
    private float originalSize;

    void Start()
    {
        text = GetComponent<TMP_Text>();

        if (text == null)
        {
            Debug.LogWarning("HoverEffect requires a TMP_Text component on the same GameObject.");
            return;
        }

        originalColor = text.color;
        originalSize = text.fontSize;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        text.color = hoverColor;
        text.fontSize = hoverSize;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        text.color = originalColor;
        text.fontSize = originalSize;
    }

    // Public method to allow runtime customization
    public void SetHoverColor(Color newColor)
    {
        hoverColor = newColor;
    }

    // Public method to allow runtime customization
    public void SetHoverSize(float newSize)
    {
        hoverSize = newSize;
    }
}
