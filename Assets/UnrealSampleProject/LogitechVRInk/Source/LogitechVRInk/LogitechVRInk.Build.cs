using UnrealBuildTool;
using System.IO;

public class LogitechVRInk : ModuleRules
{
    public LogitechVRInk(ReadOnlyTargetRules Target) : base(Target)
    {
        PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;

        PublicIncludePaths.Add(Path.Combine(ModuleDirectory, "Public"));
        PrivateIncludePaths.Add(Path.Combine(ModuleDirectory, "Private"));

        PrivateIncludePathModuleNames.AddRange(new string[]
        {
       });

        PrivateDependencyModuleNames.AddRange(
            new string[]
            {
                "Core", "CoreUObject", "Engine", "InputCore", "HeadMountedDisplay","SteamVR"
            }
            );

        PublicDependencyModuleNames.AddRange(
            new string[]
            {
                 "Core",
                 "OpenVRSDK",
                "OpenVR"
            }
            );
    }
}
