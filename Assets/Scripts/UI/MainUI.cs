using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    public Button startBattleButton;
    public Button introButton;
    public Button teamButton;
    public InfoPanel infoPanel;
    public TextMeshProUGUI maxScoreText;



    public void Start()
    {
        startBattleButton.onClick.AddListener(OnStartBattleClicked);
        introButton.onClick.AddListener(OnIntroClicked);
        teamButton.onClick.AddListener(OnTeamClicked);
        
    }

    public void OnEnable()
    {
        UpdateMaxScore();
    }

    private void OnIntroClicked()
    {
        if (infoPanel != null)
        {
            infoPanel.ShowIntro();
        }
    }

    private void OnTeamClicked()
    {
        if (infoPanel != null)
        {
            infoPanel.ShowTeam();
        }
    }


    public void UpdateMaxScore()
    {
        maxScoreText.text = $"最高分数: {GameManager.Instance.maxScore}";
    }

    public void OnStartBattleClicked()
    {
        GameManager.Instance.SwitchSecne(true);
    }
}
