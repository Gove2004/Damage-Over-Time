using System;
using System.Text;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    // 简单的单例模式
    public static BattleManager Instance;
    public bool IsGMMode = false;

    // 手动指定角色位置，用于 DamageEffectManager
    public Transform playerTransformRef;
    public Transform enemyTransformRef;

    void Awake()
    {
        Instance = this;


        if (GetComponent<GMTool>() == null && IsGMMode)
        {
            gameObject.AddComponent<GMTool>();
        }

        // 自动添加伤害特效管理器
        if (GetComponent<DamageEffectManager>() == null)
        {
            var dem = gameObject.AddComponent<DamageEffectManager>();
            // 自动注入引用
            dem.playerTransform = playerTransformRef;
            dem.enemyTransform = enemyTransformRef;
        }
        else
        {
            var dem = GetComponent<DamageEffectManager>();
            dem.playerTransform = playerTransformRef;
            dem.enemyTransform = enemyTransformRef;
        }

        // 自动添加音频管理器
        if (FindObjectOfType<AudioManager>() == null)
        {
            var audioObj = new GameObject("AudioManager");
            audioObj.AddComponent<AudioManager>();
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
        if (player == null || enemy == null) return; // Add null check
        currentTurn++;

        Debug.Log($"第{currentTurn}回合开始");

        // 轮流行动
        if (currentTurn % 2 == 1)
        {
            StartCoroutine(player.StartTurnRoutine());
        }
        else
        {
            StartCoroutine(enemy.StartTurnRoutine());
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
        
        // 游戏退出或重新加载时，清理所有静态事件，防止引用已销毁的对象
        EventCenter.ClearAllEvents();
    }
}
