using System;
using System.Text;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    // 简单的单例模式
    public static BattleManager Instance;
    public bool IsGMMode = false;
    void Awake()
    {
        Instance = this;


        if (GetComponent<GMTool>() == null && IsGMMode)
        {
            gameObject.AddComponent<GMTool>();
        }
    }


    public BaseCharacter player;
    public BaseCharacter enemy;
    private int currentTurn = 1;
    public bool IsPlayerTurn()
    {
        return currentTurn % 2 == 1;
    }
    private Action onPhaseChangedUnsub;


    public void StartBattle()
    {
        Debug.Log("战斗开始！");
        currentTurn = 0;

        // 清理旧的事件监听
        onPhaseChangedUnsub?.Invoke();

        player = new Player();
        enemy = new EnemyBoss();
        BaseCard.ResetOverclock();

        player.Target = enemy;
        enemy.Target = player;

        player.StartTurn();

        player.ChangeHealth(0);  // 触发UI更新
        enemy.ChangeHealth(0);  // 触发UI更新

        EventCenter.Publish("BattleStarted");

        // 注册阶段变化监听
        onPhaseChangedUnsub = EventCenter.Register("EnemyBoss_PhaseChanged", (param) =>
        {
            if (player != null)
            {
                player.autoManaPerTurn++;
                Debug.Log($"阶段提升，玩家每回合自动回蓝增加至: {player.autoManaPerTurn}");
            }
        });

        EventCenter.Register("PlayerDead", (param) =>
        {
            var character = param as BaseCharacter;
            EndBattle();
        });
        EventCenter.Register("CharacterEndedTurn", (param) =>
        {
            NextTurn();
        });

        NextTurn();
    }



    public void EndBattle()
    {
        Debug.Log("战斗结束。");

        EventCenter.Publish("BattleEnded");


        // 结算分数
        int score = enemy.health;
        GameManager.Instance.Save(score);

        // 清理战斗数据
        player = null;
        enemy = null;

        // UI跳转
        GameManager.Instance.SwitchSecne(false);
    }


    public void NextTurn()
    {
        currentTurn++;

        Debug.Log($"第{currentTurn}回合开始");

        // 轮流行动
        if (currentTurn % 2 == 1)
        {
            player.StartTurn();
        }
        else
        {
            enemy.StartTurn();
        }
    }


    // 注释掉临时代码

    // void Start()
    // {
    //     StartBattle();
    // }

    // void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.Space))
    //     {
    //         StartBattle();
    //     }
    // }

    void OnDestroy()
    {
        onPhaseChangedUnsub?.Invoke();
    }
}
