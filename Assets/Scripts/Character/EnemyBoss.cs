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
                
                // 移除实时检测阶段变化的逻辑
                // if (health >= nextPhaseHealthThreshold) ...
            }
        }
    }
    
    public void TriggerPhaseChange()
    {
        // 进入下一阶段
        phase++;

        autoManaPerTurn++;  // 每个阶段增加自动法力值

        nextPhaseHealthThreshold = GetThresholdForPhase(phase);  // 更新下一阶段的生命阈值

        EventCenter.Publish("EnemyBoss_PhaseChanged", phase);
        Debug.Log($"进入阶段 {phase}，下一阶段阈值 {nextPhaseHealthThreshold}");
    }


    private int GetThresholdForPhase(int phase)
    {
        // // 根据阶段返回不同的生命阈值
        // switch (phase)
        // {
        //     case 0: return 10;
        //     case 1: return 25;
        //     case 2: return 50;
        //     case 3: return 100;
        //     case 4: return 250;
        //     case 5: return 500;
        //     case 6: return 1000;
        //     default: return 1000 * (int)Mathf.Pow(2, phase - 6);  // 后续阶段每次翻倍
        // }


        return 50 * (int)Mathf.Pow(2, phase - 1);  // 姑且50一个阶段
    }



    // 假装思考一下, 至少1000ms
    private async Task WaitRandomSeconds(int min = 1000, int max = 3000)
    {
        int delay = Random.Range(min, max);
        float duration = delay / 1000f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (Time.timeScale > 0)
            {
                elapsed += Time.deltaTime;
            }
            await Task.Yield();
        }
    }


    private async Task AIAction()
    {
        await WaitRandomSeconds();
        
        while (true)
        {
            // 有10%概率直接结束回合, 这是负面的， 假装很智能的样子
            if (Random.value < 0.1f)
            {
                EndTurn();
                return;
            }


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

                    EventCenter.Publish("Enemy_PlayedCard", playable);

                    await WaitRandomSeconds();
                    continue;
                }
            }

            if (AllowDraw && mana > 0)
            {
                var card = DrawCard();

                EventCenter.Publish("Enemy_DrewCard", card);

                await WaitRandomSeconds();
                continue;
            }

            EndTurn();
            return;
        }
    }
}
