using UnityEngine;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour
{
    // UI按钮引用
    public Button drawCardButton;
    public Button playCardButton;
    public Button endTurnButton;

    public CardList cardList;

    void Start()
    {
        drawCardButton.onClick.AddListener(OnDrawCardClicked);
        playCardButton.onClick.AddListener(OnPlayCardClicked);
        endTurnButton.onClick.AddListener(OnEndTurnClicked);

        
    }

    private void OnDrawCardClicked()
    {
        Debug.Log("抽卡按钮被点击");
        EventCenter.Publish("UI_DrawCard");
    }

    private void OnPlayCardClicked()
    {
        Debug.Log("出牌按钮被点击");
        EventCenter.Publish("UI_PlayCard", cardList.selectedCard);
    }

    private void OnEndTurnClicked()
    {
        Debug.Log("结束回合按钮被点击");
        EventCenter.Publish("UI_EndTurn");
    }
}
