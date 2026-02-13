

using System.Collections.Generic;
using UnityEngine.InputSystem.Utilities;


public static class CardFactory
{
    // 自动扫所有卡牌类并注册
    static CardFactory()
    {
        // 这里可以使用反射来自动注册所有继承自BaseCard的类
        var baseType = typeof(BaseCard);
        foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            System.Type[] types;
            try { types = asm.GetTypes(); } catch { continue; }
            foreach (var t in types)
            {
                if (t.IsAbstract) continue;
                if (!baseType.IsAssignableFrom(t)) continue;
                var ctor = t.GetConstructor(System.Type.EmptyTypes);
                if (ctor == null) continue;
                try
                {
                    var inst = (BaseCard)System.Activator.CreateInstance(t);
                    if (!allCards.Exists(c => c.GetType() == t))
                        allCards.Add(inst);
                }
                catch { }
            }
        }
    }


    private static BaseCard CreateCardInstance(System.Type type)
    {
        try
        {
            return (BaseCard)System.Activator.CreateInstance(type);
        }
        catch
        {
            return null;
        }
    }


    /// <summary>
    /// 获取指定名称的卡牌
    /// </summary>
    /// <param name="cardName">卡牌名称</param>
    /// <returns></returns>
    public static BaseCard GetThisCard(string cardName)
    {
        foreach (var card in allCards)
        {
            if (card.Name == cardName)
            {
                return CreateCardInstance(card.GetType());
            }
        }
        return null;
    }

    

    // 这是所有卡牌的列表, 通过反射自动注册

    private static List<BaseCard> allCards = new();

    public static List<BaseCard> GetAllCards() => allCards;

    /// <summary>
    /// 随机获取一张卡牌
    /// </summary>
    /// <returns></returns>
    public static BaseCard GetRandomCard()
    {
        if (allCards.Count == 0) return null;
        int index = UnityEngine.Random.Range(0, allCards.Count);
        var type = allCards[index].GetType();
        return CreateCardInstance(type);
    }

    /// <summary>
    /// 随机获取一张敌人可用的卡牌（排除偷窃、回血、无敌类）
    /// </summary>
    /// <returns></returns>
    public static BaseCard GetRandomEnemyCard()
    {
        if (allCards.Count == 0) return null;
        
        // 定义黑名单（偷窃、回血、无敌、特殊机制类）
        var bannedCards = new HashSet<string> 
        { 
            // 偷窃类
            "偷窃", "偷月", "偷魔", "贪婪",
            // 回血类
            "恢复", "吸取", "生命彩票", "反伤", "吸血", "增援未来", "暴食",
            // 无敌/防御类
            "无敌金身", "逃避", "傲慢",
            // 混合/特殊机制类
            "七宗罪" // 包含多种随机效果（回血、偷窃、无敌等）
        };

        var validCards = new List<BaseCard>();
        foreach (var card in allCards)
        {
            if (!bannedCards.Contains(card.Name))
            {
                validCards.Add(card);
            }
        }
        
        if (validCards.Count == 0) return null;

        int index = UnityEngine.Random.Range(0, validCards.Count);
        // 使用实际对象的类型来创建新实例，而不是通过索引去allCards找
        // 因为validCards已经是过滤后的列表了
        var type = validCards[index].GetType();
        return CreateCardInstance(type);
    }




    // 这是玩家的牌组, 是一个权重字典
    private static List<BaseCard> playerDeck = new()
    {
        new 流血(), new 流血(), new 流血(), new 流血(),
        new 恢复(), new 恢复(), new 恢复(), new 恢复(),
        new 入魔(), new 入魔()
    };

    // 从中抽取一张
    public static BaseCard DrawCardFromPlayerDeck()
    {
        if (playerDeck.Count == 0) return null;
        int index = UnityEngine.Random.Range(0, playerDeck.Count);
        var card = playerDeck[index];
        // playerDeck.RemoveAt(index);  // 不真正移除, 只要抽了就算了， 傻逼AI
        return CreateCardInstance(card.GetType());
    }

    // 用一张卡牌替换其中一张
    public static void ReplaceCardInPlayerDeck(BaseCard newCard, int index)
    {
        if (index < 0 || index >= playerDeck.Count) return;

        playerDeck[index] = newCard;
    }

    // 获取玩家牌组
    public static List<BaseCard> GetPlayerDeck() => playerDeck;

}
