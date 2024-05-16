using UnityEngine;


public class ToolTip : MonoBehaviour
{
    [SerializeField] private GameObject tooltipPanel;


    public void ShowToolTip()
    {
        tooltipPanel.SetActive(true);
    }

    public void HideToolTip()
    {
        tooltipPanel.SetActive(false);
    }
}
