

public class Player : BaseCharacter
{
    public Player()
    {
        // 初始化玩家属性
        health = 100;
        mana = 0;
        autoManaPerTurn = 2;
    }


    public bool isReady { get; set; } = false;

    protected override void Action()
    {
        isReady = true;
    }


    public override void ChangeHealth(int amount)
    {
        {
        if (amount >= 0)
        {
            health += amount;
        }
        else
        {
            // 先扣护盾
            int damageToShield = UnityEngine.Mathf.Min(shiled, -amount);
            shiled -= damageToShield;
            amount += damageToShield; // 减去被护盾吸收的伤害

            // 如果还有剩余伤害，扣血
            if (amount < 0)
            {
                health += amount;  // amount是负数，所以是减法
            }
        }
        if (health <= 0)
        {
            health = 0;
            
            EventCenter.Publish("PlayerDead", this);
        }
    }
    }





    // UI：调用使用卡牌
    public void UI_PlayCard(BaseCard card)
    {
        if (isReady && Cards.Contains(card) && card.ManaCost <= mana)
        {
            PlayCard(card);

            EventCenter.Publish("Player_PlayCard", card);
        }
    }


    // UI：调用抽卡
    public void UI_DrawCard()
    {
        if (isReady)
        {
            var card = DrawCard();

            EventCenter.Publish("Player_DrawCard", card);
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