using System;
using UnityEngine;


public class 测试: BaseCard
{
    protected override int id => 1000;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        Debug.Log("测试白板卡牌被使用了！");
    }
}



public class 流血 : BaseCard
{
    protected override int id => 1001;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        if (Duration <= 0) return;
        var dot = new Dot(user, target, Duration, d => user.DealDamage(target, Value));
        target.dotBar.Add(dot);
    }
}


public class 恢复 : BaseCard
{
    protected override int id => 1002;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        if (Duration <= 0) return;
        var dot = new Dot(user, user, Duration, d => user.ApplyHealthChange(Value, user));
        user.dotBar.Add(dot);
    }
}



public class 入魔 : BaseCard
{
    protected override int id => 1003;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        if (Duration <= 0) return;
        var dot = new Dot(user, user, Duration, d => d.target.ChangeMana(Value));
        user.dotBar.Add(dot);
    }
}


public class 贪婪 : BaseCard
{
    protected override int id => 1004;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        if (Duration <= 0) return;
        var dot = new Dot(user, user, Duration, d => user.GainRandomCard());
        user.dotBar.Add(dot);
    }
}


public class 吸取 : BaseCard
{
    protected override int id => 1005;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        if (Duration <= 0) return;
        var dot = new Dot(user, target, Duration, d =>
        {
            user.DealDamage(target, Value);
            user.ApplyHealthChange(Value, user);
        });
        target.dotBar.Add(dot);
    }
}


public class 爆发 : BaseCard
{
    protected override int id => 1006;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        int manaSpent = user.mana;
        user.ChangeMana(-manaSpent);
        int damage = (int)Mathf.Pow(Value, manaSpent);
        if (Duration <= 0) return;
        var dot = new Dot(user, target, Duration, d => user.DealDamage(target, damage));
        target.dotBar.Add(dot);
    }
}


public class 第7张牌 : BaseCard
{
    protected override int id => 1007;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        if (Duration <= 0) return;
        var dotTarget = new Dot(user, target, Duration, d => user.DealDamage(target, Value));
        var dotSelf = new Dot(user, user, Duration, d => user.DealDamage(user, Value));
        target.dotBar.Add(dotTarget);
        user.dotBar.Add(dotSelf);
    }
}


public class 上三角 : BaseCard
{
    protected override int id => 1008;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        if (Duration <= 0) return;
        int damage = Value;
        var dot = new Dot(user, target, Duration, d =>
        {
            user.DealDamage(target, damage);
            damage *= 2;
        });
        target.dotBar.Add(dot);
    }
}


public class 下三角 : BaseCard
{
    protected override int id => 1009;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        if (Duration <= 0) return;
        int damage = Value;
        var dot = new Dot(user, target, Duration, d =>
        {
            user.DealDamage(target, damage);
            damage = Mathf.Max(0, damage / 2);
        });
        target.dotBar.Add(dot);
    }
}


public class 延续 : BaseCard
{
    protected override int id => 1010;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        foreach (var dot in user.dotBar)
        {
            dot.duration += Value;
        }
    }
}


public class 超频 : BaseCard
{
    protected override int id => 1011;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        foreach (var card in CardFactory.GetAllCards())
        {
            card.MultiplyNumbers(Value);
        }
    }
}


public class 偷窃 : BaseCard
{
    protected override int id => 1012;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        if (Duration <= 0) return;
        var dot = new Dot(user, user, Duration, d =>
        {
            if (target == null || target.Cards.Count == 0) return;
            int index = UnityEngine.Random.Range(0, target.Cards.Count);
            BaseCard stolen = target.Cards[index];
            target.Cards.RemoveAt(index);
            user.GainCard(stolen);
        });
        user.dotBar.Add(dot);
    }
}


public class 结算 : BaseCard
{
    protected override int id => 1013;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        user.TriggerDotsOnce();
        target?.TriggerDotsOnce();
    }
}


public class 疯狂 : BaseCard
{
    protected override int id => 1014;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        if (Duration <= 0) return;
        var dot = new Dot(user, target, Duration, d => user.DealDamage(target, Value));
        target.dotBar.Add(dot);
    }
}


public class 彻底疯狂 : BaseCard
{
    protected override int id => 1015;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        if (Duration <= 0) return;
        var dot = new Dot(user, target, Duration, d => user.DealDamage(target, Value));
        target.dotBar.Add(dot);
    }
}


public class 增援未来 : BaseCard
{
    protected override int id => 1016;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        int delay = 3;
        var dot = new Dot(user, user, Duration + delay, d =>
        {
            if (delay > 0)
            {
                delay--;
                return;
            }
            user.shiled += Value;
        });
        user.dotBar.Add(dot);
    }
}


public class 魔力急救 : BaseCard
{
    protected override int id => 1017;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        user.ChangeMana(Value);
    }
}


public class 闪击 : BaseCard
{
    protected override int id => 1018;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        user.DealDamage(target, Value);
    }
}


public class 随机种子 : BaseCard
{
    protected override int id => 1019;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        for (int i = 0; i < Value; i++)
        {
            user.GainRandomCard();
        }
    }
}


public class 攻击彩票 : BaseCard
{
    protected override int id => 1020;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        if (Duration <= 0) return;
        var dot = new Dot(user, target, Duration, d =>
        {
            if (UnityEngine.Random.Range(0, 3) == 0)
            {
                user.DealDamage(target, Value);
            }
        });
        target.dotBar.Add(dot);
    }
}


public class 生命彩票 : BaseCard
{
    protected override int id => 1021;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        if (Duration <= 0) return;
        var dot = new Dot(user, user, Duration, d =>
        {
            if (UnityEngine.Random.Range(0, 3) == 0)
            {
                user.ApplyHealthChange(Value, user);
            }
        });
        user.dotBar.Add(dot);
    }
}


public class 魔力彩票 : BaseCard
{
    protected override int id => 1022;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        if (Duration <= 0) return;
        var dot = new Dot(user, user, Duration, d =>
        {
            if (UnityEngine.Random.Range(0, 3) == 0)
            {
                user.ChangeMana(Value);
            }
        });
        user.dotBar.Add(dot);
    }
}


public class 随机彩票 : BaseCard
{
    protected override int id => 1023;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        string[] names = { "攻击彩票", "生命彩票", "魔力彩票" };
        string name = names[UnityEngine.Random.Range(0, names.Length)];
        user.GainCard(CardFactory.GetThisCard(name));
    }
}


public class 破甲 : BaseCard
{
    protected override int id => 1024;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        if (target != null) target.shiled = 0;
    }
}


public class 无敌金身 : BaseCard
{
    protected override int id => 1025;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        int delay = 1;
        bool activated = false;
        var dot = new Dot(user, user, Duration + delay, d =>
        {
            if (!activated)
            {
                activated = true;
                return;
            }
            user.SetImmuneThisTurn(true);
        });
        user.dotBar.Add(dot);
    }
}


public class 偷dot : BaseCard
{
    protected override int id => 1026;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        if (target == null || target.dotBar.Count == 0) return;
        int index = UnityEngine.Random.Range(0, target.dotBar.Count);
        Dot stolen = target.dotBar[index];
        target.dotBar.RemoveAt(index);
        stolen.source = user;
        stolen.target = user;
        user.dotBar.Add(stolen);
    }
}


public class 偷魔 : BaseCard
{
    protected override int id => 1027;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        if (Duration <= 0) return;
        var dot = new Dot(user, user, Duration, d =>
        {
            if (target == null) return;
            if (target.mana <= 0) return;
            target.ChangeMana(-Value);
            user.ChangeMana(Value);
        });
        user.dotBar.Add(dot);
    }
}


public class 苦修 : BaseCard
{
    protected override int id => 1028;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        if (Duration <= 0) return;
        Action<int, BaseCharacter> handler = (amount, source) =>
        {
            if (!user.IsInTurn) return;
            user.ChangeMana(Value);
        };
        user.DamageTaken += handler;
        var dot = new Dot(user, user, Duration, d => { }, d => user.DamageTaken -= handler);
        user.dotBar.Add(dot);
    }
}


public class 献祭 : BaseCard
{
    private static int SacrificeMultiplier = 1;
    protected override int id => 1029;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        if (Duration <= 0) return;
        SacrificeMultiplier *= 2;
        var dot = new Dot(user, user, Duration, d =>
        {
            int damage = Value * SacrificeMultiplier;
            user.DealDamage(user, damage);
            if (target != null) user.DealDamage(target, damage);
        });
        user.dotBar.Add(dot);
    }
}


public class 卖血 : BaseCard
{
    protected override int id => 1030;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        if (Duration <= 0) return;
        var dot = new Dot(user, user, Duration, d =>
        {
            user.ApplyHealthChange(-Value, user);
            user.GainRandomCard();
        });
        user.dotBar.Add(dot);
    }
}


public class 反伤 : BaseCard
{
    protected override int id => 1031;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        if (Duration <= 0) return;
        Action<int, BaseCharacter> handler = (amount, source) =>
        {
            if (!user.IsInTurn) return;
            user.ApplyHealthChange(amount, user);
            if (target != null) user.DealDamage(target, amount);
        };
        user.DamageTaken += handler;
        var dot = new Dot(user, user, Duration, d => { }, d => user.DamageTaken -= handler);
        user.dotBar.Add(dot);
    }
}


public class 逃避 : BaseCard
{
    protected override int id => 1032;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        if (Duration <= 0) return;
        user.SetImmuneThisTurn(true);
        var dot = new Dot(user, user, Duration, d => user.SetImmuneThisTurn(true));
        user.dotBar.Add(dot);
    }
}


public class 吸血 : BaseCard
{
    protected override int id => 1033;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        if (Duration <= 0) return;
        Action<int, BaseCharacter> handler = (amount, victim) =>
        {
            user.ApplyHealthChange(amount, user);
        };
        user.DamageDealt += handler;
        var dot = new Dot(user, user, Duration, d => { }, d => user.DamageDealt -= handler);
        user.dotBar.Add(dot);
    }
}


public class 制衡 : BaseCard
{
    protected override int id => 1034;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        int count = user.Cards.Count;
        if (count <= 0) return;
        if (user is Player)
        {
            foreach (var card in new System.Collections.Generic.List<BaseCard>(user.Cards))
            {
                user.Cards.Remove(card);
                EventCenter.Publish("Player_PlayCard", card);
            }
        }
        else
        {
            user.Cards.Clear();
        }
        for (int i = 0; i < count; i++)
        {
            user.GainRandomCard();
        }
    }
}
