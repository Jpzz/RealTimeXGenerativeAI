using UnityEngine;

public class SystemManager : MonoBehaviour
{
    public GameObject characterSystem;
    public GameObject pixelSystem;
    public static SystemManager instance;
    [HideInInspector] public string genIMGPath;
    [HideInInspector] public string segIMGPath;
    [HideInInspector] public bool isDoneSEG;
    private string todayFolderName;
    private void Awake()
    {
        if(instance == null)
            instance = this;
        todayFolderName = System.DateTime.Now.ToString("yyyy-MM-dd");
        todayFolderName = todayFolderName.Replace("-", "");
        genIMGPath = Application.streamingAssetsPath + "/GEN/" + todayFolderName;
        segIMGPath = Application.streamingAssetsPath + "/SEG/" + todayFolderName;
    }
    public void NextSystem()
    {
        pixelSystem.SetActive(false);
        characterSystem.SetActive(true);
    }

    public void PreviousSystem()
    {
        characterSystem.SetActive(false);
        pixelSystem.SetActive(true);
    }
}
