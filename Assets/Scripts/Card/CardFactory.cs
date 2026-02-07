

using System.Collections.Generic;


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

    private static List<BaseCard> allCards = new();

    public static List<BaseCard> GetAllCards() => allCards;

    /// <summary>
    /// 随机获取一张卡牌
    /// </summary>
    /// <returns></returns>
    public static BaseCard GetRandomCard()
    {
        int index = UnityEngine.Random.Range(0, allCards.Count);
        return allCards[index];
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
                return card;
            }
        }
        return null;
    }

    
}
