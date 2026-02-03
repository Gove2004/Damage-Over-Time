using UnityEngine;

public class Test : MonoBehaviour
{
    private BattleManager battleManager;




    void Start()
    {
        battleManager = BattleManager.Instance;

        battleManager.player = new Player();
        battleManager.enemy = new EnemyBoss();

        battleManager.StartBattle();
    }

    // Update is called once per frame
    void Update()
    {
        // 使用小键盘数字键测试功能: 0-使用手牌0, 1-使用手牌1, 2-抽卡, 3-结束回合
        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            try
            {
                battleManager.player.PlayCard(battleManager.player.handCards[0]);
            }
            catch (System.Exception e)
            {
                Debug.Log("没有手牌0可用: " + e.Message);
            }
        }

        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            try
            {
                battleManager.player.PlayCard(battleManager.player.handCards[1]);
            }
            catch (System.Exception e)
            {
                Debug.Log("没有手牌1可用: " + e.Message);
            }
        }

        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            battleManager.player.DrawCard(1);

        }

        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            battleManager.player.EndTurn();

        }
    }
}
