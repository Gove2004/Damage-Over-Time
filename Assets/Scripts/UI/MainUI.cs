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
        int maxScore = PlayerPrefs.GetInt("MaxScore", 0);
        maxScoreText.text = $"最高分数: {maxScore}";
    }

    public void OnStartBattleClicked()
    {
        BattleManager.Instance.StartBattle();
        gameObject.SetActive(false);
    }
}
