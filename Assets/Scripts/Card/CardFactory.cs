

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




    // 这是玩家的牌组, 是一个权重字典
    private static List<BaseCard> playerDeck = new()
    {
        new 测试(), new 测试(), 
        new 流血(), new 流血(), new 流血(),
        new 恢复(), new 恢复(), new 恢复(),
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
