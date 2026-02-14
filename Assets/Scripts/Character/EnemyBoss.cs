using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


public class EnemyBoss : BaseCharacter
{
    private CancellationTokenSource _cts;

    public override void OnBattleEnd()
    {
        Stop();
    }

    public void Stop()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    // 这里是敌人的行动逻辑
    protected override void Action()
    {
        Stop(); // Cancel previous task if any
        _cts = new CancellationTokenSource();
        _ = AIAction(_cts.Token);
    }

    public static bool AllowPlay = true;
    public static bool AllowDraw = true;

    public EnemyBoss()
    {
        // 初始化敌人属性
        health = 0;
        mana = 1;
        autoManaPerTurn = 1;  // 初始Boss比较弱
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
        if (phase <= 1) return 10;
        if (phase == 2) return 25;
        if (phase == 3) return 50;
        if (phase == 4) return 100;
        if (phase <= 13) return 100 * (phase - 3);

        int n = phase - 13;
        return 1000 + 100 * (n * (n + 3) / 2);
    }



    // 假装思考一下, 至少1000ms
    private async Task WaitRandomSeconds(CancellationToken token, int min = 1000, int max = 3000)
    {
        int delay = Random.Range(min, max);
        float duration = delay / 1000f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (token.IsCancellationRequested) return;

            if (Time.timeScale > 0)
            {
                elapsed += Time.deltaTime;
            }
            await Task.Yield();
        }
    }


    private async Task AIAction(CancellationToken token)
    {
        await WaitRandomSeconds(token);
        
        while (!token.IsCancellationRequested)
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

                    await WaitRandomSeconds(token);
                    continue;
                }
            }

            if (AllowDraw && mana > 0)
            {
                var card = DrawCard();

                EventCenter.Publish("Enemy_DrewCard", card);

                await WaitRandomSeconds(token);
                continue;
            }

            EndTurn();
            return;
        }
    }
}
