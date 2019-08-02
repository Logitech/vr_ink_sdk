using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using Logitech.XRToolkit.Components;

public class Builder : Editor
{
    private const string INPUT_MANAGER_CONTENT =
@"  - serializedVersion: 3
    m_Name: left grip
    descriptiveName:
    descriptiveNegativeName:
    negativeButton:
    positiveButton: escape
    altNegativeButton:
    altPositiveButton: joystick button 1
    gravity: 1000
    dead: 0.001
    sensitivity: 1000
    snap: 0
    invert: 0
    type: 2
    axis: 10
    joyNum: 0
  - serializedVersion: 3
    m_Name: right grip
    descriptiveName:
    descriptiveNegativeName:
    negativeButton:
    positiveButton: escape
    altNegativeButton:
    altPositiveButton: joystick button 1
    gravity: 1000
    dead: 0.001
    sensitivity: 1000
    snap: 0
    invert: 0
    type: 2
    axis: 11
    joyNum: 0
  - serializedVersion: 3
    m_Name: 9th axis
    descriptiveName:
    descriptiveNegativeName:
    negativeButton:
    positiveButton: escape
    altNegativeButton:
    altPositiveButton: joystick button 1
    gravity: 1
    dead: 0.01
    sensitivity: 2
    snap: 0
    invert: 0
    type: 2
    axis: 8
    joyNum: 0
  - serializedVersion: 3
    m_Name: 10th axis
    descriptiveName:
    descriptiveNegativeName:
    negativeButton:
    positiveButton: escape
    altNegativeButton:
    altPositiveButton: joystick button 1
    gravity: 1
    dead: 0.01
    sensitivity: 2
    snap: 0
    invert: 0
    type: 2
    axis: 9
    joyNum: 0
  - serializedVersion: 3
    m_Name: Y Axis
    descriptiveName:
    descriptiveNegativeName:
    negativeButton:
    positiveButton: escape
    altNegativeButton:
    altPositiveButton: joystick button 1
    gravity: 1
    dead: 0.01
    sensitivity: 2
    snap: 0
    invert: 0
    type: 2
    axis: 1
    joyNum: 0
  - serializedVersion: 3
    m_Name: 5th axis
    descriptiveName:
    descriptiveNegativeName:
    negativeButton:
    positiveButton:
    altNegativeButton:
    altPositiveButton:
    gravity: 1
    dead: 0.01
    sensitivity: 2
    snap: 0
    invert: 0
    type: 2
    axis: 4
    joyNum: 0
";

    [MenuItem("XR Toolkit/Import Recommended Settings")]
    static void EnsureSettings()
    {
        // Set names.
        PlayerSettings.companyName = "Logitech";
        PlayerSettings.productName = "Labs XR Toolkit";

        // Enable VR.
        PlayerSettings.SetVirtualRealitySupported(BuildTargetGroup.Standalone, true);
        PlayerSettings.SetVirtualRealitySDKs(BuildTargetGroup.Standalone, new string[] { "OpenVR" });

        // Disable splash screens, resolutions dialog and fullscreen.
        PlayerSettings.SplashScreen.showUnityLogo = false;
        PlayerSettings.defaultIsFullScreen = false;
        PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Disabled;

        Debug.Log("Settings imported.");
    }

    [MenuItem("XR Toolkit/Import Axes Settings")]
    static void EnsureInputAxes()
    {
        string inputManagerPath = Application.dataPath + "/../ProjectSettings/InputManager.asset";
        var inputManagerContent = File.ReadAllText(inputManagerPath, System.Text.Encoding.UTF8);

        // look for two of the values; if they aren't there the other probably aren't either
        if (!inputManagerContent.Contains("10th axis")
            && !inputManagerContent.Contains("5th axis"))
        {
            // modify input manager to have access to the axes we need for the toolkit Unity abstraction
            File.AppendAllText(inputManagerPath, INPUT_MANAGER_CONTENT);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            Debug.Log("Axes added to your Input Manager.");
        }
        else
        {
            Debug.LogWarning("It looks like you already have the right axes set up.");
        }
    }

    [MenuItem("XR Toolkit/Build Only")]
    static void BuildApp()
    {
        string[] scenes = { "./Assets/labs_xr_toolkit/Toolkit/Example/Scenes/All_In_One.unity" };
        string destination = "Builds/Toolkit/Logitech_LabsXR_Toolkit.exe";

        Debug.Log("Building " + destination);

        BuildPipeline.BuildPlayer(scenes,
            destination,
            BuildTarget.StandaloneWindows64,
            BuildOptions.None);
    }

    [MenuItem("XR Toolkit/Package Only")]
    static void PackageAssets()
    {
        string source = "Assets/labs_xr_toolkit/Toolkit";
        string destination = Directory.GetCurrentDirectory() + "/Builds/Logitech_LabsXR_Toolkit.unitypackage";

        Debug.Log("Exporting asset package in " + destination);

        AssetDatabase.ExportPackage(
            source,
            destination,
            ExportPackageOptions.Recurse);
    }

    /// <summary>
    /// Copies the default SteamVR action binding files for the Toolkit.
    /// </summary>
    [MenuItem("XR Toolkit/Copy Default SteamVR Actions for the Toolkit", false, 10)]
    static void CopyFiles()
    {
        int copyOption = 1;

        string rootPath = Application.dataPath.Replace("Assets", "");
        string guid = AssetDatabase.FindAssets("SteamVRBindings")[0];
        string defaultBindingsPath = rootPath + AssetDatabase.GUIDToAssetPath(guid);
        Debug.Log("[Logitech Toolkit] Default action bindings location: " + defaultBindingsPath);

        string[] sourceFilePaths = Directory.GetFiles(defaultBindingsPath, "*.json");
        foreach (string sourceFilePath in sourceFilePaths)
        {
            string sourceFileName = Path.GetFileName(sourceFilePath);
            string destinationFilePath = rootPath + sourceFileName;

            try
            {
                File.Copy(sourceFilePath, destinationFilePath, false);
                Debug.Log("[Logitech Toolkit] Copied " + sourceFilePath + " to root: " + rootPath);
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
                    Debug.Log("[Logitech Toolkit] Copied " + sourceFilePath + " to root: " + rootPath);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[Logitech Toolkit] Could not copy " + sourceFilePath + " to root: " + rootPath + ". " + e);
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
