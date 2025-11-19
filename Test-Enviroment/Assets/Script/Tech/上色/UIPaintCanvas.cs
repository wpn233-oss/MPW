using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIPaintCanvas : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("画布设置")]
    public RawImage targetImage;
    public Renderer potteryRenderer;

    [Header("笔刷设置")]
    public ColoringPainter painter;

    [Header("画布尺寸设置")]
    public float canvasWidth = 800f;
    public float heightScaleFactor = 1.2f;
    public float minCanvasHeight = 400f;

    [Header("柔边画笔设置")]
    public AnimationCurve brushFalloff = AnimationCurve.EaseInOut(0, 1, 1, 0);
    public float softBrushSizeMultiplier = 2f;

    [Header("油漆桶设置")]
    public float colorTolerance = 0.1f;

    private int textureSize = 1024;
    private Texture2D paintTex;
    private RectTransform rect;
    private Vector2Int? lastPixelPos = null;

    private float potteryHeight;
    private Vector2 originalCanvasSize;

    private Stack<Color[]> undoStack = new Stack<Color[]>();
    private const int MaxUndoSteps = 10;

    private bool isDrawing = false;
    private Color[] preDrawState;

    // 用于连续循环坐标
    private float lastContinuousX = 0f;
    private bool hasContinuousPos = false;

    void Start()
    {
        rect = targetImage.rectTransform;
        originalCanvasSize = rect.sizeDelta;

        paintTex = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        Clear(Color.white);
        targetImage.texture = paintTex;

        if (potteryRenderer != null)
            potteryRenderer.material.mainTexture = paintTex;

        CalculatePotteryHeight();
        AdjustCanvasSize();
        SaveUndoState();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z))
            UndoLastStroke();

        if (painter != null && !Input.GetMouseButton(1))
            UpdateBrushCursor();
    }

    // ---------- 尺寸 ----------
    void CalculatePotteryHeight()
    {
        if (potteryRenderer != null)
            potteryHeight = potteryRenderer.bounds.size.y;
        else potteryHeight = 2f;
    }

    void AdjustCanvasSize()
    {
        if (rect == null) return;
        float canvasHeight = Mathf.Max(potteryHeight * heightScaleFactor, minCanvasHeight);
        rect.sizeDelta = new Vector2(canvasWidth, canvasHeight);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    // ---------- 鼠标 ----------
    public void OnPointerDown(PointerEventData e)
    {
        if (e.button != PointerEventData.InputButton.Left) return;

        SavePreDrawState();

        // 初始化轨迹状态
        lastPixelPos = null;
        hasContinuousPos = false;
        lastContinuousX = 0f;
        isDrawing = true;

        Draw(e);
    }

    public void OnDrag(PointerEventData e)
    {
        if (e.button != PointerEventData.InputButton.Left || !isDrawing) return;
        Draw(e);
    }

    public void OnPointerUp(PointerEventData e)
    {
        if (e.button != PointerEventData.InputButton.Left) return;
        if (isDrawing)
        {
            SaveUndoState();
            isDrawing = false;
            hasContinuousPos = false;
            lastPixelPos = null;
        }
    }

    public void OnPointerEnter(PointerEventData e)
    {
        if (painter != null &&
            (painter.currentTool == ColoringPainter.ToolType.Brush ||
             painter.currentTool == ColoringPainter.ToolType.Spray))
        {

        }
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (painter != null) ;
    }

    // ---------- 绘制 ----------
    void Draw(PointerEventData e)
    {
        if (Input.GetMouseButton(1)) return;
        if (painter == null) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, e.position, e.pressEventCamera, out Vector2 local))
        {
            float uvX = (local.x + rect.rect.width / 2) / rect.rect.width;
            float uvY = (local.y + rect.rect.height / 2) / rect.rect.height;

            float currentContinuousX = uvX * textureSize;
            float currentY = uvY * textureSize;

            Color drawColor = painter.brushColor;
            float texBrushSize = (painter.brushSize / rect.rect.width) * textureSize;

            switch (painter.currentTool)
            {
                case ColoringPainter.ToolType.Brush:
                    DrawBrushLoop(currentContinuousX, currentY, texBrushSize, drawColor);
                    break;

                case ColoringPainter.ToolType.Spray:
                    // 柔边不参与循环逻辑
                    DrawSoftBrush(Mathf.RoundToInt(currentContinuousX), Mathf.RoundToInt(currentY), texBrushSize * softBrushSizeMultiplier, drawColor);
                    paintTex.Apply();
                    break;

                case ColoringPainter.ToolType.PaintBucket:
                    PaintBucketFill(Mathf.RoundToInt(currentContinuousX), Mathf.RoundToInt(currentY), drawColor);
                    paintTex.Apply();
                    SaveUndoState();
                    isDrawing = false;
                    lastPixelPos = null;
                    break;
            }
        }
    }

    // ---------- 循环笔刷 ----------
    void DrawBrushLoop(float currentX, float currentY, float brushSize, Color color)
    {
        // 第一次落笔
        if (!hasContinuousPos)
        {
            PaintCircleAtWrapped(Mathf.RoundToInt(currentX), Mathf.RoundToInt(currentY), brushSize, color);
            hasContinuousPos = true;
            lastContinuousX = currentX;
            lastPixelPos = new Vector2Int(Mathf.RoundToInt(currentX), Mathf.RoundToInt(currentY));
            paintTex.Apply();
            return;
        }

        // 有上次点，进行插值
        float deltaX = currentX - lastContinuousX;
        if (deltaX > textureSize / 2f) deltaX -= textureSize;
        else if (deltaX < -textureSize / 2f) deltaX += textureSize;

        float deltaY = currentY - lastPixelPos.Value.y;
        float distance = Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY);
        int steps = Mathf.CeilToInt(distance * 2f);
        steps = Mathf.Clamp(steps, 1, 200);

        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            float interpX = lastContinuousX + t * deltaX;
            float interpY = Mathf.Lerp(lastPixelPos.Value.y, currentY, t);

            int drawX = Mathf.FloorToInt(interpX % textureSize);
            if (drawX < 0) drawX += textureSize;
            int drawY = Mathf.Clamp(Mathf.RoundToInt(interpY), 0, textureSize - 1);

            PaintCircleAtWrapped(drawX, drawY, brushSize, color);
        }

        lastContinuousX += deltaX;
        lastPixelPos = new Vector2Int(Mathf.RoundToInt(currentX % textureSize), Mathf.RoundToInt(currentY));
        paintTex.Apply();
    }

    void PaintCircleAtWrapped(int cx, int cy, float brushSize, Color color)
    {
        int r = Mathf.CeilToInt(brushSize);
        for (int ox = -r; ox <= r; ox++)
        {
            for (int oy = -r; oy <= r; oy++)
            {
                if (ox * ox + oy * oy <= brushSize * brushSize)
                {
                    int drawX = (cx + ox + textureSize) % textureSize; // 环绕 X
                    int drawY = cy + oy;
                    if (drawY >= 0 && drawY < textureSize)
                        paintTex.SetPixel(drawX, drawY, color);
                }
            }
        }
    }

    // ---------- 柔边笔刷（普通） ----------
    void DrawSoftBrush(int centerX, int centerY, float brushSize, Color brushColor)
    {
        int radius = Mathf.CeilToInt(brushSize);
        int startX = Mathf.Max(0, centerX - radius);
        int endX = Mathf.Min(textureSize - 1, centerX + radius);
        int startY = Mathf.Max(0, centerY - radius);
        int endY = Mathf.Min(textureSize - 1, centerY + radius);

        for (int y = startY; y <= endY; y++)
        {
            for (int x = startX; x <= endX; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));
                float normalized = dist / radius;
                if (normalized <= 1f)
                {
                    float alpha = brushFalloff.Evaluate(normalized);
                    Color current = paintTex.GetPixel(x, y);
                    Color final = Color.Lerp(current, brushColor, alpha);
                    paintTex.SetPixel(x, y, final);
                }
            }
        }
    }

    // ---------- 油漆桶 ----------
    void PaintBucketFill(int x, int y, Color newColor)
    {
        x = (x + textureSize) % textureSize;
        if (y < 0 || y >= textureSize) return;

        Color target = paintTex.GetPixel(x, y);
        if (ApproximatelyEqualColor(target, newColor)) return;

        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        stack.Push(new Vector2Int(x, y));
        visited.Add(new Vector2Int(x, y));

        while (stack.Count > 0)
        {
            Vector2Int p = stack.Pop();
            Color c = paintTex.GetPixel((p.x + textureSize) % textureSize, p.y);
            if (ColorDistance(c, target) <= colorTolerance)
            {
                paintTex.SetPixel((p.x + textureSize) % textureSize, p.y, newColor);
                TryAddNeighbor(p.x + 1, p.y, stack, visited, target);
                TryAddNeighbor(p.x - 1, p.y, stack, visited, target);
                TryAddNeighbor(p.x, p.y + 1, stack, visited, target);
                TryAddNeighbor(p.x, p.y - 1, stack, visited, target);
            }
        }
    }

    void TryAddNeighbor(int x, int y, Stack<Vector2Int> stack, HashSet<Vector2Int> visited, Color target)
    {
        int wrapX = (x + textureSize) % textureSize;
        if (y < 0 || y >= textureSize) return;
        Vector2Int pos = new Vector2Int(wrapX, y);
        if (visited.Contains(pos)) return;
        Color c = paintTex.GetPixel(wrapX, y);
        if (ColorDistance(c, target) <= colorTolerance)
        {
            stack.Push(pos);
            visited.Add(pos);
        }
    }

    // ---------- 撤销 ----------
    void SavePreDrawState() => preDrawState = paintTex.GetPixels();

    void SaveUndoState()
    {
        if (preDrawState == null) return;
        if (undoStack.Count >= MaxUndoSteps)
        {
            var temp = undoStack.ToArray();
            undoStack.Clear();
            for (int i = 1; i < MaxUndoSteps; i++) undoStack.Push(temp[i]);
        }
        undoStack.Push(preDrawState);
        preDrawState = null;
    }

    void UndoLastStroke()
    {
        if (undoStack.Count == 0) return;
        Color[] last = undoStack.Pop();
        paintTex.SetPixels(last);
        paintTex.Apply();
        isDrawing = false;
        lastPixelPos = null;
    }

    // ---------- 通用 ----------
    float ColorDistance(Color a, Color b)
    {
        return Mathf.Sqrt(Mathf.Pow(a.r - b.r, 2) + Mathf.Pow(a.g - b.g, 2) + Mathf.Pow(a.b - b.b, 2));
    }

    bool ApproximatelyEqualColor(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) < 0.01f &&
               Mathf.Abs(a.g - b.g) < 0.01f &&
               Mathf.Abs(a.b - b.b) < 0.01f;
    }

    void Clear(Color c)
    {
        Color[] cols = new Color[textureSize * textureSize];
        for (int i = 0; i < cols.Length; i++) cols[i] = c;
        paintTex.SetPixels(cols);
        paintTex.Apply();
        undoStack.Clear();
        preDrawState = null;
        isDrawing = false;
        lastPixelPos = null;
        SaveUndoState();
    }

    public void ClearCanvas() => Clear(Color.white);

    // ---------- 光标 ----------
    void UpdateBrushCursor()
    {
        if (painter == null) return;

        if (painter.currentTool == ColoringPainter.ToolType.Brush ||
            painter.currentTool == ColoringPainter.ToolType.Spray)
        {
            Vector3 mousePos = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            Plane canvasPlane = new Plane(Vector3.forward, transform.position);
            if (canvasPlane.Raycast(ray, out float distance))
            {
                Vector3 worldPos = ray.GetPoint(distance);

            }

        }

    }
}
