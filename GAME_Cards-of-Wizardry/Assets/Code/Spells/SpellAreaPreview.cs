using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SpellAreaPreview : MonoBehaviour
{
    [SerializeField] private float radius = 5f;
    [SerializeField] private int segments = 100;
    [SerializeField] private bool disallowMapBorderPassing = false;
    [SerializeField] private Color normalColor = new Color(255f, 255f, 255f, 1f);
    [SerializeField] private Color ivalidColor = new Color(255f, 0f, 0f, 1f);

    private LineRenderer lineRenderer;
    private Vector3 lastPosition;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lastPosition = transform.position;

        lineRenderer.startColor = normalColor;
        lineRenderer.endColor = normalColor;

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
        bool overlapsWithBorders = false;

        for (int i = 0; i <= segments; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            float y = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
            Vector3 position = new Vector3(x, y, 0) + transform.position;
            lineRenderer.SetPosition(i, position);

            // Check for overlap with map borders
            if (disallowMapBorderPassing && Physics2D.OverlapPoint(position, LayerMask.GetMask("MapBorders")))
            {
                overlapsWithBorders = true;
            }

            angle += 360f / segments;
        }

        if (disallowMapBorderPassing && overlapsWithBorders)
        {
            lineRenderer.startColor =ivalidColor;
            lineRenderer.endColor = ivalidColor;
        }
        else
        {
            lineRenderer.startColor = normalColor;
            lineRenderer.endColor = normalColor;
        }
    }
}
