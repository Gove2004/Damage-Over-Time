

public class Player : BaseCharacter
{

    public bool isReady { get; set; } = false;

    protected override void Action()
    {
        isReady = true;
    }

    void Start()
    {
        EventCenter.Register("UI_PlayCard", (card) => PlayCard(card as BaseCard));
        EventCenter.Register("UI_DrawCard", (_) => UI_DrawCard());
        EventCenter.Register("UI_EndTurn", (_) => UI_EndTurn());
    }


    // UI：调用使用卡牌
    public void UI_PlayCard(BaseCard card)
    {
        if (isReady && handCards.Contains(card) && card.ManaCost <= mana)
        {
            PlayCard(card);
        }
    }


    // UI：调用抽卡
    public void UI_DrawCard()
    {
        if (isReady)
        {
            DrawCard();
        }
    }

    // UI：调用结束回合
    public void UI_EndTurn()
    {
        if (isReady)
        {
            isReady = false;
            EndTurn();
        }
    }



    
}