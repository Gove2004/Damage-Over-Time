

using System.Collections.Generic;


public static class CardFactory
{
    // 自动扫所有卡牌类并注册（可选）
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

    private static List<BaseCard> allCards = new List<BaseCard>()
    {
        new 基础回复卡牌(),
        // 可以在这里添加更多卡牌实例
    };

    public static BaseCard GetRandomCard()
    {
        int index = UnityEngine.Random.Range(0, allCards.Count);
        return allCards[index];
    }


    
}