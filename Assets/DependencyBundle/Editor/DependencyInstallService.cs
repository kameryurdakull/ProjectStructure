using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Networking;

namespace PackageStructure.Editor
{
    internal static class DependencyInstallService
    {
        private static AddRequest _packageRequest;
        private static UnityWebRequest _dotweenRequest;
        private static string _dotweenDownloadPath;

        internal static bool IsBusy => _packageRequest != null || _dotweenRequest != null;

        internal static bool IsPackageInstalled(string packageName)
        {
            var packages = UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages();

            for (var index = 0; index < packages.Length; index++)
            {
                if (packages[index].name == packageName)
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool IsDotweenInstalled()
        {
            return File.Exists(DependencyInstallCatalog.DotweenDllPath);
        }

        internal static string GetInstallationBlockReason(DependencyPackage package)
        {
            var missing = new List<string>();
            AddMissingPrerequisite(package, DependencyPrerequisite.UniTask, DependencyInstallCatalog.UniTaskName, missing);
            AddMissingPrerequisite(package, DependencyPrerequisite.VContainer, DependencyInstallCatalog.VContainerName, missing);
            AddMissingPrerequisite(package, DependencyPrerequisite.EventBus, DependencyInstallCatalog.EventBusName, missing);

            if ((package.Prerequisites & DependencyPrerequisite.Dotween) != 0 && !IsDotweenReady())
            {
                missing.Add("DOTween (complete Setup DOTween for the active build target)");
            }

            return missing.Count == 0 ? null : $"Install first: {string.Join(", ", missing)}.";
        }

        private static bool IsDotweenReady()
        {
            if (!IsDotweenInstalled())
            {
                return false;
            }

            var targetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            var target = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(targetGroup);
            var defines = PlayerSettings.GetScriptingDefineSymbols(target).Split(';');
            return Array.IndexOf(defines, DependencyInstallCatalog.DotweenDefine) >= 0;
        }

        private static void AddMissingPrerequisite(DependencyPackage package, DependencyPrerequisite prerequisite,
            string packageName, List<string> missing)
        {
            if ((package.Prerequisites & prerequisite) != 0 && !IsPackageInstalled(packageName))
            {
                missing.Add(DependencyInstallCatalog.GetDisplayName(packageName));
            }
        }

        internal static void InstallPackage(DependencyPackage package)
        {
            if (IsBusy || IsPackageInstalled(package.Name))
            {
                return;
            }

            var blockReason = GetInstallationBlockReason(package);
            if (blockReason != null)
            {
                Debug.LogWarning(blockReason);
                return;
            }

            _packageRequest = Client.Add(package.Url);
            EditorApplication.update += PollPackageRequest;
            Debug.Log($"Installing {package.Name} from its official Git repository.");
        }

        internal static void InstallDotween()
        {
            if (IsBusy || IsDotweenInstalled())
            {
                return;
            }

            _dotweenDownloadPath = Path.Combine(Path.GetTempPath(), $"DOTween_{Guid.NewGuid():N}.zip");
            _dotweenRequest = UnityWebRequest.Get(DependencyInstallCatalog.DotweenDownloadUrl);
            _dotweenRequest.downloadHandler = new DownloadHandlerFile(_dotweenDownloadPath);
            var operation = _dotweenRequest.SendWebRequest();
            operation.completed += OnDotweenDownloaded;
            Debug.Log($"Downloading DOTween {DependencyInstallCatalog.DotweenVersion} from Demigiant.");
        }

        private static void PollPackageRequest()
        {
            if (_packageRequest == null || !_packageRequest.IsCompleted)
            {
                return;
            }

            EditorApplication.update -= PollPackageRequest;

            if (_packageRequest.Status == StatusCode.Success)
            {
                Debug.Log($"Installed {_packageRequest.Result.name} {_packageRequest.Result.version}.");
            }
            else
            {
                Debug.LogError($"Package installation failed: {_packageRequest.Error?.message}");
            }

            _packageRequest = null;
        }

        private static void OnDotweenDownloaded(AsyncOperation operation)
        {
            try
            {
                if (_dotweenRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"DOTween download failed: {_dotweenRequest.error}");
                    return;
                }

                using (var archive = ZipFile.OpenRead(_dotweenDownloadPath))
                {
                    var entry = archive.GetEntry(DependencyInstallCatalog.DotweenPackageName);

                    if (entry == null)
                    {
                        Debug.LogError("The official DOTween download does not contain the expected Unity package.");
                        return;
                    }

                    var packagePath = Path.Combine(Path.GetDirectoryName(_dotweenDownloadPath),
                        $"DOTween_{Guid.NewGuid():N}.unitypackage");
                    entry.ExtractToFile(packagePath);
                    AssetDatabase.ImportPackage(packagePath, false);
                }

                Debug.Log("DOTween imported. Complete 'Setup DOTween...' in Tools > Demigiant > DOTween Utility Panel.");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                _dotweenRequest.Dispose();
                _dotweenRequest = null;
                _dotweenDownloadPath = null;
            }
        }

    }
}
