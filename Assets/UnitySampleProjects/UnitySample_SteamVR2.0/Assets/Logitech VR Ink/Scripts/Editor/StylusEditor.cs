using System;
using UnityEngine;
using UnityEditor;
using System.IO;

public class StylusEditor : Editor
{
    /// <summary>
    /// Copies the default SteamVR action binding files for the Toolkit.
    /// </summary>
    [MenuItem("Window/Copy Default SteamVR Actions for the VR Ink")]
    static void CopyBindings()
    {
        int copyOption = 1;

        string rootPath = Application.dataPath.Replace("Assets", "");
        string guid = AssetDatabase.FindAssets("SteamVRBindings")[0];
        string defaultBindingsPath = rootPath + AssetDatabase.GUIDToAssetPath(guid);
        Debug.Log("Default action bindings location: " + defaultBindingsPath);

        string[] sourceFilePaths = Directory.GetFiles(defaultBindingsPath, "*.json");
        foreach (string sourceFilePath in sourceFilePaths)
        {
            string sourceFileName = Path.GetFileName(sourceFilePath);
            string destinationFilePath = rootPath + sourceFileName;

            try
            {
                File.Copy(sourceFilePath, destinationFilePath, false);
                Debug.Log("Copied " + sourceFilePath + " to root: " + rootPath);
            }
            catch (IOException)
            {
                if (copyOption != 2)
                {
                    copyOption = EditorUtility.DisplayDialogComplex("Could not copy binding file " + sourceFileName,
                        destinationFilePath + "\nalready exists.", "Replace", "Skip", "Replace All");
                }

                if (copyOption == 0 || copyOption == 2)
                {
                    File.Copy(sourceFilePath, destinationFilePath, true);
                    Debug.Log("Copied " + sourceFilePath + " to root: " + rootPath);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Could not copy " + sourceFilePath + " to root: " + rootPath + ". " + e);
            }
        }
    }

#if !STEAMVR_ENABLED
    /// <summary>
    /// This class allows the Toolkit to know when an asset is removed or added to the project.
    /// </summary>
    class MyAllPostprocessor : AssetPostprocessor
    {
        // Suggest to add a script define if the project doesn't detect SteamVR Assemblies.
        private const string SteamVRAssetName = "SteamVR";
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string str in importedAssets)
            {
                if (str.Contains(SteamVRAssetName))
                {
                    Debug.Log("Did you just import the SteamVR plugin? If so, please add 'STEAMVR_ENABLED' to Player Settings > Other Settings > Scripting Definition Symbols.");
                    break;
                }
            }
        }
    }
#endif
}
