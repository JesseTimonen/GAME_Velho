using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SpellAreaPreview : MonoBehaviour
{
    [SerializeField] private float radius = 5f;
    [SerializeField] private int segments = 100;
    private LineRenderer lineRenderer;
    private Vector3 lastPosition;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lastPosition = transform.position;
        DrawCircle();
    }

    private void Update()
    {
        if (transform.position != lastPosition)
        {
            DrawCircle();
            lastPosition = transform.position;
        }
    }

    void DrawCircle()
    {
        lineRenderer.positionCount = segments + 1;
        lineRenderer.useWorldSpace = true;

        float angle = 0f;
        for (int i = 0; i <= segments; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            float y = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
            Vector3 position = new Vector3(x, y, 0) + transform.position;
            lineRenderer.SetPosition(i, position);
            angle += 360f / segments;
        }
    }
}
