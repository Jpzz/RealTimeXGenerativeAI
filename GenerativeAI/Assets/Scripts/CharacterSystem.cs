using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
public class CharacterSystem : MonoBehaviour
{
    public enum Equipments
    {
        Armor, Pants, Boots, Weapon, Gloves, Shield
    }
    public enum Direction
    {
        Up, Down, Left, Right
    }
    public Equipments equipment;
    [Header("Character Parts")]
    public RectTransform[] characterParts;
    [Header("UI")]
    public Button[] movementButtons;
    [Header("Character Window System")]
    public RawImage[] rawImages;
    public Slider scaleSlider;
    public Slider rotationSlider;
    public ExecuteComfyUI executeComfyUI;
    
    [Header("Scale and Rotation")]
    public float maxScale;

    [Header("ScreenShot")]
    public GameObject uGUI;
    public RectTransform characterWindows;
    public Vector2 cropPos;
    public Vector2 cropSize;
    void Start()
    {
        DeligateButtonFunction();
    }
    
    void Update()
    {
        UpLoad();
    }
    private void DeligateButtonFunction()
    {
        for (int i = 0; i < movementButtons.Length; i++)
        {
            int index = i;
            movementButtons[index].onClick.AddListener(delegate {MoveCharacterParts((Direction)index);});
        }

        scaleSlider.onValueChanged.AddListener(delegate {ChangeScale();});
        rotationSlider.onValueChanged.AddListener(delegate {ChangeRotation();});
    }

    private void UpLoad()
    {
        if(SystemManager.instance.isDoneSEG)
        {
            string segIMGPath = executeComfyUI.GetLatestFilesInStreamingAssets(SystemManager.instance.segIMGPath);
            string segIMGName = segIMGPath.Split(Path.DirectorySeparatorChar)[^1].Split('.')[0];
            MatchEquipment(segIMGName);
            executeComfyUI.LoadImage(segIMGPath, rawImages[(int)equipment]);
            SystemManager.instance.isDoneSEG = false;
        }
    }
    private void MatchEquipment(string name)
    {
        if (name.Contains("armor"))
            equipment = Equipments.Armor;
        else if (name.Contains("pants"))
            equipment = Equipments.Pants;  
        else if (name.Contains("boots"))
            equipment = Equipments.Boots;
        else if (name.Contains("sword"))
            equipment = Equipments.Weapon;
        else if (name.Contains("gloves"))
            equipment = Equipments.Gloves;
        else if (name.Contains("shield"))
            equipment = Equipments.Shield;
    }
    #region Button Functions
    
    public void MoveCharacterParts(Direction direction)
    {
        Vector2 movement = Vector2.zero;
        
        switch(direction)
        {
            case Direction.Up:
                movement = Vector2.up;
                break;
            case Direction.Down: 
                movement = Vector2.down;
                break;
            case Direction.Left:
                movement = Vector2.left;
                break;
            case Direction.Right:
                movement = Vector2.right;
                break;
        }
        characterParts[(int)equipment].anchoredPosition += movement;
    }
    public void SetEquipment(int index)
    {
        equipment = (Equipments)index;
        var scale = characterParts[index].localScale;
        scaleSlider.value = Remap(scale.x, 0f, 1.5f, 0f, 1f);
        var rotation = characterParts[index].localRotation;
        rotationSlider.value = Remap(rotation.z, -180f, 180f, 0f, 1f);
    }
   
    public void ChangeScale()
    {
        var scale = Remap(scaleSlider.value, 0f, 1f, 0f, maxScale);
        characterParts[(int)equipment].localScale = new Vector3(scale, scale, scale);
    }
    public void ChangeRotation()
    {
        var rotation = Remap(rotationSlider.value, 0f, 1f, -180f, 180f);
        characterParts[(int)equipment].localRotation = Quaternion.Euler(0, 0, rotation);
    }
    #endregion
    private float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    public void CaptureScreen()
    {
        string filePath = Application.streamingAssetsPath + "/ScreenShots" + string.Format("/{0}.png", System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
        uGUI.SetActive(false);
        Vector2 rectPos = characterWindows.anchoredPosition;
        characterWindows.anchoredPosition = Vector2.zero;
        characterWindows.localScale = new Vector3(1.85f, 1.85f, 1.85f);
        // 스크린샷 해상도 설정 
        int width = 1024;
        int height = 1024;
        
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
        
        // RenderTexture의 내용을 Texture2D에 읽어옴 (crop 영역만)
        screenShot.ReadPixels(new Rect(cropPos.x, cropPos.y, cropSize.x, cropSize.y), 0, 0);
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
        uGUI.SetActive(true);
        characterWindows.anchoredPosition = rectPos;
        characterWindows.localScale = Vector2.one;
    }
}
