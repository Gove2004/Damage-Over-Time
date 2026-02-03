

public class Player : BaseCharacter
{

    public bool isReady { get; set; } = false;

    protected override void Action()
    {
        isReady = true;
    }


    // UI：调用使用卡牌
    public void PlayerPlayCard(BaseCard card)
    {
        if (isReady && handCards.Contains(card) && card.ManaCost <= mana)
        {
            PlayCard(card);
        }
    }


    // UI：调用抽卡
    public void PlayerDrawCard(int nums = 1)
    {
        if (isReady)
        {
            DrawCard(nums);
        }
    }

    // UI：调用结束回合
    public void PlayerEndTurn()
    {
        if (isReady)
        {
            isReady = false;
            EndTurn();
        }
    }



    
}