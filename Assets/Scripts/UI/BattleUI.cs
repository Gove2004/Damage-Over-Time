using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleUI : MonoBehaviour
{
    // UI按钮引用
    public Button drawCardButton;
    public Button playCardButton;
    public Button endTurnButton;

    private TextMeshProUGUI endTurnButtonText;
    private int lastAutoMana = -1;

    public CardList cardList;

    void Start()
    {
        drawCardButton.onClick.AddListener(OnDrawCardClicked);
        playCardButton.onClick.AddListener(OnPlayCardClicked);
        endTurnButton.onClick.AddListener(OnEndTurnClicked);

        endTurnButtonText = endTurnButton.GetComponentInChildren<TextMeshProUGUI>();
    }


    void Update()
    {
        // 根据当前回合状态更新按钮的可交互性
        IsPlayerTurn();

        // 快捷键
        if (Input.GetKeyDown(KeyCode.A))
        {
            OnDrawCardClicked();
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            OnPlayCardClicked();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            OnEndTurnClicked();
        }
    }

    private void OnDrawCardClicked()
    {
        ((Player)BattleManager.Instance.player).UI_DrawCard();
    }

    private void OnPlayCardClicked()
    {
        ((Player)BattleManager.Instance.player).UI_PlayCard(cardList.selectedCard);
    }

    private void OnEndTurnClicked()
    {
        ((Player)BattleManager.Instance.player).UI_EndTurn();
    }

    private void IsPlayerTurn()
    {
        if ((Player)BattleManager.Instance.player != null)
        {
            Player player = (Player)BattleManager.Instance.player;
            bool isPlayerTurn = player.isReady;
            drawCardButton.interactable = isPlayerTurn && player.mana >= 1 && player.Cards.Count < 7; // 限制手牌上限
            playCardButton.interactable = isPlayerTurn && cardList.AblePlay;
            endTurnButton.interactable = isPlayerTurn;

            if (player.autoManaPerTurn != lastAutoMana)
            {
                lastAutoMana = player.autoManaPerTurn;
                if (endTurnButtonText != null)
                {
                    endTurnButtonText.text = $"结束（D）\n(回复{lastAutoMana}魔力)";
                }
            }
        }
    }
}
