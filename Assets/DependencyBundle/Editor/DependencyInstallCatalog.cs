using System.Collections.Generic;

namespace PackageStructure.Editor
{
    internal static class DependencyInstallCatalog
    {
        internal const string UnityMcpName = "com.coplaydev.unity-mcp";
        internal const string UniTaskName = "com.cysharp.unitask";
        internal const string VContainerName = "jp.hadashikick.vcontainer";
        internal const string EventBusName = "com.kameryurdakull.eventbus";
        internal const string PostProcessName = "com.kameryurdakull.post-process-service";
        internal const string DotweenDefine = "DOTWEEN";
        private const string GitHubRoot = "https://github.com/kameryurdakull/";
        private const DependencyPrerequisite TweenPrerequisites = DependencyPrerequisite.UniTask
            | DependencyPrerequisite.VContainer | DependencyPrerequisite.Dotween;

        internal const string UnityMcpUrl = "https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#30d22075093d1d35dfb0091c1c7550e9ad948577";
        internal const string UniTaskUrl = "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask#2.5.11";
        internal const string VContainerUrl = "https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer#1.19.0";
        internal const string EventBusUrl = "https://github.com/kameryurdakull/unity-eventbus.git?path=/Packages/com.kameryurdakull.eventbus#v1.0.0";

        internal const string DotweenVersion = "1.3.030";
        internal const string DotweenDownloadUrl = "https://dotween.demigiant.com/downloads/DOTween_1_3_030.zip";
        internal const string DotweenPackageName = "DOTween_1_3_030.unityPackage";
        internal const string DotweenDllPath = "Assets/Plugins/Demigiant/DOTween/DOTween.dll";
        internal const string DotweenUtilityPanelMenu = "Tools/Demigiant/DOTween Utility Panel";

        internal static IReadOnlyList<DependencyPackage> Packages { get; } =
            System.Array.AsReadOnly(new[]
            {
                new DependencyPackage("Unity MCP", UnityMcpName, UnityMcpUrl),
                new DependencyPackage("UniTask", UniTaskName, UniTaskUrl),
                new DependencyPackage("VContainer", VContainerName, VContainerUrl),
                new DependencyPackage("Event Bus", EventBusName, EventBusUrl,
                    DependencyPrerequisite.UniTask | DependencyPrerequisite.VContainer),
                new DependencyPackage("PinnedPrefab", "com.kamer.pinned-prefabs",
                    GitHubRoot + "PinnedPrefab.git#7f32eae15e22e5b1667bfacae306e773965ac931"),
                new DependencyPackage("TimeScaleToolbar", "com.kamer.time-scale-toolbar",
                    GitHubRoot + "TimeScaleToolbar.git#9cb304c7a93b68db452c317ef8f1b06b0f257d8b"),
                new DependencyPackage("URP PostProcessing Toolkit", PostProcessName,
                    GitHubRoot + "URP-PostProcessing-Toolkit.git?path=/Packages/com.kameryurdakull.post-process-service#bcdb5f30e62809c3f3265c2395bea149ca503e8b",
                    DependencyPrerequisite.VContainer | DependencyPrerequisite.Dotween),
                new DependencyPackage("Component Pool", "com.kameryurdakull.pool-service",
                    GitHubRoot + "unity-component-pool.git?path=/Packages/com.kameryurdakull.pool-service#28f4e6025338d9ea650af438cfd1d64d4f884360",
                    DependencyPrerequisite.VContainer),
                new DependencyPackage("Camera Effects", "com.kameryurdakull.camera-framework",
                    GitHubRoot + "unity-camera-effects.git?path=/Distribution/com.kameryurdakull.camera-framework#27f13341d5da6d33164f7c849ba972b83cd2f34a",
                    TweenPrerequisites),
                new DependencyPackage("State Machine", "com.kameryurdakull.state-machine",
                    GitHubRoot + "state-machine.git?path=/Packages/com.kameryurdakull.state-machine#29656416b9f79959b958ce69621cdd07bf928d05"),
                new DependencyPackage("Scene Loader", "com.kameryurdakull.sceneloader",
                    GitHubRoot + "unity-scene-loader.git?path=/Packages/com.kameryurdakull.sceneloader#3250e1dfdae220f53af441de12afc913d44d5b92",
                    DependencyPrerequisite.UniTask),
                new DependencyPackage("Audio System", "com.kameryurdakull.audiosystem",
                    GitHubRoot + "unity-audio-system.git?path=/Packages/com.kameryurdakull.audiosystem#220cd8abe4d2962a6edb834886656f86c96e10dc",
                    TweenPrerequisites | DependencyPrerequisite.EventBus),
                new DependencyPackage("Prefab Exporter", "com.kameryurdakull.prefab-exporter",
                    GitHubRoot + "unity-prefab-exporter.git?path=/Packages/com.kameryurdakull.prefab-exporter#8cbe81b0b2215db4c2ae24379ad40195e5f54d07")
            });

        internal static string GetDisplayName(string packageName)
        {
            for (var index = 0; index < Packages.Count; index++)
            {
                if (Packages[index].Name == packageName)
                {
                    return Packages[index].DisplayName;
                }
            }

            return packageName;
        }
    }
}
