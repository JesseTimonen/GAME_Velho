using UnityEngine;
using UnityEditor;


[RequireComponent(typeof(LineRenderer))]
public class SpellDirectionPreview : MonoBehaviour
{
    public bool useCone = false;
    public int coneSegments = 10;
    public float coneAngle = 10f;
    private float halfConeAngle;

    private LineRenderer lineRenderer;
    private Transform playerTransform;

    private Quaternion[] precomputedRotations;
    private Vector3 lastPlayerPosition;
    private Vector3 lastMousePosition;


    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        playerTransform = GameManager.Instance.GetPlayerTransform();
        halfConeAngle = coneAngle / 2f;
        PrecomputeRotations();

        if (useCone)
        {
            lineRenderer.positionCount = coneSegments + 2;
        }
        else
        {
            lineRenderer.positionCount = 2;
        }
    }


    private void PrecomputeRotations()
    {
        precomputedRotations = new Quaternion[coneSegments + 1];
        for (int i = 0; i <= coneSegments; i++)
        {
            float angle = -halfConeAngle + (i * (coneAngle / coneSegments));
            precomputedRotations[i] = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }


    void Update()
    {
        if (lastPlayerPosition != playerTransform.position || lastMousePosition != transform.position)
        {
            if (useCone)
            {
                DrawCone(playerTransform.position, transform.position);
            }
            else
            {
                DrawLine();
            }

            lastPlayerPosition = playerTransform.position;
            lastMousePosition = transform.position;
        }

    }


    void DrawLine()
    {
        lineRenderer.SetPosition(0, playerTransform.position);
        lineRenderer.SetPosition(1, transform.position);
    }


    void DrawCone(Vector3 startPosition, Vector3 endPosition)
    {
        Vector3 direction = (endPosition - startPosition).normalized;
        float lineDistance = Vector3.Distance(startPosition, endPosition);
        lineRenderer.SetPosition(0, startPosition);

        for (int i = 0; i <= coneSegments; i++)
        {
            Vector3 coneDirection = precomputedRotations[i] * direction * lineDistance;
            lineRenderer.SetPosition(i + 1, startPosition + coneDirection);
        }

        lineRenderer.SetPosition(coneSegments + 1, startPosition);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SpellDirectionPreview))]
public class SpellDirectionPreviewEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SpellDirectionPreview script = (SpellDirectionPreview)target;

        // Always draw the common controls
        script.useCone = EditorGUILayout.Toggle("Use Cone", script.useCone);

        // Only draw cone options if useCone is true
        if (script.useCone)
        {
            script.coneAngle = EditorGUILayout.FloatField("Cone Angle", script.coneAngle);
            script.coneSegments = EditorGUILayout.IntField("Cone Segments", script.coneSegments);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(script);
        }
    }
}
#endif