using System.Collections;
using System.Threading.Tasks;
using UnityEngine;


public class EnemyBoss : BaseCharacter
{
    // 这里是敌人的行动逻辑
    protected override void Action() => _ = AIAction();

    public static bool AllowPlay = true;
    public static bool AllowDraw = true;

    public EnemyBoss()
    {
        // 初始化敌人属性
        health = 0;
        mana = 3;
        autoManaPerTurn = 3;
    }


    public int phase = 0;  // 当前阶段
    public int nextPhaseHealthThreshold = 10;  // 下一阶段的生命阈值
    public override void ChangeHealth(int amount)
    {
        // 如果是治疗效果，转为护盾
        if (amount > 0)
        {
            shiled += amount;
        }
        else
        {
            // 先扣护盾
            int damageToShield = Mathf.Min(shiled, -amount);
            shiled -= damageToShield;
            amount += damageToShield; // 减去被护盾吸收的伤害

            // 如果还有剩余伤害，扣血
            if (amount < 0)
            {
                health -= amount;  // Boss的生命是得分

                if (health >= nextPhaseHealthThreshold)
                {
                    // 进入下一阶段
                    phase++;

                    autoManaPerTurn++;  // 每个阶段增加自动法力值

                    nextPhaseHealthThreshold = GetThresholdForPhase(phase);  // 更新下一阶段的生命阈值
                }
            }
        }
    }


    private int GetThresholdForPhase(int phase)
    {
        // 根据阶段返回不同的生命阈值
        switch (phase)
        {
            case 0: return 10;
            case 1: return 50;
            case 2: return 100;
            case 3: return 250;
            case 4: return 500;
            case 5: return 1000;
            case 6: return 2000;
            case 7: return 5000;
            case 8: return 10000;
            default: return 10000 * (int)Mathf.Pow(2, phase - 8);  // 后续阶段每次翻倍
        }
    }


    private async Task AIAction()
    {
        await Task.Delay(1000); // 等待1秒钟再进行下一次行动
        while (true)
        {
            if (AllowPlay)
            {
                BaseCard playable = null;
                foreach (var card in Cards)
                {
                    if (card.Cost <= mana)
                    {
                        playable = card;
                        break;
                    }
                }
                if (playable != null)
                {
                    PlayCard(playable);
                    await Task.Delay(1000);
                    continue;
                }
            }

            if (AllowDraw && mana > 0)
            {
                DrawCard();
                await Task.Delay(1000);
                continue;
            }

            EndTurn();
            return;
        }
    }
}
