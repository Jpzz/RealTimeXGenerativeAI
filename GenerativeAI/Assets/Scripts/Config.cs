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

    [Header("Workflow Settings")]
    public WorkflowType workflowType;
    public enum WorkflowType { T2IBasic, PixelItem}
    
    [Header("Prompt Settings")]
    
    public TMP_InputField positivePrompt;
    public TMP_InputField negativePrompt;
    [HideInInspector] public string positive;
    [HideInInspector] public string negative;
    public int seed;
    

    [HideInInspector] public string unityProjectPath;
    [HideInInspector] public string projectPath;
    [HideInInspector] public string pythonProjectPath;

    private ItemPixel _itemPixel;
    
    void Start()
    {
        _itemPixel = GetComponent<ItemPixel>();
        outputPath = Application.streamingAssetsPath;
        unityProjectPath = Directory.GetParent(Application.dataPath).FullName;
        projectPath = Directory.GetParent(unityProjectPath).FullName;
        pythonProjectPath = projectPath + "/Python-GenerativeAI";
    }

    public void SetConfig()
    {
        var imageSeed = SetSeed(this.seed);

        string batchFilePath = pythonProjectPath + "/run.bat";
        switch (workflowType)
        {
            case WorkflowType.T2IBasic:
                SetT2IBasic(batchFilePath, imageSeed);
                break;
            case WorkflowType.PixelItem:
                SetPixelItem(batchFilePath, imageSeed);
                break;
        }
    }
    private void SetT2IBasic(string batchFilePath, int seed)
    {
        SetPrompt();
        File.WriteAllText(batchFilePath, "@echo off\n");
        File.AppendAllLines(batchFilePath, new string[]{"call genAI\\Scripts\\activate.bat"});
        File.AppendAllLines(batchFilePath, new string[]{String.Format(@"set server=""{0}""", comfyServer)});
        File.AppendAllLines(batchFilePath, new string[]{String.Format(@"set positive=""{0}""", positive)});
        File.AppendAllLines(batchFilePath, new string[]{String.Format(@"set negative=""{0}""", negative)});
        File.AppendAllLines(batchFilePath, new string[]{String.Format("set seed={0}", seed)});
        File.AppendAllLines(batchFilePath, new string[]{String.Format(@"set output_path=""{0}""", outputPath)});
        File.AppendAllLines(batchFilePath, new string[]{String.Format(@"set comfy_path=""{0}""", comfyPath)});
        string executePython = "python websocket_api_basicT2I.py --server %server% --positive %positive% --negative %negative% --seed %seed% --comfy_path %comfy_path% --output_path %output_path%";
        File.AppendAllText(batchFilePath, executePython);
    }

    private void SetPixelItem(string batchFilePath, int seed)
    {
        _itemPixel.SaveControlNetImage();
        File.WriteAllText(batchFilePath, "@echo off\n");
        File.AppendAllLines(batchFilePath, new string[]{"call genAI\\Scripts\\activate.bat"});
        File.AppendAllLines(batchFilePath, new string[]{String.Format(@"set server=""{0}""", comfyServer)});
        File.AppendAllLines(batchFilePath, new string[]{String.Format("set seed={0}", seed)});
        File.AppendAllLines(batchFilePath, new string[]{String.Format(@"set output_path=""{0}""", outputPath)});
        File.AppendAllLines(batchFilePath, new string[]{String.Format(@"set comfy_path=""{0}""", comfyPath)});
        File.AppendAllLines(batchFilePath, new string[]{String.Format(@"set equipments=""{0}""", _itemPixel.TMP_itemLabel.text)});
        File.AppendAllLines(batchFilePath, new string[]{String.Format(@"set controlnet_image=""{0}""", _itemPixel.controlNetImageName)});
        string executePython = "python websocket_api_item.py --server %server% --seed %seed% --comfy_path %comfy_path% --output_path %output_path% --equipments %equipments% --controlnet_image %controlnet_image%";
        File.AppendAllText(batchFilePath, executePython);
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
