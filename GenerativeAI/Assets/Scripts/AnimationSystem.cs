using UnityEngine;

public class AnimationSystem : MonoBehaviour
{
    private int width = 1024;
    private int height = 1024;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CaptureAnimation(); 
        }
    }

    public void CaptureAnimation()
    {
        Debug.Log("Start Capture ScreenShot");
        string filePath = Application.streamingAssetsPath + "/Animations" + string.Format("/{0}.png", System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
        
        // 새로운 RenderTexture 생성
        RenderTexture rt = new RenderTexture(width, height, 24);
        // 현재 활성 카메라의 타겟 텍스처를 rt로 변경
        Camera.main.targetTexture = rt;
        
        // 텍스처 생성
        Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGBA32, false);
        
        // 카메라 렌더링
        Camera.main.Render();
        
        // 활성 RenderTexture를 방금 생성한 것으로 설정
        RenderTexture.active = rt;
        
        // RenderTexture의 내용을 Texture2D에 읽어옴
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenShot.Apply();
        
        // 카메라 설정 복구
        Camera.main.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);
        
        // PNG 파일로 저장
        byte[] bytes = screenShot.EncodeToPNG();
        Debug.Log(filePath);
        System.IO.File.WriteAllBytes(filePath, bytes);
        Destroy(screenShot);
    }
}
