using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class BattleUI : MonoBehaviour
{
    // UI按钮引用
    public Button drawCardButton;
    public Button playCardButton;
    public Button endTurnButton;

    public TextMeshProUGUI turnText;

    private TextMeshProUGUI endTurnButtonText;
    private int lastAutoMana = -1;

    public CardList cardList;

    void Start()
    {
        drawCardButton.onClick.AddListener(OnDrawCardClicked);
        playCardButton.onClick.AddListener(OnPlayCardClicked);
        endTurnButton.onClick.AddListener(OnEndTurnClicked);

        endTurnButtonText = endTurnButton.GetComponentInChildren<TextMeshProUGUI>();

        EventCenter.Register("TurnStart", (param) => ShowNewTurnInfo());
        EventCenter.Register("BattleStarted", (param) => 
        {
            if (cardList != null) cardList.Clear();
        });
    }


    Sequence showTurnInfoSequence;
    private void ShowNewTurnInfo()
    {
        if (turnText != null)
        {
            if (BattleManager.Instance.IsPlayerTurn())
            {
                turnText.gameObject.SetActive(true);

                turnText.text = "你的回合";
                turnText.color = Color.green;

                if (showTurnInfoSequence != null && showTurnInfoSequence.IsActive())
                {
                    showTurnInfoSequence.Restart();
                }
                else
                {
                    turnText.transform.localScale = Vector3.one * 1.5f;
                    showTurnInfoSequence = DOTween.Sequence();
                    showTurnInfoSequence.Append(turnText.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack));

                    showTurnInfoSequence.AppendInterval(1.5f); // 显示1.5秒后淡出
                    showTurnInfoSequence.Append(turnText.DOFade(0f, 0.5f).SetEase(Ease.OutQuad).OnComplete(() =>
                    {
                        turnText.gameObject.SetActive(false);
                        turnText.color = new Color(turnText.color.r, turnText.color.g, turnText.color.b, 1f); // 重置透明度
                    }));
                }

            }
            else
            {
                turnText.text = "敌人回合";
                turnText.color = Color.red;
                turnText.gameObject.SetActive(true);

                if (showTurnInfoSequence != null && showTurnInfoSequence.IsActive())
                {
                    showTurnInfoSequence.Restart();
                }
                else
                {
                    turnText.transform.localScale = Vector3.one * 1.5f;
                    showTurnInfoSequence = DOTween.Sequence();
                    showTurnInfoSequence.Append(turnText.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack));

                    showTurnInfoSequence.AppendInterval(1.5f); // 显示1.5秒后淡出
                    showTurnInfoSequence.Append(turnText.DOFade(0f, 0.5f).SetEase(Ease.OutQuad).OnComplete(() =>
                    {
                        turnText.gameObject.SetActive(false);
                        turnText.color = new Color(turnText.color.r, turnText.color.g, turnText.color.b, 1f); // 重置透明度
                    }));
                }
            }
        }
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
        if ((Player)BattleManager.Instance?.player != null)
        {
            Player player = (Player)BattleManager.Instance.player;
            bool isPlayerTurn = player.isReady;
            drawCardButton.interactable = isPlayerTurn && player.mana >= 1 && player.Cards.Count < Player.HandLimit;
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
