using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// Unity Hub may mark "Android SDK &amp; NDK Tools" as Installed while sub-modules
/// like platform-tools / cmdline-tools were never extracted into the Editor SDK folder.
/// </summary>
[InitializeOnLoad]
public static class AndroidSdkPathBootstrap
{
    private const string PrefKeyConfigured = "NinthSlime.AndroidSdkBootstrap.ConfiguredFor";
    private const string PrefKeyWarned = "NinthSlime.AndroidSdkBootstrap.WarnedIncomplete";

    private const string PlatformToolsUrl =
        "https://dl.google.com/android/repository/platform-tools_r36.0.0-win.zip";

    private const string CmdlineToolsUrl =
        "https://dl.google.com/android/repository/commandlinetools-win-12266719_latest.zip";

    static AndroidSdkPathBootstrap()
    {
        EditorApplication.delayCall += ConfigureIfNeeded;
    }

    [MenuItem("Tools/Android/Repair Missing SDK Components", priority = 99)]
    public static void RepairFromMenu()
    {
        if (!ResolveUnityAndroidPaths(out string sdk, out string ndk, out string jdk))
        {
            EditorUtility.DisplayDialog("Android Tools", "未找到 Unity AndroidPlayer SDK 目录。", "OK");
            return;
        }

        bool ok = RepairMissingSdkComponents(sdk, showDialogs: true);
        if (ok)
        {
            TryConfigureAndroidTools();
            EditorUtility.DisplayDialog(
                "Android Tools",
                "已补装缺失的 platform-tools / cmdline-tools，并写入 External Tools 路径。\n\n请重新 Build Android。",
                "OK");
        }
    }

    [MenuItem("Tools/Android/Configure SDK Paths From Unity Hub", priority = 100)]
    public static void ConfigureFromMenu()
    {
        var result = TryConfigureAndroidTools();
        switch (result)
        {
            case ConfigureResult.Success:
                EditorUtility.DisplayDialog(
                    "Android Tools",
                    "SDK / NDK / JDK 路径已配置。\n\n可在 Edit > Preferences > External Tools 查看。",
                    "OK");
                break;
            case ConfigureResult.IncompleteSdk:
                EditorUtility.DisplayDialog(
                    "Android Tools",
                    "SDK 仍不完整（缺少 platform-tools）。\n\n" +
                    "Hub 显示 Installed 不代表子组件都装好了。\n" +
                    "请先点 Tools > Android > Repair Missing SDK Components。",
                    "OK");
                break;
            default:
                EditorUtility.DisplayDialog(
                    "Android Tools",
                    "未找到 AndroidPlayer SDK。请先通过 Unity Hub 安装 Android Build Support。",
                    "OK");
                break;
        }
    }

    [MenuItem("Tools/Android/Show Detected SDK Paths", priority = 101)]
    public static void ShowDetectedPaths()
    {
        ResolveUnityAndroidPaths(out string sdk, out string ndk, out string jdk);
        bool sdkOk = IsValidAndroidSdk(sdk);
        EditorUtility.DisplayDialog(
            "Detected Android Paths",
            $"SDK: {sdk}\n  valid={sdkOk}\n  platform-tools={Directory.Exists(Path.Combine(sdk ?? string.Empty, "platform-tools"))}\n  cmdline-tools={Directory.Exists(Path.Combine(sdk ?? string.Empty, "cmdline-tools"))}\n\n" +
            $"NDK: {ndk}\n  exists={Directory.Exists(ndk)}\n\n" +
            $"JDK: {jdk}\n  exists={Directory.Exists(jdk)}",
            "OK");
    }

    private static void ConfigureIfNeeded()
    {
        string editorVersion = Application.unityVersion;
        if (EditorPrefs.GetString(PrefKeyConfigured, "") == editorVersion)
        {
            return;
        }

        if (!ResolveUnityAndroidPaths(out string sdk, out _, out _))
        {
            return;
        }

        if (!IsValidAndroidSdk(sdk))
        {
            // Auto-repair once per editor version if possible.
            if (RepairMissingSdkComponents(sdk, showDialogs: false))
            {
                Debug.Log("[AndroidSdkPathBootstrap] Auto-repaired missing Android SDK components.");
            }
        }

        var result = TryConfigureAndroidTools();
        if (result == ConfigureResult.Success)
        {
            EditorPrefs.SetString(PrefKeyConfigured, editorVersion);
            return;
        }

        if (result == ConfigureResult.IncompleteSdk &&
            EditorPrefs.GetString(PrefKeyWarned, "") != editorVersion)
        {
            EditorPrefs.SetString(PrefKeyWarned, editorVersion);
            Debug.LogWarning(
                "[AndroidSdkPathBootstrap] Unity SDK 缺少 platform-tools。\n" +
                "Hub 可能显示 Installed，但子模块未解压。请用 Tools/Android/Repair Missing SDK Components。");
        }
    }

    private enum ConfigureResult
    {
        Success,
        MissingSdk,
        IncompleteSdk,
        Failed
    }

    private static ConfigureResult TryConfigureAndroidTools()
    {
        if (!ResolveUnityAndroidPaths(out string sdk, out string ndk, out string jdk))
        {
            return ConfigureResult.MissingSdk;
        }

        if (!IsValidAndroidSdk(sdk))
        {
            return ConfigureResult.IncompleteSdk;
        }

        EditorPrefs.SetString("AndroidSdkRoot", sdk);
        if (Directory.Exists(ndk))
        {
            EditorPrefs.SetString("AndroidNdkRoot", ndk);
            EditorPrefs.SetString("AndroidNdkRootR16b", ndk);
            EditorPrefs.SetString("AndroidNdkRootR19", ndk);
            EditorPrefs.SetString("AndroidNdkRootR21", ndk);
        }

        if (Directory.Exists(jdk))
        {
            EditorPrefs.SetString("JdkPath", jdk);
            EditorPrefs.SetString("JdkPathRuntimes", jdk);
        }

        TrySetAndroidExternalToolsSettings(sdk, ndk, jdk);
        Debug.Log($"[AndroidSdkPathBootstrap] Configured Android tools.\nSDK={sdk}\nNDK={ndk}\nJDK={jdk}");
        return ConfigureResult.Success;
    }

    private static bool ResolveUnityAndroidPaths(out string sdk, out string ndk, out string jdk)
    {
        sdk = ndk = jdk = null;

        // Prefer repaired user-writable SDK (Hub install can miss platform-tools under Program Files).
        string userRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Unity");
        string userSdk = Path.Combine(userRoot, "android-sdk", Application.unityVersion);
        string userNdk = Path.Combine(userRoot, "android-ndk", Application.unityVersion);
        string userJdk = Path.Combine(userRoot, "android-jdk", Application.unityVersion);
        if (IsValidAndroidSdk(userSdk))
        {
            sdk = userSdk;
            ndk = Directory.Exists(userNdk) ? userNdk : null;
            jdk = Directory.Exists(userJdk) ? userJdk : null;
            return true;
        }

        string editorDir = Path.GetDirectoryName(EditorApplication.applicationPath);
        if (string.IsNullOrEmpty(editorDir))
        {
            return false;
        }

        string androidPlayer = Path.Combine(editorDir, "Data", "PlaybackEngines", "AndroidPlayer");
        sdk = Path.Combine(androidPlayer, "SDK");
        ndk = Path.Combine(androidPlayer, "NDK");
        jdk = Path.Combine(androidPlayer, "OpenJDK");
        return Directory.Exists(sdk);
    }

    private static bool IsValidAndroidSdk(string sdkRoot)
    {
        if (string.IsNullOrEmpty(sdkRoot) || !Directory.Exists(sdkRoot))
        {
            return false;
        }

        return File.Exists(Path.Combine(sdkRoot, "platform-tools", "adb.exe"));
    }

    private static bool RepairMissingSdkComponents(string sdkRoot, bool showDialogs)
    {
        bool changed = false;
        try
        {
            // If Unity's Program Files SDK is incomplete, build a repaired copy under LocalAppData.
            if (!sdkRoot.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), StringComparison.OrdinalIgnoreCase))
            {
                string userSdk = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Unity",
                    "android-sdk",
                    Application.unityVersion);
                if (!IsValidAndroidSdk(userSdk))
                {
                    if (showDialogs)
                    {
                        EditorUtility.DisplayProgressBar("Android SDK Repair", "Copying SDK to user profile...", 0.1f);
                    }

                    CopyDirectory(sdkRoot, userSdk);
                    sdkRoot = userSdk;
                    changed = true;
                }
                else
                {
                    sdkRoot = userSdk;
                }
            }

            if (showDialogs)
            {
                EditorUtility.DisplayProgressBar("Android SDK Repair", "Checking SDK components...", 0.05f);
            }

            string platformToolsDir = Path.Combine(sdkRoot, "platform-tools");
            if (!File.Exists(Path.Combine(platformToolsDir, "adb.exe")))
            {
                if (showDialogs)
                {
                    EditorUtility.DisplayProgressBar("Android SDK Repair", "Downloading platform-tools...", 0.2f);
                }

                DownloadAndExtractZip(PlatformToolsUrl, sdkRoot);
                changed = true;
                Debug.Log("[AndroidSdkPathBootstrap] Installed platform-tools.");
            }

            string cmdlineFinal = Path.Combine(sdkRoot, "cmdline-tools", "16.0", "bin", "sdkmanager.bat");
            if (!File.Exists(cmdlineFinal))
            {
                if (showDialogs)
                {
                    EditorUtility.DisplayProgressBar("Android SDK Repair", "Downloading cmdline-tools...", 0.55f);
                }

                string tempRoot = Path.Combine(Path.GetTempPath(), "unity-android-cmdline-" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(tempRoot);
                try
                {
                    DownloadAndExtractZip(CmdlineToolsUrl, tempRoot);
                    string extracted = Path.Combine(tempRoot, "cmdline-tools");
                    if (!Directory.Exists(extracted))
                    {
                        throw new Exception("cmdline-tools zip did not contain cmdline-tools folder.");
                    }

                    string destRoot = Path.Combine(sdkRoot, "cmdline-tools");
                    Directory.CreateDirectory(destRoot);
                    string dest16 = Path.Combine(destRoot, "16.0");
                    if (Directory.Exists(dest16))
                    {
                        Directory.Delete(dest16, true);
                    }

                    Directory.Move(extracted, dest16);
                    changed = true;
                    Debug.Log("[AndroidSdkPathBootstrap] Installed cmdline-tools/16.0.");
                }
                finally
                {
                    if (Directory.Exists(tempRoot))
                    {
                        Directory.Delete(tempRoot, true);
                    }
                }
            }

            AcceptAndroidLicenses(sdkRoot);
            return changed || IsValidAndroidSdk(sdkRoot);
        }
        catch (Exception e)
        {
            Debug.LogError("[AndroidSdkPathBootstrap] Repair failed: " + e.Message);
            if (showDialogs)
            {
                EditorUtility.DisplayDialog("Android Tools", "修复失败：\n" + e.Message, "OK");
            }

            return false;
        }
        finally
        {
            if (showDialogs)
            {
                EditorUtility.ClearProgressBar();
            }
        }
    }

    private static void CopyDirectory(string sourceDir, string destDir)
    {
        if (!Directory.Exists(destDir))
        {
            Directory.CreateDirectory(destDir);
        }

        foreach (string file in Directory.GetFiles(sourceDir))
        {
            string destFile = Path.Combine(destDir, Path.GetFileName(file));
            File.Copy(file, destFile, true);
        }

        foreach (string dir in Directory.GetDirectories(sourceDir))
        {
            CopyDirectory(dir, Path.Combine(destDir, Path.GetFileName(dir)));
        }
    }

    private static void DownloadAndExtractZip(string url, string destinationRoot)
    {
        string tempZip = Path.Combine(Path.GetTempPath(), "unity-android-" + Guid.NewGuid().ToString("N") + ".zip");
        try
        {
            using (var client = new WebClient())
            {
                client.DownloadFile(url, tempZip);
            }

            ZipFile.ExtractToDirectory(tempZip, destinationRoot);
        }
        finally
        {
            if (File.Exists(tempZip))
            {
                File.Delete(tempZip);
            }
        }
    }

    private static void AcceptAndroidLicenses(string sdkRoot)
    {
        string sdkmanager = Path.Combine(sdkRoot, "cmdline-tools", "16.0", "bin", "sdkmanager.bat");
        if (!File.Exists(sdkmanager))
        {
            return;
        }

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{sdkmanager}\" --sdk_root=\"{sdkRoot}\" --licenses",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var process = Process.Start(psi))
            {
                if (process == null)
                {
                    return;
                }

                // Accept all license prompts.
                process.StandardInput.WriteLine("y");
                process.StandardInput.WriteLine("y");
                process.StandardInput.WriteLine("y");
                process.StandardInput.WriteLine("y");
                process.StandardInput.WriteLine("y");
                process.StandardInput.Close();
                process.WaitForExit(120000);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("[AndroidSdkPathBootstrap] License acceptance skipped: " + e.Message);
        }
    }

    private static void TrySetAndroidExternalToolsSettings(string sdk, string ndk, string jdk)
    {
        try
        {
            var settingsType = Type.GetType(
                "UnityEditor.Android.AndroidExternalToolsSettings, UnityEditor.Android.Extensions");
            if (settingsType == null)
            {
                return;
            }

            TrySetMember(settingsType, "sdkRootPath", sdk);
            if (Directory.Exists(ndk))
            {
                TrySetMember(settingsType, "ndkRootPath", ndk);
            }

            if (Directory.Exists(jdk))
            {
                TrySetMember(settingsType, "jdkRootPath", jdk);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(
                "[AndroidSdkPathBootstrap] AndroidExternalToolsSettings skipped: " +
                (e.InnerException != null ? e.InnerException.Message : e.Message));
        }
    }

    private static void TrySetMember(Type type, string name, string value)
    {
        try
        {
            var prop = type.GetProperty(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(null, value, null);
                return;
            }

            var field = type.GetField(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            field?.SetValue(null, value);
        }
        catch (Exception e)
        {
            string msg = e.InnerException != null ? e.InnerException.Message : e.Message;
            Debug.LogWarning($"[AndroidSdkPathBootstrap] Failed to set {name}: {msg}");
        }
    }
}
