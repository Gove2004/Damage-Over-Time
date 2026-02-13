using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanel : MonoBehaviour
{
    public GameObject introObj;
    public GameObject teamObj;
    public Button closeButton;

    private void Start()
    {
        closeButton.onClick.AddListener(ClosePanel);
        // 初始时隐藏面板，除非已经在编辑器中设置为隐藏
        gameObject.SetActive(false);
    }

    public void ShowIntro()
    {
        gameObject.SetActive(true);
        if (introObj != null) introObj.SetActive(true);
        if (teamObj != null) teamObj.SetActive(false);
    }

    public void ShowTeam()
    {
        gameObject.SetActive(true);
        if (introObj != null) introObj.SetActive(false);
        if (teamObj != null) teamObj.SetActive(true);
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}
