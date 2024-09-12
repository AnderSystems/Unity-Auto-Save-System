using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class AutoSave
{
    static float saveInterval;
    static string savePath;
    static int maxSaves;
    static double lastSaveTime;

    static AutoSave()
    {
        LoadSettings();
        EditorApplication.update += Update;
        EditorApplication.delayCall += CheckForAutoSave;
    }

    static void LoadSettings()
    {
        var settings = AutoSaveSettings.Instance;
        saveInterval = settings.saveInterval;
        savePath = settings.savePath;
        maxSaves = settings.maxSaves;
        lastSaveTime = EditorApplication.timeSinceStartup;
    }

    static void Update()
    {
        if (EditorApplication.timeSinceStartup - lastSaveTime >= saveInterval)
        {
            SaveScene();
            lastSaveTime = EditorApplication.timeSinceStartup;
        }
    }

    public static void SaveScene()
    {
        if (!EditorSceneManager.GetActiveScene().isDirty)
            return;

        string scenePath = EditorSceneManager.GetActiveScene().path;
        string sceneName = Path.GetFileNameWithoutExtension(scenePath);
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string saveFileName = $"{sceneName}_{timestamp}.unity";
        string fullPath = Path.Combine(savePath, saveFileName);

        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        // Salvar a cena como uma cópia sem abrir a nova cena salva
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), fullPath, true);
        Debug.Log($"AutoSave: Scene saved to <a href=\"file://{savePath}\">{fullPath}</a>");

        // Limitar o número de saves
        var files = Directory.GetFiles(savePath, $"{sceneName}_*.unity")
                             .OrderByDescending(f => File.GetCreationTime(f))
                             .Skip(maxSaves)
                             .ToList();

        foreach (var file in files)
        {
            File.Delete(file);
            Debug.Log($"AutoSave: Deleted old save {file}");
        }
    }


    public static void CheckForAutoSave()
    {
        string scenePath = EditorSceneManager.GetActiveScene().path;
        string sceneName = Path.GetFileNameWithoutExtension(scenePath);
        var autoSaveFiles = Directory.GetFiles(savePath, $"{sceneName}_*.unity")
                                     .OrderByDescending(f => File.GetCreationTime(f))
                                     .ToList();

        if (autoSaveFiles.Count > 0)
        {
            string latestAutoSave = autoSaveFiles.First();
            if (File.GetLastWriteTime(latestAutoSave) > File.GetLastWriteTime(scenePath))
            {
                if (EditorUtility.DisplayDialog("AutoSave Detected",
                    "A more recent auto-save file was found. Would you like to load it?",
                    "Yes", "No"))
                {
                    RestoreBackup(latestAutoSave);
                }
            }
        }
    }


    // Criar um caminho temporário dentro da pasta Assets
    public static void RestoreBackup(string backupFilePath)
    {
        if (File.Exists(backupFilePath))
        {
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            string tempFileName = "tmp.unity";
            string tempFilePath = Path.Combine("Assets", "Temp", tempFileName);

            // Certificar-se de que a pasta Temp existe
            if (!Directory.Exists(Path.Combine("Assets", "Temp")))
            {
                Directory.CreateDirectory(Path.Combine("Assets", "Temp"));
            }

            string scenePath = EditorSceneManager.GetActiveScene().path;
            File.Copy(backupFilePath, tempFilePath, true);
            var BackupScene = EditorSceneManager.OpenScene(tempFilePath, OpenSceneMode.Additive);

            // Copiar o conteúdo da cena de backup para a nova cena
            foreach (var rootGameObject in BackupScene.GetRootGameObjects())
            {
                SceneManager.MoveGameObjectToScene(rootGameObject, newScene);
            }

            EditorSceneManager.CloseScene(BackupScene, true);
            EditorSceneManager.MarkSceneDirty(newScene);

            Debug.Log($"AutoSave: Scene restored from {backupFilePath}");
        }
        else
        {
            Debug.LogError($"AutoSave: Backup file not found at {backupFilePath}");
        }
    }

}
