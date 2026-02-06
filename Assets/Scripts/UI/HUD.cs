using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    // UI元素
    public TextMeshProUGUI playerHPText;
    public TextMeshProUGUI playerSPText;
    public TextMeshProUGUI playerMPText;
    public TextMeshProUGUI enemyHPText;
    public TextMeshProUGUI enemySPText;
    public TextMeshProUGUI enemyMPText;

    void Update()
    {
        // 简化逻辑， 每帧更新UI显示
        if (BattleManager.Instance != null)
        {
            var player = BattleManager.Instance.player;
            var enemy = BattleManager.Instance.enemy;

            if (player != null)
            {
                playerHPText.text = $"HP={player.health}";
                playerSPText.text = $"SP={player.shiled}";
                playerMPText.text = $"MP={player.mana}";
            }
            if (enemy != null)
            {
                enemyHPText.text = $"SCORE={enemy.health}";
                enemySPText.text = $"SP={enemy.shiled}";
                enemyMPText.text = $"MP={enemy.mana}";
            }
        }
    }
}
