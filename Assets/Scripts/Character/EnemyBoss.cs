using System.Collections;
using System.Threading.Tasks;


public class EnemyBoss : BaseCharacter
{
    // 这里是敌人的行动逻辑
    protected override void Action() => _ = AIAction();


    private async Task AIAction()
    {
        await Task.Delay(1000); // 等待1秒钟再进行下一次行动

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
                DrawCard();
            }

            await Task.Delay(1000); // 等待1秒钟再进行下一次行动
        }

        EndTurn();
    }
}
