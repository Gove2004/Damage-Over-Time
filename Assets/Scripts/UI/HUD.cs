using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HUD : MonoBehaviour
{
    // UI元素
    public TextMeshProUGUI playerHPText;
    public TextMeshProUGUI playerSPText;
    public TextMeshProUGUI playerMPText;
    public Image playerMPBgImage; // 新增：MP背景图片引用
    public TextMeshProUGUI enemyHPText;
    public TextMeshProUGUI enemySPText;
    public TextMeshProUGUI enemyMPText;

    private int lastPlayerMana = -1;
    private BaseCharacter lastPlayerInstance;

    void Start()
    {
        // 尝试自动查找 MP 背景
        if (playerMPBgImage == null)
        {
            // 假设 PlayerMP_BG 是一个名为 "PlayerMP_BG" 的子对象或者在场景中唯一
            var foundObj = GameObject.Find("PlayerMP_BG");
            if (foundObj != null)
            {
                playerMPBgImage = foundObj.GetComponent<Image>();
            }
        }
    }

    void Update()
    {
        // 简化逻辑， 每帧更新UI显示
        if (BattleManager.Instance != null)
        {
            var player = BattleManager.Instance.player;
            var enemy = BattleManager.Instance.enemy;

            // 检测是否开启了新的一局（玩家实例改变）
            if (player != lastPlayerInstance)
            {
                lastPlayerInstance = player;
                lastPlayerMana = -1;
            }

            if (player != null)
            {
                playerHPText.text = $"{player.health}";
                playerSPText.text = $"SP={player.shiled}";
                playerMPText.text = $"{player.mana}";

                // 检测 MP 增加并播放特效
                if (lastPlayerMana != -1 && player.mana > lastPlayerMana)
                {
                    PlayManaGainEffect();
                }
                lastPlayerMana = player.mana;
            }
            if (enemy != null)
            {
                enemyHPText.text = $"{enemy.health}";
                enemySPText.text = $"SP={enemy.shiled}";
                enemyMPText.text = $"MP={enemy.mana}";
            }
        }
    }

    private void PlayManaGainEffect()
    {
        if (playerMPBgImage != null)
        {
            // 使用 DOTween 制作发光效果 (颜色变亮然后恢复)
            playerMPBgImage.DOColor(Color.cyan, 0.3f).SetLoops(2, LoopType.Yoyo);
            // 或者简单的缩放效果
            playerMPBgImage.transform.DOScale(1.2f, 0.2f).SetLoops(2, LoopType.Yoyo);
        }
    }
}
