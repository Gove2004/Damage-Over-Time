using System.Text;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    // 简单的单例模式
    public static BattleManager Instance;
    void Awake()
    {
        Instance = this;
        if (GetComponent<GMTool>() == null)
        {
            gameObject.AddComponent<GMTool>();
        }
    }


    public BaseCharacter player;
    public BaseCharacter enemy;
    private int currentTurn = 1;


    public void StartBattle()
    {
        Debug.Log("战斗开始！");
        currentTurn = 0;

        player = new Player();
        enemy = new EnemyBoss();
        BaseCard.ResetOverclock();

        player.Target = enemy;
        enemy.Target = player;

        player.StartTurn();

        player.ChangeHealth(0);  // 触发UI更新
        enemy.ChangeHealth(0);  // 触发UI更新

        EventCenter.Publish("BattleStarted");

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



    void Start()
    {
        StartBattle();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartBattle();
        }
    }
}
