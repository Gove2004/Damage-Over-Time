using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    public Button startBattleButton;
    public TextMeshProUGUI maxScoreText;



    public void Start()
    {
        startBattleButton.onClick.AddListener(OnStartBattleClicked);
        UpdateMaxScore();
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
