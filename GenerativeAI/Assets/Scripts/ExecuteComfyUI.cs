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
    /// <summary>
    /// Execute the ComfyUI
    /// </summary>
    public void Execute()
    {  
        config.SetConfig();
        string batchFilePath = config.pythonProjectPath + "/run.bat";
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
        startInfo.FileName = batchFilePath;
        startInfo.UseShellExecute = true;
        startInfo.WorkingDirectory = config.pythonProjectPath;
        System.Diagnostics.Process.Start(startInfo);

        StartCoroutine(LoadImageCoroutine(GetLatestFilesInStreamingAssets()));
    }

    /// <summary>
    /// Load the image from the file path
    /// </summary>
    /// <param name="filePath"></param>
    private IEnumerator LoadImageCoroutine(string filePath)
    {
        string[] beforeFiles = Directory.GetFiles(Application.streamingAssetsPath);
        while(Directory.GetFiles(Application.streamingAssetsPath).Length <= beforeFiles.Length)
        {
            yield return new WaitForSeconds(0.1f);
        }
        LoadImage(filePath);
    }
    /// <summary>
    /// Load the image from the file path
    /// </summary>
    /// <param name="filePath"></param>
    private void LoadImage(string filePath)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D tex2D = new Texture2D(2, 2, TextureFormat.ARGB32, false);
        tex2D.LoadImage(fileData);
        Texture texture = tex2D;
        rawImage.texture = texture;
    }

    /// <summary>
    /// Get the latest file in the StreamingAssets folder
    /// </summary>
    /// <returns></returns>
    public string GetLatestFilesInStreamingAssets()
    {
        string streamingAssetsPath = Application.streamingAssetsPath;
        string[] files = Directory.GetFiles(streamingAssetsPath);
        System.Array.Sort(files, (a, b) => File.GetLastWriteTime(b).CompareTo(File.GetLastWriteTime(a)));
        files = files.Where(file => !file.EndsWith(".meta")).ToArray();
        return files[0];
    }
}
