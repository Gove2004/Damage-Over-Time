using HybridCLR.Editor.Commands;
using HybridCLR.Editor.Installer;
using UnityEditor;
using UnityEngine;

/// <summary>
/// One-click HybridCLR install for builds (same as HybridCLR/Installer -> Install).
/// </summary>
public static class HybridCLRQuickInstall
{
    [MenuItem("HybridCLR/Generate All (Pre-Build)", priority = 64)]
    public static void GenerateAllMenu()
    {
        EditorUtility.DisplayProgressBar("HybridCLR", "Generate/All (compile + AOT + MethodBridge)...", 0.1f);
        try
        {
            PrebuildCommand.GenerateAll();
            EditorUtility.DisplayDialog("HybridCLR", "Generate/All finished. You can build now.", "OK");
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
            EditorUtility.DisplayDialog("HybridCLR", $"Generate/All failed:\n{e.Message}", "OK");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    [MenuItem("HybridCLR/Quick Install (Default)", priority = 61)]
    public static void Install()
    {
        var controller = new InstallerController();
        if (controller.HasInstalledHybridCLR())
        {
            bool reinstall = EditorUtility.DisplayDialog(
                "HybridCLR",
                "HybridCLR is already installed. Reinstall?",
                "Reinstall",
                "Cancel");
            if (!reinstall)
            {
                return;
            }
        }

        EditorUtility.DisplayProgressBar("HybridCLR", "Installing (git clone + copy il2cpp)...", 0.2f);
        try
        {
            controller.InstallDefaultHybridCLR();
            // Ensure version stamp matches package exactly (avoid BOM/newline mismatch blocking builds)
            SyncInstalledVersionStamp(controller);
            if (controller.HasInstalledHybridCLR())
            {
                EditorUtility.DisplayDialog("HybridCLR", "Install succeeded. You can build now.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("HybridCLR", "Install failed. Check Console for details.", "OK");
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    [MenuItem("HybridCLR/Sync Installed Version Stamp", priority = 63)]
    public static void SyncVersionMenu()
    {
        var controller = new InstallerController();
        if (!controller.HasInstalledHybridCLR())
        {
            EditorUtility.DisplayDialog("HybridCLR", "Not installed yet. Run Quick Install first.", "OK");
            return;
        }

        SyncInstalledVersionStamp(controller);
        EditorUtility.DisplayDialog(
            "HybridCLR",
            $"Synced.\nPackage: v{controller.PackageVersion}\nLocal: v{controller.InstalledLibil2cppVersion}",
            "OK");
    }

    private static void SyncInstalledVersionStamp(InstallerController controller)
    {
        controller.WriteLocalVersion();
        Debug.Log($"[HybridCLR] Synced installed version stamp to v{controller.PackageVersion}");
    }

    [MenuItem("HybridCLR/Check Install Status", priority = 62)]
    public static void CheckStatus()
    {
        var controller = new InstallerController();
        bool installed = controller.HasInstalledHybridCLR();
        Debug.Log($"[HybridCLR] Installed={installed}, PackageVersion={controller.PackageVersion}, LocalVersion={controller.InstalledLibil2cppVersion}");
        EditorUtility.DisplayDialog(
            "HybridCLR Status",
            installed
                ? $"Installed.\nPackage: v{controller.PackageVersion}\nLocal: v{controller.InstalledLibil2cppVersion}"
                : "Not installed. Use HybridCLR/Quick Install (Default) or HybridCLR/Installer.",
            "OK");
    }
}
