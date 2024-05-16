using UnityEngine;


public class CursorManager : MonoBehaviour
{
    [SerializeField] private Texture2D[] cursorTextureArray;
    [SerializeField] private float animationFrameTime;
    [SerializeField] private Vector2 cursorOffset = Vector2.zero;
    [SerializeField] private CursorMode cursorMode = CursorMode.Auto;

    private int currentCursorFrame;
    private int cursorFrameCount;
    private float cursorFrameTimer;


    private void Awake()
    {
        if (cursorTextureArray == null || cursorTextureArray.Length == 0)
        {
            Debug.LogWarning("CursorManager: No cursor textures set");
            return;
        }

        currentCursorFrame = 0;
        cursorFrameTimer = animationFrameTime;
        cursorFrameCount = cursorTextureArray.Length;
        UpdateCursor();
    }


    private void UpdateCursor()
    {
        Cursor.SetCursor(cursorTextureArray[currentCursorFrame], cursorOffset, cursorMode);
    }


    private void Update()
    {
        // Skip update if animation is disabled or not necessary
        if (animationFrameTime <= 0 || cursorFrameCount <= 1) return;

        cursorFrameTimer -= Time.deltaTime;
        if (cursorFrameTimer <= 0f)
        {
            cursorFrameTimer += animationFrameTime;
            currentCursorFrame = (currentCursorFrame + 1) % cursorFrameCount;
            UpdateCursor();
        }
    }
}
