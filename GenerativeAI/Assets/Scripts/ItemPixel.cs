using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ItemPixel : MonoBehaviour
{
    public Config config;
    public DrawingManager drawingManager;
    public TextMeshProUGUI TMP_itemLabel;
    [HideInInspector] public string controlNetIMGName;
    public RawImage guideCanvas;
    public Texture2D[] armorIMGs;
    public Texture2D[] swordIMGs;
    public Texture2D[] axeIMGs;
    public Texture2D[] beltIMGs;
    public Texture2D[] helmetIMGs;
    public Texture2D[] bootsIMGs;
    public Texture2D[] glovesIMGs;
    public Texture2D[] pantsIMGs;
    public Slider alphaSlider;
    public Toggle useToggle;

    private Dictionary<string, Texture2D[]> itemIMGs = new Dictionary<string, Texture2D[]>();

    void Start()
    {
        itemIMGs.Add("armor", armorIMGs);
        itemIMGs.Add("sword", swordIMGs);
        itemIMGs.Add("axe", axeIMGs);
        itemIMGs.Add("belt", beltIMGs);
        itemIMGs.Add("helmet", helmetIMGs);
        itemIMGs.Add("boots", bootsIMGs);
        itemIMGs.Add("gloves", glovesIMGs);
        itemIMGs.Add("pants", pantsIMGs);

        var items = itemIMGs["sword"];
        var index = UnityEngine.Random.Range(0, items.Length);
        guideCanvas.texture = items[index];
        guideCanvas.color = new Color(1, 1, 1, alphaSlider.value);

        alphaSlider.onValueChanged.AddListener(delegate {SetGuideAlpha();});
    }
    public void SaveControlNetImage()
    {
        string filePath = Application.streamingAssetsPath + "/ControlNet" + String.Format("/{0}_controlnet.png", TMP_itemLabel.text);
        controlNetIMGName = filePath.Split('/')[^1];
        Debug.Log(controlNetIMGName);
        var canvas = useToggle.isOn ? guideCanvas : drawingManager.drawingCanvas;
        SaveImageAsPNG(canvas, filePath, 1024);
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

    public void SetGuideImage()
    {
        var items = itemIMGs[TMP_itemLabel.text];
        var index = UnityEngine.Random.Range(0, items.Length);
        guideCanvas.texture = items[index];
    }  

    public void SetGuideAlpha()
    {
        guideCanvas.color = new Color(1, 1, 1, alphaSlider.value);
    }
}
