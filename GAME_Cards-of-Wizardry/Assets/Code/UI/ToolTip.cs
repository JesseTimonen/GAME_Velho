using UnityEngine;
using TMPro;
using UnityEditor;

public class ToolTip : MonoBehaviour
{
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipText;
    [HideInInspector] public string tooltipContent;

    private void OnDisable()
    {
        tooltipPanel.SetActive(false);
    }

    public void ShowToolTip()
    {
        tooltipPanel.SetActive(true);

        if (!string.IsNullOrEmpty(tooltipContent) && tooltipText != null)
        {
            tooltipText.text = tooltipContent;
        }
    }

    public void HideToolTip()
    {
        tooltipPanel.SetActive(false);
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(ToolTip))]
public class ToolTipEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ToolTip toolTip = (ToolTip)target;

        EditorGUILayout.LabelField("Tooltip Content", EditorStyles.boldLabel);
        toolTip.tooltipContent = EditorGUILayout.TextArea(toolTip.tooltipContent, GUILayout.Height(75));

        if (GUI.changed)
        {
            EditorUtility.SetDirty(toolTip);
        }
    }
}
#endif