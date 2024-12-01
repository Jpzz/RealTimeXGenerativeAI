using UnityEngine;
using UnityEngine.UI;

public class DrawingManager : MonoBehaviour
{
    public RawImage drawingCanvas;
    public int textureSize = 512;
    private Texture2D drawTexture;
    private bool isDrawing = false;
    private bool isEraser = false;
    
    private Color drawColor = Color.black;
    private Color eraserColor = Color.white;
    private int brushSize = 5;
    private Vector2 lastDrawPosition;

    void Start()
    {
        InitializeCanvas();
    }

    void InitializeCanvas()
    {
        // 새로운 텍스처 생성
        drawTexture = new Texture2D(textureSize, textureSize);
        
        // 텍스처를 흰색으로 초기화
        Color[] colors = new Color[textureSize * textureSize];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.white;
        }
        drawTexture.SetPixels(colors);
        drawTexture.Apply();

        // RawImage에 텍스처 할당
        drawingCanvas.texture = drawTexture;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDrawing = true;
            // 시작 위치 저장
            lastDrawPosition = GetTextureCoordinates(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDrawing = false;
        }

        if (isDrawing)
        {
            Vector2 currentPosition = GetTextureCoordinates(Input.mousePosition);
            DrawLine(lastDrawPosition, currentPosition);
            lastDrawPosition = currentPosition;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleEraser(true);
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleEraser(false);
        }
    }

    Vector2 GetTextureCoordinates(Vector3 mousePosition)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            drawingCanvas.rectTransform,
            mousePosition,
            null,
            out localPoint
        );

        float width = drawingCanvas.rectTransform.rect.width;
        float height = drawingCanvas.rectTransform.rect.height;
        
        float x = ((localPoint.x + width/2) * textureSize / width);
        float y = ((localPoint.y + height/2) * textureSize / height);

        return new Vector2(x, y);
    }

    void DrawLine(Vector2 start, Vector2 end)
    {
        // 두 점 사이의 거리 계산
        float distance = Vector2.Distance(start, end);
        
        // 거리에 따라 보간할 점의 개수 결정
        int steps = Mathf.Max(20, Mathf.CeilToInt(distance));
        
        // 각 보간 지점마다 원을 그림
        for (float i = 0; i <= steps; i++)
        {
            float t = i / steps;
            Vector2 drawPos = Vector2.Lerp(start, end, t);
            DrawPoint(Mathf.RoundToInt(drawPos.x), Mathf.RoundToInt(drawPos.y));
        }
        
        drawTexture.Apply();
    }

    void DrawPoint(int x, int y)
    {
        Color colorToUse = isEraser ? eraserColor : drawColor;
        
        for (int i = -brushSize; i <= brushSize; i++)
        {
            for (int j = -brushSize; j <= brushSize; j++)
            {
                if (i*i + j*j <= brushSize * brushSize)
                {
                    int pixelX = x + i;
                    int pixelY = y + j;
                    
                    if (pixelX >= 0 && pixelX < textureSize && 
                        pixelY >= 0 && pixelY < textureSize)
                    {
                        drawTexture.SetPixel(pixelX, pixelY, colorToUse);
                    }
                }
            }
        }
    }

    // 브러시/지우개 전환
    public void ToggleEraser(bool eraser)
    {
        isEraser = eraser;
    }

    // 캔버스 초기화
    public void ClearCanvas()
    {
        InitializeCanvas();
    }
} 