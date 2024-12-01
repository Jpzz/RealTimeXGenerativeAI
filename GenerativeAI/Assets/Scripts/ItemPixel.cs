using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

public class ItemPixel : MonoBehaviour
{
    public Config config;
    public DrawingManager drawingManager;
    public TextMeshProUGUI TMP_itemLabel;
    [HideInInspector] public string controlNetImageName;
    public void SaveControlNetImage()
    {
        string filePath = Application.streamingAssetsPath + "/ControlNet" + String.Format("/{0}_controlnet.png", TMP_itemLabel.text);
        controlNetImageName = filePath.Split('/')[^1];
        Debug.Log(controlNetImageName);
        SaveImageAsPNG(drawingManager.drawingCanvas, filePath, 1024);
    }
    public void SaveImageAsPNG(RawImage rawImage, string filePath, int size)
    {
        // RawImage의 텍스처를 Texture2D로 변환
        Texture2D tex = new Texture2D(
            size,
            size,
            TextureFormat.RGBA32,
            false);
            
        // 현재 RawImage의 픽셀 데이터를 읽어옴
        RenderTexture rt = RenderTexture.GetTemporary(size, size);
        Graphics.Blit(rawImage.texture, rt);
        RenderTexture.active = rt;
        tex.ReadPixels(
            new Rect(0, 0, tex.width, tex.height),
            0, 0);
        tex.Apply();
        
        // PNG 파일로 저장
        byte[] bytes = tex.EncodeToPNG();
        System.IO.File.WriteAllBytes(filePath, bytes);
        // 임시로 생성한 텍스처 정리
        Destroy(tex);
    }    
}
