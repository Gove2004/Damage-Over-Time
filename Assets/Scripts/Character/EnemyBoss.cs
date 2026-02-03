using UnityEngine;

public class EnemyBoss : BaseCharacter
{
    // 这里是敌人的行动逻辑
    protected override void Action()
    {
        while (mana >= 0)  // 当有法力值时持续行动
        {
            // 选择一张可以使用的卡牌
            BaseCard cardToPlay = null;
            foreach (BaseCard card in handCards)
            {
                if (card.ManaCost <= mana)
                {
                    cardToPlay = card;
                    break;
                }
            }

            // 如果找到了可用卡牌，则使用它
            if (cardToPlay != null)
            {
                PlayCard(cardToPlay);
            }
            else
            {
                // 否则将剩下的魔力均用于抽卡
                DrawCard(mana);
                break; // 没有可用卡牌，结束行动
            }
        }

        EndTurn();
    }
}
