using UnityEngine;
using System.IO;
using System;
using TMPro;

public class Config : MonoBehaviour
{
    [Header("ComfyUI Path Settings")]
    public string comfyPath;
    public string comfyServer;
    [HideInInspector] public string outputPath;
    
    [Header("Prompt Settings")]
    
    public TMP_InputField positivePrompt;
    public TMP_InputField negativePrompt;
    [HideInInspector] public string positive;
    [HideInInspector] public string negative;
    public int seed;
    

    [HideInInspector] public string unityProjectPath;
    [HideInInspector] public string projectPath;
    [HideInInspector] public string pythonProjectPath;
    [HideInInspector] public string executeBatchPath;

    [HideInInspector] public string genIMGName;
    [HideInInspector] public string basicBatchPath;
    [HideInInspector] public string itemBatchPath;
    [HideInInspector] public string useIconBatchPath;
    private ItemPixel _itemPixel;
    
    void Start()
    {
        _itemPixel = GetComponent<ItemPixel>();
        outputPath = Application.streamingAssetsPath;
        unityProjectPath = Directory.GetParent(Application.dataPath).FullName;
        projectPath = Directory.GetParent(unityProjectPath).FullName;
        pythonProjectPath = projectPath + "/Python-GenerativeAI";

        basicBatchPath = pythonProjectPath + "/run_basic.bat";
        itemBatchPath = pythonProjectPath + "/run_item.bat";
        useIconBatchPath = pythonProjectPath + "/use_icon.bat";
    }

    
    public void SetT2IBasic()
    {
        var imageSeed = SetSeed(this.seed);
        SetPrompt();
        File.WriteAllText(basicBatchPath, "@echo off\n");
        File.AppendAllLines(basicBatchPath, new string[]{"call genAI\\Scripts\\activate.bat"});
        File.AppendAllLines(basicBatchPath, new string[]{String.Format(@"set server=""{0}""", comfyServer)});
        File.AppendAllLines(basicBatchPath, new string[]{String.Format(@"set positive=""{0}""", positive)});
        File.AppendAllLines(basicBatchPath, new string[]{String.Format(@"set negative=""{0}""", negative)});
        File.AppendAllLines(basicBatchPath, new string[]{String.Format("set seed={0}", imageSeed)});
        File.AppendAllLines(basicBatchPath, new string[]{String.Format(@"set output_path=""{0}""", outputPath)}); 
        File.AppendAllLines(basicBatchPath, new string[]{String.Format(@"set comfy_path=""{0}""", comfyPath)});
        string executePython = "python websocket_api_basicT2I.py --server %server% --positive %positive% --negative %negative% --seed %seed% --comfy_path %comfy_path% --output_path %output_path%";
        File.AppendAllText(basicBatchPath, executePython);
    }

    public void SetPixelItem()
    {
        var imageSeed = SetSeed(this.seed);
        _itemPixel.SaveControlNetImage();
        File.WriteAllText(itemBatchPath, "@echo off\n");
        File.AppendAllLines(itemBatchPath, new string[]{"call genAI\\Scripts\\activate.bat"});
        File.AppendAllLines(itemBatchPath, new string[]{String.Format(@"set server=""{0}""", comfyServer)});
        File.AppendAllLines(itemBatchPath, new string[]{String.Format("set seed={0}", imageSeed)});
        File.AppendAllLines(itemBatchPath, new string[]{String.Format(@"set output_path=""{0}""", outputPath)});
        File.AppendAllLines(itemBatchPath, new string[]{String.Format(@"set comfy_path=""{0}""", comfyPath)});
        File.AppendAllLines(itemBatchPath, new string[]{String.Format(@"set equipments=""{0}""", _itemPixel.TMP_itemLabel.text)});
        File.AppendAllLines(itemBatchPath, new string[]{String.Format(@"set controlnet_image=""{0}""", _itemPixel.controlNetIMGName)});
        string executePython = "python websocket_api_item.py --server %server% --seed %seed% --comfy_path %comfy_path% --output_path %output_path% --equipments %equipments% --controlnet_image %controlnet_image%";
        File.AppendAllText(itemBatchPath, executePython);
    }

    public void SetUseIcon()   
    {
        File.WriteAllText(useIconBatchPath, "@echo off\n");
        File.AppendAllLines(useIconBatchPath, new string[]{"call genAI\\Scripts\\activate.bat"});
        File.AppendAllLines(useIconBatchPath, new string[]{String.Format(@"set server=""{0}""", comfyServer)});
        File.AppendAllLines(useIconBatchPath, new string[]{String.Format(@"set output_path=""{0}""", outputPath)});
        File.AppendAllLines(useIconBatchPath, new string[]{String.Format(@"set comfy_path=""{0}""", comfyPath)});
        File.AppendAllLines(useIconBatchPath, new string[]{String.Format(@"set segment_image=""{0}""", genIMGName)});
        string executePython = "python websocket_api_segment.py --server %server% --comfy_path %comfy_path% --output_path %output_path% --segment_image %segment_image%";
        File.AppendAllText(useIconBatchPath, executePython);
    }
    private void SetPrompt()
    {
        positive = positivePrompt.text;
        negative = negativePrompt.text;
    }
    private int SetSeed(int seed)
    {
        if (seed == -1)
        {
            return UnityEngine.Random.Range(0, Int16.MaxValue);
        }
        return seed;
    }
}
