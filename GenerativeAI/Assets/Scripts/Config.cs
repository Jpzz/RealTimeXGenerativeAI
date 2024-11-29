using UnityEngine;
using System.IO;
using System;
using TMPro;

public class Config : MonoBehaviour
{
    [Header("ComfyUI Path Settings")]
    public string comfyPath;
    public string comfyServer;
    public string outputPath;


    [Header("Prompt Settings")]
    
    public TMP_InputField positivePrompt;
    public TMP_InputField negativePrompt;
    [HideInInspector] public string positive;
    [HideInInspector] public string negative;
    public int seed;

    [HideInInspector] public string unityProjectPath;
    [HideInInspector] public string projectPath;
    [HideInInspector] public string pythonProjectPath;
    
    void Start()
    {
        unityProjectPath = Directory.GetParent(Application.dataPath).FullName;
        projectPath = Directory.GetParent(unityProjectPath).FullName;
        pythonProjectPath = projectPath + "/Python-GenerativeAI";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetConfig()
    {
        SetPrompt();
        var imageSeed = SetSeed(this.seed);
        string batchFilePath = pythonProjectPath + "/run.bat";
        File.WriteAllText(batchFilePath, "@echo off\n");
        File.AppendAllLines(batchFilePath, new string[]{"call genAI\\Scripts\\activate.bat"});
        File.AppendAllLines(batchFilePath, new string[]{String.Format(@"set server=""{0}""", comfyServer)});
        File.AppendAllLines(batchFilePath, new string[]{String.Format(@"set positive=""{0}""", positive)});
        File.AppendAllLines(batchFilePath, new string[]{String.Format(@"set negative=""{0}""", negative)});
        File.AppendAllLines(batchFilePath, new string[]{String.Format("set seed={0}", imageSeed)});
        File.AppendAllLines(batchFilePath, new string[]{String.Format(@"set output_path=""{0}""", outputPath)});
        File.AppendAllLines(batchFilePath, new string[]{String.Format(@"set comfy_path=""{0}""", comfyPath)});

        string executePython = "python websocket_api.py --server %server% --positive %positive% --negative %negative% --seed %seed% --comfy_path %comfy_path% --output_path %output_path%";
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
