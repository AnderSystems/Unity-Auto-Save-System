using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AutoSaveSettings))]
public class AutoSaveSettingsEditor : Editor
{
    [SettingsProvider]
    public static SettingsProvider CreateSettingsProvider()
    {
        var provider = new SettingsProvider("Project/AutoSaveSettings", SettingsScope.Project)
        {
            label = "Auto Save Settings",
            guiHandler = (searchContext) =>
            {
                var settings = AutoSaveSettings.Instance;
                settings.saveInterval = EditorGUILayout.FloatField("Save Interval (seconds)", settings.saveInterval);

                EditorGUILayout.BeginHorizontal();
                settings.savePath = EditorGUILayout.TextField("Save Path", settings.savePath);
                if (GUILayout.Button("...", GUILayout.Width(30)))
                {
                    string selectedPath = EditorUtility.OpenFolderPanel("Select Save Path", "", "");
                    if (!string.IsNullOrEmpty(selectedPath))
                    {
                        settings.savePath = selectedPath;
                    }
                }
                EditorGUILayout.EndHorizontal();

                settings.maxSaves = EditorGUILayout.IntField("Max Saves", settings.maxSaves);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Backup Now"))
                {
                    if (EditorUtility.DisplayCancelableProgressBar("AutoSave", "Backing up...", 0.0f))
                        return;
                    AutoSave.SaveScene();
                    EditorUtility.ClearProgressBar();
                }


                //Botão pra restaurar backup
                if (GUILayout.Button("Restore from Backup", GUILayout.Width(150)) && Directory.Exists(settings.savePath))
                {
                    AutoSave.CheckForAutoSave();
                }

                EditorGUILayout.EndHorizontal();

                GUILayout.FlexibleSpace();

                // Preview do tamanho dos backups
                long totalSize = GetBackupSize(settings.savePath);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Total Backup Size: " + FormatBytes(totalSize));

                // Botão para limpar todos os backups
                if (GUILayout.Button("Clear All Backups"))
                {
                    if (EditorUtility.DisplayDialog("Clear All Backups", "Are you sure you want to delete all backups?", "Yes", "No"))
                    {
                        ClearAllBackups(settings.savePath);
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorUtility.SetDirty(settings);


                if (GUI.changed)
                {
                    settings.SaveSettings();
                }
            }
        };
        return provider;
    }

    private static long GetBackupSize(string path)
    {
        if (!Directory.Exists(path))
            return 0;

        return Directory.GetFiles(path, "*.unity", SearchOption.AllDirectories)
                        .Sum(file => new FileInfo(file).Length);
    }

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        while (bytes >= 1024 && order < sizes.Length - 1)
        {
            order++;
            bytes = bytes / 1024;
        }
        return $"{bytes:0.##} {sizes[order]}";
    }

    private static void ClearAllBackups(string path)
    {
        if (Directory.Exists(path))
        {
            var files = Directory.GetFiles(path, "*.unity", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                File.Delete(file);
            }
            Debug.Log("All backups have been cleared.");
        }
    }
}
