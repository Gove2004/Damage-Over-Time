using System;
using UnityEditor;
using UnityEngine;
using YooAsset;
using YooAsset.Editor;

/// <summary>
/// 一键完成 YooAsset 本地模式资源构建（无需手动点 Collector/Builder 窗口）。
/// </summary>
public static class YooAssetQuickBuild
{
    public const string PackageName = "DefaultPackage";

    [MenuItem("YooAsset/Quick Setup/本地模式 - 模拟构建 (编辑器 Play)", false, 200)]
    public static void SimulateBuildMenu()
    {
        try
        {
            RunSimulateBuild();
            EditorUtility.DisplayDialog("YooAsset", "模拟构建完成，可以直接点 Play 测试。", "OK");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            EditorUtility.DisplayDialog("YooAsset", $"模拟构建失败：\n{e.Message}", "OK");
        }
    }

    [MenuItem("YooAsset/Quick Setup/本地模式 - 打内置包 (StreamingAssets)", false, 201)]
    public static void BuildBuiltinMenu()
    {
        try
        {
            var output = RunBuiltinBuild(EditorUserBuildSettings.activeBuildTarget);
            EditorUtility.DisplayDialog("YooAsset", $"内置包构建完成。\n输出：{output}", "OK");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            EditorUtility.DisplayDialog("YooAsset", $"内置包构建失败：\n{e.Message}", "OK");
        }
    }

    [MenuItem("YooAsset/Quick Setup/本地模式 - 一键完成全部", false, 202)]
    public static void BuildAllMenu()
    {
        try
        {
            RunSimulateBuild();
            var output = RunBuiltinBuild(EditorUserBuildSettings.activeBuildTarget);
            EditorUtility.DisplayDialog("YooAsset", $"本地模式资源已全部构建完成。\n内置包输出：{output}", "OK");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            EditorUtility.DisplayDialog("YooAsset", $"构建失败：\n{e.Message}", "OK");
        }
    }

    /// <summary>
    /// Unity 批处理入口：-executeMethod YooAssetQuickBuild.BatchBuildLocalMode
    /// </summary>
    public static void BatchBuildLocalMode()
    {
        RunSimulateBuild();
        var output = RunBuiltinBuild(EditorUserBuildSettings.activeBuildTarget);
        Debug.Log($"[YooAssetQuickBuild] Local mode build completed. Output: {output}");
    }

    private static void RunSimulateBuild()
    {
        var result = EditorSimulateModeHelper.SimulateBuild(PackageName);
        if (string.IsNullOrEmpty(result.PackageRootDirectory))
        {
            throw new Exception("EditorSimulateBuildPipeline 未返回 PackageRootDirectory。");
        }

        Debug.Log($"[YooAssetQuickBuild] Simulate build OK: {result.PackageRootDirectory}");
    }

    private static string RunBuiltinBuild(BuildTarget buildTarget)
    {
        var buildParameters = new BuiltinBuildParameters();
        buildParameters.BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
        buildParameters.BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
        buildParameters.BuildPipeline = EBuildPipeline.BuiltinBuildPipeline.ToString();
        buildParameters.BuildBundleType = (int)EBuildBundleType.AssetBundle;
        buildParameters.BuildTarget = buildTarget;
        buildParameters.PackageName = PackageName;
        buildParameters.PackageVersion = DateTime.Now.ToString("yyyy-MM-dd-HH-mm");
        buildParameters.EnableSharePackRule = true;
        buildParameters.VerifyBuildingResult = true;
        buildParameters.FileNameStyle = EFileNameStyle.HashName;
        buildParameters.BuildinFileCopyOption = EBuildinFileCopyOption.ClearAndCopyAll;
        buildParameters.BuildinFileCopyParams = string.Empty;
        buildParameters.CompressOption = ECompressOption.LZ4;
        buildParameters.ClearBuildCacheFiles = true;
        buildParameters.UseAssetDependencyDB = true;

        var pipeline = new BuiltinBuildPipeline();
        var buildResult = pipeline.Run(buildParameters, true);
        if (!buildResult.Success)
        {
            throw new Exception(buildResult.ErrorInfo ?? "BuiltinBuildPipeline build failed.");
        }

        Debug.Log($"[YooAssetQuickBuild] Builtin build OK: {buildResult.OutputPackageDirectory}");
        return buildResult.OutputPackageDirectory;
    }
}
