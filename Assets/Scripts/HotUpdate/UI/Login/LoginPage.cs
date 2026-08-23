

using System;
using System.Threading.Tasks;
using GoveKits.Runtime.Storage;
using TapSDK.Login;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginPage : MonoBehaviour
{
    private bool isLoggingIn = false;
    public Button startButtonUI;
    public TextMeshProUGUI loginTipTextUI;


    public void Start()
    {
        startButtonUI.onClick.AddListener(AudioManager.Instance.PlayOnClick(OnLoginButtonClicked));

        if (GameCore.LocalOfflineMode)
        {
            loginTipTextUI.text = "本地模式，点击开始游戏";
            OnLoginSuccess(new TapTapAccount());
            return;
        }

        TapTapCore.Initialize();

        _ = ChackLoginToken();
    }


    private async Task ChackLoginToken()
    {
        TapTapAccount account = await TapTapLogin.Instance.GetCurrentTapAccount();

        if (account == null)
        {
            // 用户未登录
            loginTipTextUI.text = "请先登录 TapTap 账号";
            MessageToastManager.Instance.ShowMessage("请先登录 TapTap 账号");
        }
        else
        {
            // 用户已登录
            OnLoginSuccess(account);
        }
    }


    public void OnLoginButtonClicked()
    {
        if (isLoggingIn)
        {
            // 如果已经在登录流程中，直接进入下一场景
            ResCore.LoadSceneAsync("Home");
            return;
        }

        if (GameCore.LocalOfflineMode)
        {
            OnLoginSuccess(new TapTapAccount());
            return;
        }

#if UNITY_EDITOR
        OnLoginSuccess(new TapTapAccount());
#else
        startButtonUI.interactable = false;
        _ = TapTapCore.LoginAsync(OnLoginSuccess, OnLoginCancel, OnLoginFailure);
#endif
    }

    private void OnLoginSuccess(TapTapAccount result)
    {
        loginTipTextUI.text = $"登录成功，欢迎 {result.name}！";
        MessageToastManager.Instance.ShowMessage($"登录成功，欢迎 {result.name}！");

        isLoggingIn = true;
        startButtonUI.interactable = true;

        GameCore.SetAccount(result);

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.Load();
        }
    }

    private void OnLoginFailure(Exception exception)
    {
        // 登录失败，errorCode 和 errorMsg 提供错误信息
        loginTipTextUI.text = $"登录失败，出现异常：{exception.Message}";
        MessageToastManager.Instance.ShowMessage($"登录失败，出现异常：{exception.Message}");

        startButtonUI.interactable = true; // 重新启用登录按钮，允许用户重试
    }


    private void OnLoginCancel()
    {
        // 登录被用户取消
        loginTipTextUI.text = "登录被取消";
        MessageToastManager.Instance.ShowMessage("登录被取消");

        startButtonUI.interactable = true; // 重新启用登录按钮，允许用户重试
    }
}