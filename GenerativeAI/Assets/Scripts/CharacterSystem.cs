using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
public class CharacterSystem : MonoBehaviour
{
    public enum Equipments
    {
        Helmet, Armor, Belts, Pants, Boots, Weapon, Gloves
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
        if (name.Contains("helmet"))
            equipment = Equipments.Helmet;
        else if (name.Contains("armor"))
            equipment = Equipments.Armor;
        else if (name.Contains("belt"))
            equipment = Equipments.Belts;
        else if (name.Contains("pants"))
            equipment = Equipments.Pants;  
        else if (name.Contains("boots"))
            equipment = Equipments.Boots;
        else if (name.Contains("sword"))
            equipment = Equipments.Weapon;
        else if (name.Contains("axe"))
            equipment = Equipments.Weapon;
        else if (name.Contains("gloves"))
            equipment = Equipments.Gloves;
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
        var scale = Remap(scaleSlider.value, 0f, 1f, 0f, 1.5f);
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
}
