using UnityEngine;

public class GMTool : MonoBehaviour
{
    private string healthDelta = "0";
    private string manaDelta = "0";
    private string cardId = "1000";
    private string message = "";

    private void OnGUI()
    {
        if (BattleManager.Instance == null) return;
        var player = BattleManager.Instance.player;
        if (player == null) return;

        GUILayout.BeginArea(new Rect(10, 10, 260, 260), "GM", GUI.skin.window);

        GUILayout.Label("玩家生命值调整(可负数)");
        healthDelta = GUILayout.TextField(healthDelta);
        if (GUILayout.Button("应用生命值"))
        {
            if (int.TryParse(healthDelta, out var value))
            {
                player.ApplyHealthChange(value, player);
                message = $"生命值调整 {value}";
            }
            else
            {
                message = "生命值输入无效";
            }
        }

        GUILayout.Label("玩家法力值调整(可负数)");
        manaDelta = GUILayout.TextField(manaDelta);
        if (GUILayout.Button("应用法力值"))
        {
            if (int.TryParse(manaDelta, out var value))
            {
                player.ChangeMana(value);
                message = $"法力值调整 {value}";
            }
            else
            {
                message = "法力值输入无效";
            }
        }

        GUILayout.Label("获取指定ID卡牌");
        cardId = GUILayout.TextField(cardId);
        if (GUILayout.Button("获取卡牌"))
        {
            if (int.TryParse(cardId, out var id))
            {
                var data = CardDatabase.GetCardData(id);
                if (data != null)
                {
                    var card = CardFactory.GetThisCard(data.name);
                    if (card != null)
                    {
                        player.GainCard(card);
                        message = $"已获得卡牌 {data.name}";
                    }
                    else
                    {
                        message = "未找到卡牌类";
                    }
                }
            }
            else
            {
                message = "卡牌ID输入无效";
            }
        }

        if (!string.IsNullOrEmpty(message))
        {
            GUILayout.Label(message);
        }

        GUILayout.EndArea();
    }
}
