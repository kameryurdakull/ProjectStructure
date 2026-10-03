using UnityEditor;
using UnityEngine;

namespace PackageStructure.Editor
{
    public sealed class DependencyInstallerWindow : EditorWindow
    {
        private const string MenuPath = "Tools/Package Structure/Dependency Installer";
        private const string WindowTitle = "Dependencies";
        private const string InstallLabel = "Install";
        private const string InstalledLabel = "Installed";
        private const string NotInstalledLabel = "Not installed";
        private const float InstallButtonWidth = 90f;
        private Vector2 _scrollPosition;

        [MenuItem(MenuPath)]
        private static void Open()
        {
            var window = GetWindow<DependencyInstallerWindow>(WindowTitle);
            window.minSize = new Vector2(480f, 360f);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Online dependency installer", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Install dependencies with their individual buttons. Git packages use pinned revisions; DOTween is downloaded from Demigiant. Required dependencies are shown below each package.", MessageType.Info);

            using (var scroll = new EditorGUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scroll.scrollPosition;
                DrawDotweenRow();

                for (var index = 0; index < DependencyInstallCatalog.Packages.Count; index++)
                {
                    DrawPackageRow(DependencyInstallCatalog.Packages[index]);
                }
            }

            if (DependencyInstallService.IsBusy)
            {
                EditorGUILayout.HelpBox("Installation is running. Check the Console for the result.", MessageType.None);
            }

        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }

        private static void DrawPackageRow(DependencyPackage package)
        {
            var installed = DependencyInstallService.IsPackageInstalled(package.Name);
            var blockReason = installed ? null : DependencyInstallService.GetInstallationBlockReason(package);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(package.DisplayName, installed ? InstalledLabel : NotInstalledLabel);

                using (new EditorGUI.DisabledScope(installed || blockReason != null || DependencyInstallService.IsBusy))
                {
                    if (GUILayout.Button(InstallLabel, GUILayout.Width(InstallButtonWidth)))
                    {
                        DependencyInstallService.InstallPackage(package);
                    }
                }
            }

            if (blockReason != null)
            {
                EditorGUILayout.HelpBox(blockReason, MessageType.Warning);
            }
        }

        private static void DrawDotweenRow()
        {
            var installed = DependencyInstallService.IsDotweenInstalled();

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("DOTween", installed ? InstalledLabel : NotInstalledLabel);

                using (new EditorGUI.DisabledScope(installed || DependencyInstallService.IsBusy))
                {
                    if (GUILayout.Button(InstallLabel, GUILayout.Width(InstallButtonWidth)))
                    {
                        DependencyInstallService.InstallDotween();
                    }
                }
            }

            if (installed)
            {
                if (GUILayout.Button("Open DOTween Setup"))
                {
                    EditorApplication.ExecuteMenuItem(DependencyInstallCatalog.DotweenUtilityPanelMenu);
                }

                EditorGUILayout.HelpBox("Click 'Setup DOTween...' in the Utility Panel after a new import.", MessageType.None);
            }
        }

    }
}
