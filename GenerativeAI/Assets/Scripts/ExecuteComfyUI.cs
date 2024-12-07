using UnityEngine;
using System.IO;
using System.Linq;
using UnityEngine.UI;
using System.Collections;

public class ExecuteComfyUI : MonoBehaviour
{
    [Header("Variables")]
    public Config config;
    public RawImage rawImage;

    private int genIMGCount;
    private int segIMGCount;
 
    void Start()
    {
    
        if(!Directory.Exists(SystemManager.instance.genIMGPath))
            Directory.CreateDirectory(SystemManager.instance.genIMGPath);
        if(!Directory.Exists(SystemManager.instance.segIMGPath))
            Directory.CreateDirectory(SystemManager.instance.segIMGPath);

        genIMGCount = Directory.GetFiles(SystemManager.instance.genIMGPath).Length;
        segIMGCount = Directory.GetFiles(SystemManager.instance.segIMGPath).Length;
    }
    /// <summary>
    /// Execute the ComfyUI
    /// </summary>
    public void ExecuteT2IBasic()
    {  
        config.SetT2IBasic();
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
        startInfo.FileName = config.basicBatchPath;
        startInfo.UseShellExecute = true;
        startInfo.WorkingDirectory = config.pythonProjectPath;
        System.Diagnostics.Process.Start(startInfo);
        StartCoroutine(LoadImageCoroutine());
    }

    public void ExecutePixelItem()
    {  
        config.SetPixelItem();
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
        startInfo.FileName = config.itemBatchPath;
        startInfo.UseShellExecute = true;
        startInfo.WorkingDirectory = config.pythonProjectPath;
        System.Diagnostics.Process.Start(startInfo);
        StartCoroutine(LoadImageCoroutine());
    }

    public void ExecuteUseIcon()
    {  
        config.SetUseIcon();
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
        startInfo.FileName = config.useIconBatchPath;
        startInfo.UseShellExecute = true;
        startInfo.WorkingDirectory = config.pythonProjectPath;
        System.Diagnostics.Process.Start(startInfo);
        StartCoroutine(MonitorSEGFolder());
    }

    /// <summary>
    /// Load the image from the file path
    /// </summary>
    /// <param name="filePath"></param>
    private IEnumerator LoadImageCoroutine()
    {
        Debug.Log("WAITING for IMG GENERATION...");
        while(Directory.GetFiles(SystemManager.instance.genIMGPath).Length <= genIMGCount)
        {
            yield return new WaitForSeconds(0.1f);
        }
        Debug.Log("IMG GENERATION DONE");
        string imgPath = GetLatestFilesInStreamingAssets(SystemManager.instance.genIMGPath);
        config.genIMGName = imgPath.Split(Path.DirectorySeparatorChar)[^1];
        Debug.Log(config.genIMGName);
        LoadImage(imgPath, rawImage);
        genIMGCount = Directory.GetFiles(SystemManager.instance.genIMGPath).Length;
    }
    /// <summary>
    /// Load the image from the file path
    /// </summary>
    /// <param name="imgPath"></param>
    public void LoadImage(string imgPath, RawImage rawImage)
    {
        byte[] fileData = File.ReadAllBytes(imgPath);
        Texture2D tex2D = new Texture2D(2, 2, TextureFormat.ARGB32, false);
        tex2D.LoadImage(fileData);
        Texture texture = tex2D;
        rawImage.texture = texture;
    }

    private IEnumerator MonitorSEGFolder()
    {
        while(Directory.GetFiles(SystemManager.instance.segIMGPath).Length <= segIMGCount)
        {
            yield return new WaitForSeconds(0.1f);
        }
        Debug.Log("SEG DONE");
        SystemManager.instance.isDoneSEG = true;
    }
    /// <summary>
    /// Get the latest file in the StreamingAssets folder
    /// </summary>
    /// <returns></returns>
    public string GetLatestFilesInStreamingAssets(string path)
    {
        string[] files = Directory.GetFiles(path);
        System.Array.Sort(files, (a, b) => File.GetLastWriteTime(b).CompareTo(File.GetLastWriteTime(a)));
        files = files.Where(file => !file.EndsWith(".meta")).ToArray();
        return Path.GetFullPath(files[0]);
    }
}
