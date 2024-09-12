using UnityEditor;
using UnityEngine;

public class AutoSaveSettings : ScriptableObject
{
    public float saveInterval = 300f;
    public string savePath = "AutoSaves";
    public int maxSaves = 5;

    private static AutoSaveSettings instance;
    public static AutoSaveSettings Instance
    {
        get
        {
            if (instance == null)
            {
                instance = CreateInstance<AutoSaveSettings>();
                instance.LoadSettings();
            }
            return instance;
        }
    }

    public void SaveSettings()
    {
        EditorPrefs.SetFloat("AutoSave_SaveInterval", saveInterval);
        EditorPrefs.SetString("AutoSave_SavePath", savePath);
        EditorPrefs.SetInt("AutoSave_MaxSaves", maxSaves);
    }

    public void LoadSettings()
    {
        if (EditorPrefs.HasKey("AutoSave_SaveInterval"))
        {
            saveInterval = EditorPrefs.GetFloat("AutoSave_SaveInterval");
            savePath = EditorPrefs.GetString("AutoSave_SavePath");
            maxSaves = EditorPrefs.GetInt("AutoSave_MaxSaves");
        }
    }
}
