using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// HybridCLR / Android IL2CPP 构建需要 Unity Hub 安装「Windows Build Support (IL2CPP)」。
/// </summary>
public static class Il2CppModuleChecker
{
    private const string WindowsIl2CppVariationsRelative =
        "Editor/Data/PlaybackEngines/windowsstandalonesupport/Variations/il2cpp";

    [MenuItem("Tools/Build/Check IL2CPP Module", priority = 0)]
    public static void CheckFromMenu()
    {
        if (IsWindowsIl2CppModuleInstalled())
        {
            EditorUtility.DisplayDialog(
                "IL2CPP",
                "Windows Build Support (IL2CPP) 已安装，可以打 IL2CPP 包。",
                "OK");
        }
        else
        {
            ShowInstallDialog();
        }
    }

    [MenuItem("Tools/Build/Install Windows IL2CPP Module (Unity Hub)", priority = 1)]
    public static void OpenInstallPage()
    {
        try
        {
            System.Diagnostics.Process.Start("unityhub://");
        }
        catch
        {
            // ignore
        }

        EditorUtility.DisplayDialog(
            "IL2CPP 模块安装",
            "请在 Unity Hub 中安装模块（必须手动操作一次）：\n\n" +
            "1. 打开 Unity Hub → Installs（安装）\n" +
            "2. 找到 Unity 6000.4.4f1 → 右侧齿轮 → Add modules\n" +
            "3. 勾选「Windows Build Support (IL2CPP)」\n" +
            "4. 点 Done / Install，等待下载完成\n" +
            "5. 关闭并重新打开 Unity，再 Build Android\n\n" +
            "说明：HybridCLR 打 Android 包必须在 Windows 编辑器上安装此 IL2CPP 模块，" +
            "与 Android Build Support 是两项不同的模块。",
            "OK");
    }

    public static bool IsWindowsIl2CppModuleInstalled()
    {
        string unityRoot = EditorApplication.applicationContentsPath;
        string il2cppVariations = Path.Combine(unityRoot, WindowsIl2CppVariationsRelative);
        return Directory.Exists(il2cppVariations);
    }

    private static void ShowInstallDialog()
    {
        bool openHub = EditorUtility.DisplayDialog(
            "缺少 IL2CPP 模块",
            "当前 Unity 未安装「Windows Build Support (IL2CPP)」，无法打 Android IL2CPP 包。\n\n" +
            "请任选一种方式安装：\n" +
            "1. Unity Hub → 6000.4.4f1 → Add modules → Windows Build Support (IL2CPP)\n" +
            "2. 菜单 Tools/Build/Install Windows IL2CPP Module 下载安装包\n\n" +
            "安装后重启 Unity 再构建。",
            "打开下载页",
            "我知道了");
        if (openHub)
        {
            OpenInstallPage();
        }
    }
}

public class Il2CppModuleBuildGuard : IPreprocessBuildWithReport
{
    public int callbackOrder => -100;

    public void OnPreprocessBuild(BuildReport report)
    {
        if (Il2CppModuleChecker.IsWindowsIl2CppModuleInstalled())
        {
            return;
        }

        Il2CppModuleChecker.ShowInstallDialog();
        throw new BuildFailedException(
            "IL2CPP is not installed. Install「Windows Build Support (IL2CPP)」via Unity Hub for Editor 6000.4.4f1.");
    }
}
