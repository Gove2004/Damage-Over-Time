using System;
using System.Threading.Tasks;
using GoveKits.Runtime;
using GoveKits.Runtime.Core;
using GoveKits.Runtime.Storage;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;
using HybridCLR;

public class Boot : MonoSingleton<Boot>
{
    private enum BootState
    {
        None,
        Initializing,
        UpdatingPackage,
        LoadingHotUpdate,
        LoadingGameServices,
        Failed
    }

    #region 生命周期

    protected override void Awake()
    {
        base.Awake();

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        const string packageName = "DefaultPackage";

        try
        {
            SetState(BootState.Initializing);

            YooAssets.Initialize(new YooLogger());
            InitializeCoreServices();

            SetState(BootState.UpdatingPackage);
            TipText = "正在加载本地资源...";
            Progress = 0f;

            bool ok = await ResCore.PackageWorkflowAsync(
                new AutoOfflinePackageConfig(packageName),
                CreateUpdateCallbacks());

            if (!ok)
            {
                throw new Exception("本地资源包初始化失败，请先在 Unity 中执行 YooAsset 模拟构建/打包。");
            }

            await ContinueStartupAsync();
        }
        catch (Exception e)
        {
            HandleStartupFailure("初始化失败", e);
        }
    }

    private UpdateCallbacks CreateUpdateCallbacks()
    {
        return new UpdateCallbacks
        {
            OnCheckVersionBegin = OnCheckVersionBegin,
            OnCheckVersionSuccess = OnCheckVersionSuccess,
            OnCheckVersionFailed = OnCheckVersionFailed,
            OnUpdateManifestBegin = OnUpdateManifestBegin,
            OnUpdateManifestSuccess = OnUpdateManifestSuccess,
            OnUpdateManifestFailed = OnUpdateManifestFailed,
            OnDownloadBegin = OnDownloadBegin,
            OnDownloadFileBegin = OnDownloadFileBegin,
            OnDownloadUpdate = OnDownloadUpdate,
            OnDownloadError = OnDownloadError,
            OnDownloadFinish = OnDownloadFinish
        };
    }

    private void InitializeCoreServices()
    {
        LogCore.InfuseLogger(new UnityLogger());
        RandomCore.Initialize(new NormalRNG(Environment.TickCount));
        TimeCore.Initialize(16, 128);
        TimeCore.RigisterWheel(TimeCore.NormalWheelName, 0.05f, 512);
        TimeCore.RigisterWheel(TimeCore.UnscaledWheelName, 0.05f, 512);
    }

    private async Task ContinueStartupAsync()
    {
        if (startupContinuationStarted || state == BootState.Failed)
        {
            return;
        }

        startupContinuationStarted = true;

        try
        {
            SetState(BootState.LoadingHotUpdate);
            TipText = "正在加载热更新...";
            Progress = 0.9f;
            await LoadHotUpdateAsync();

            SetState(BootState.LoadingGameServices);
            TipText = "正在初始化游戏服务...";
            Progress = 0.95f;
            LoadAsset();

            TipText = "正在进入游戏...";
            var sceneHandle = ResCore.LoadSceneAsync("Login");
            if (sceneHandle == null)
            {
                throw new Exception("无法加载 Login 场景：资源包未就绪。");
            }

            while (!sceneHandle.IsDone)
            {
                await Task.Yield();
            }

            if (sceneHandle.Status != EOperationStatus.Succeed)
            {
                throw new Exception($"Login 场景加载失败：{sceneHandle.LastError}");
            }

            Progress = 1f;
        }
        catch (Exception e)
        {
            HandleStartupFailure("加载失败", e);
        }
    }

    private void Update()
    {
        TimeCore.Update(TimeCore.NormalWheelName, Time.deltaTime);
        TimeCore.Update(TimeCore.UnscaledWheelName, Time.unscaledDeltaTime);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    private void OnApplicationQuit()
    {
        YooAssets.Destroy();
    }

    #endregion

    #region 热更新

    public void OnCheckVersionBegin()
    {
        TipText = "正在加载本地资源...";
        Progress = 0f;
    }

    public void OnCheckVersionSuccess(string version) => TipText = $"本地资源就绪，版本: {version}";

    public void OnCheckVersionFailed(string error)
    {
        TipText = $"本地资源检查失败：{error}";
        Debug.LogWarning($"[Boot] Local package version check failed: {error}");
    }

    public void OnUpdateManifestBegin() => TipText = "正在加载资源清单...";

    public void OnUpdateManifestSuccess() => TipText = "资源清单就绪。";

    public void OnUpdateManifestFailed(string error)
    {
        TipText = $"资源清单加载失败：{error}";
        Debug.LogWarning($"[Boot] Local manifest load failed: {error}");
    }

    public void OnDownloadBegin(int totalCount, long totalBytes) => TipText = $"开始下载，文件数量: {totalCount}, 总大小: {totalBytes / 1024 / 1024:F2} MB.";

    public void OnDownloadFileBegin(DownloadFileData data) => Progress = 0f;

    public void OnDownloadUpdate(DownloadUpdateData data) => Progress = data.Progress;

    public void OnDownloadError(DownloadErrorData data) => TipText = $"下载错误，文件: {data.FileName}, 错误信息: {data.ErrorInfo}.";

    public void OnDownloadFinish(DownloaderFinishData data)
    {
        TipText = data.Succeed ? "本地资源准备完成..." : "资源准备未完全成功，继续启动...";
        Progress = 1f;
    }

    public async Task LoadHotUpdateAsync()
    {
#if !UNITY_EDITOR
        if (!await HotfixCore.LoadAotMetadataAsync(AOTGenericReferences.PatchedAOTAssemblyList))
        {
            throw new Exception("AOT 元数据加载失败。");
        }
#endif

        var assembly = await HotfixCore.LoadHotfixAssemblyAsync("HotUpdate.dll");
        if (assembly == null)
        {
            throw new Exception("热更程序集 HotUpdate 加载失败。");
        }
    }

    public void LoadAsset()
    {
        ConfigCore.InfuseParser(new JsonConfigParser());
        ConfigCore.InfuseParser(new CsvConfigParser());
        ConfigCore.Initialize();
        AudioCore.Initialize(16);
        SaveCore.Initialize(new JsonSerializer());
    }

    #endregion

    #region 字段

    private string tipText = "资源加载中...";
    public string TipText
    {
        get => tipText;
        set
        {
            tipText = value;
            if (tipTextUI != null)
            {
                tipTextUI.text = tipText;
            }
        }
    }

    private float progress;
    public float Progress
    {
        get => progress;
        set
        {
            progress = Mathf.Clamp01(value);
            if (progressSliderUI != null)
            {
                progressSliderUI.value = progress;
            }

            if (progressTextUI != null)
            {
                progressTextUI.text = $"{(int)(progress * 100)}%";
            }
        }
    }

    private bool startupContinuationStarted;
    private BootState state = BootState.None;

    #endregion

    #region UI

    public GameObject loadingPanelUI;
    public TextMeshProUGUI tipTextUI;
    public TextMeshProUGUI progressTextUI;
    public Slider progressSliderUI;

    private void SetState(BootState nextState)
    {
        state = nextState;
    }

    private void HandleStartupFailure(string message, Exception exception)
    {
        state = BootState.Failed;
        Progress = 1f;
        TipText = $"{message}：{exception.Message}";
        Debug.LogException(exception);
        LogCore.Error(nameof(Boot), $"{message}: {exception}");
    }

    #endregion
}
