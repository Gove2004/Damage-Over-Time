using System.Text;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    // 简单的单例模式
    public static BattleManager Instance;
    void Awake()
    {
        Instance = this;
    }
    public bool DebugMode = true;

    public BaseCharacter player;
    public BaseCharacter enemy;
    public int currentTurn = 1;

    public void StartBattle()
    {
        Debug.Log("战斗开始！");
        currentTurn = 1;

        // player.OnTurnEnd += OnPlayerTurnEnd;
        // enemy.OnTurnEnd += OnEnemyTurnEnd;

        player.Target = enemy;
        enemy.Target = player;

        player.StartTurn();
    }

    private void OnPlayerTurnEnd()
    {
        Debug.Log("玩家回合结束，敌人开始行动。");

        enemy.StartTurn();
    }

    private void OnEnemyTurnEnd()
    {
        Debug.Log("敌人回合结束，玩家开始行动。");

        player.StartTurn();

        currentTurn++;
    }

    public void EndBattle()
    {
        Debug.Log("战斗结束。");
    }


    // void OnGUI()
    // {
    //     if (!DebugMode) return;

    //     StringBuilder sb = new StringBuilder("使用小键盘数字键测试功能: 0-使用手牌0, 1-使用手牌1, 2-抽卡, 3-结束回合");

    //     sb.AppendLine($"回合数: {currentTurn}");
    //     sb.AppendLine($"玩家 - 生命值: {player.health}, 法力值: {player.mana}");
    //     sb.AppendLine("DOT条状态:");
    //     sb.AppendLine($"  伤害DOT: {player.damageBar.ToString()}");
    //     sb.AppendLine($"  治疗DOT: {player.healBar.ToString()}");
    //     sb.AppendLine($"  法力DOT: {player.manaBar.ToString()}");
    //     sb.AppendLine("手牌:");
    //     for (int i = 0; i < player.handCards.Count; i++)
    //     {
    //         sb.AppendLine($"  玩家手牌 {i} - 名称: {player.handCards[i].Name}, 法力值消耗: {player.handCards[i].ManaCost}");
    //     }
        
        

    //     sb.AppendLine($"敌人 - 生命值: {enemy.health}, 法力值: {enemy.mana}");
    //     sb.AppendLine("DOT条状态:");
    //     sb.AppendLine($"  伤害DOT: {enemy.damageBar.ToString()}");
    //     sb.AppendLine($"  治疗DOT: {enemy.healBar.ToString()}");
    //     sb.AppendLine($"  法力DOT: {enemy.manaBar.ToString()}");
    //     sb.AppendLine("手牌:");
    //     for (int i = 0; i < enemy.handCards.Count; i++)
    //     {
    //         sb.AppendLine($"  敌人手牌 {i} - 名称: {enemy.handCards[i].Name}, 法力值消耗: {enemy.handCards[i].ManaCost}");
    //     }

    //     GUI.Label(new Rect(10, 10, 600, 600), sb.ToString());
    // }
}
