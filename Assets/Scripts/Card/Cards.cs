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
        int duration = Duration;
        int value = Value;
        if (duration <= 0) return;
        var dot = new Dot(user, target, duration, d => user.DealDamage(target, value));
        user.dotBar.Add(dot);
    }
}


public class 恢复 : BaseCard
{
    protected override int id => 1002;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        int duration = Duration;
        int value = Value;
        if (duration <= 0) return;
        var dot = new Dot(user, user, duration, d => user.ApplyHealthChange(value, user));
        user.dotBar.Add(dot);
    }
}



public class 入魔 : BaseCard
{
    protected override int id => 1003;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        int duration = Duration;
        int value = Value;
        if (duration <= 0) return;
        var dot = new Dot(user, user, duration, d => d.target.ChangeMana(value));
        user.dotBar.Add(dot);
    }
}


public class 贪婪 : BaseCard
{
    protected override int id => 1004;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        if (Duration <= 0) return;
        int drawCount = Mathf.Max(0, Value);
        var dot = new Dot(user, user, Duration, d =>
        {
            for (int i = 0; i < drawCount; i++)
            {
                user.DrawCard(0);
            }
        });
        user.dotBar.Add(dot);
    }
}


public class 吸取 : BaseCard
{
    protected override int id => 1005;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        int duration = Duration;
        int value = Value;
        if (duration <= 0) return;
        var dot = new Dot(user, target, duration, d =>
        {
            user.DealDamage(target, value);
            user.ApplyHealthChange(value, user);
        });
        user.dotBar.Add(dot);
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
        user.dotBar.Add(dot);
    }
}


public class 第7张牌 : BaseCard
{
    protected override int id => 1007;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        int duration = Duration;
        int value = Value;
        if (duration <= 0) return;
        if (value < 0) value = 0;
        var dot = new Dot(user, target, duration, d =>
        {
            int effectIndex = UnityEngine.Random.Range(0, 7);
            if (effectIndex == 0)
            {
                int damage = value * 11;
                if (UnityEngine.Random.value < 0.77f)
                {
                    damage *= 2;
                    if (UnityEngine.Random.value < 0.77f)
                    {
                        damage = Mathf.Max(0, value / 7);
                    }
                }
                user.DealDamage(target, damage);
            }
            else if (effectIndex == 1)
            {
                user.SetImmuneThisTurn(true);
            }
            else if (effectIndex == 2)
            {
                int stealCount = Mathf.Max(0, value);
                for (int i = 0; i < stealCount; i++)
                {
                    bool stolen = false;
                    for (int attempt = 0; attempt < 3 && !stolen; attempt++)
                    {
                        int pick = UnityEngine.Random.Range(0, 3);
                        if (pick == 0)
                        {
                            if (target != null && target.Cards.Count > 0)
                            {
                                int index = UnityEngine.Random.Range(0, target.Cards.Count);
                                BaseCard stolenCard = target.Cards[index];
                                target.Cards.RemoveAt(index);
                                user.GainCard(stolenCard);
                                stolen = true;
                            }
                        }
                        else if (pick == 1)
                        {
                            if (target != null && target.dotBar.Count > 0)
                            {
                                int index = UnityEngine.Random.Range(0, target.dotBar.Count);
                                Dot stolenDot = target.dotBar[index];
                                target.dotBar.RemoveAt(index);
                                stolenDot.source = user;
                                stolenDot.target = user;
                                user.dotBar.Add(stolenDot);
                                stolen = true;
                            }
                        }
                        else
                        {
                            if (target != null && target.mana > 0)
                            {
                                target.ChangeMana(-1);
                                user.ChangeMana(1);
                                stolen = true;
                            }
                        }
                    }
                    if (!stolen) break;
                }
            }
            else if (effectIndex == 3)
            {
                user.ChangeMana(value);
                if (user.Cards.Count > 0)
                {
                    if (user is Player)
                    {
                        var discardList = new System.Collections.Generic.List<BaseCard>(user.Cards);
                        foreach (var card in discardList)
                        {
                            user.Cards.Remove(card);
                            EventCenter.Publish("Player_PlayCard", card);
                        }
                    }
                    else
                    {
                        user.Cards.Clear();
                    }
                }
            }
            else if (effectIndex == 4)
            {
                int count = Mathf.Max(0, value);
                for (int i = 0; i < count; i++)
                {
                    user.GainRandomCard();
                }
                user.EndTurn();
            }
            else if (effectIndex == 5)
            {
                int add = Mathf.Max(0, value);
                if (add > 0)
                {
                    foreach (var otherDot in user.dotBar)
                    {
                        if (otherDot != d)
                        {
                            otherDot.duration += add;
                        }
                    }
                }
            }
            else
            {
                int heal = value * 11;
                if (UnityEngine.Random.value < 0.07f)
                {
                    heal = Mathf.Max(0, value / 7);
                }
                user.ApplyHealthChange(heal, user);
            }
        });
        user.dotBar.Add(dot);
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
        user.dotBar.Add(dot);
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
        user.dotBar.Add(dot);
    }
}


public class 延续 : BaseCard
{
    protected override int id => 1010;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        int add = Value;
        if (add == 0) return;
        foreach (var dot in user.dotBar)
        {
            dot.duration += add;
        }
        foreach (var card in user.Cards)
        {
            card.AddDuration(add);
        }
    }
}


public class 超频 : BaseCard
{
    protected override int id => 1011;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        user.ApplyOverclock(Value);
    }
}


public class 偷窃 : BaseCard
{
    protected override int id => 1012;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        if (Duration <= 0) return;
        int stealCount = Mathf.Max(0, Value);
        if (stealCount == 0) return;
        var dot = new Dot(user, user, Duration, d =>
        {
            if (target == null || target.Cards.Count == 0) return;
            int count = Mathf.Min(stealCount, target.Cards.Count);
            for (int i = 0; i < count; i++)
            {
                int index = UnityEngine.Random.Range(0, target.Cards.Count);
                BaseCard stolen = target.Cards[index];
                target.Cards.RemoveAt(index);
                user.GainCard(stolen);
                if (target.Cards.Count == 0) break;
            }
        });
        user.dotBar.Add(dot);
    }
}


public class 结算 : BaseCard
{
    protected override int id => 1013;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        int times = Mathf.Max(0, Value);
        for (int i = 0; i < times; i++)
        {
            user.TriggerDotsOnce();
            target?.TriggerDotsOnce();
        }
    }
}


public class 疯狂 : BaseCard
{
    protected override int id => 1014;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        int duration = Duration;
        int value = Value;
        if (duration <= 0) return;
        var dot = new Dot(user, target, duration, d => user.DealDamage(target, value));
        user.dotBar.Add(dot);
    }
}


public class 彻底疯狂 : BaseCard
{
    protected override int id => 1015;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        int duration = Duration;
        int value = Value;
        if (duration <= 0) return;
        int actualDuration = UnityEngine.Random.Range(1, duration + 1);
        var dot = new Dot(user, target, actualDuration, d =>
        {
            int damage = UnityEngine.Random.Range(0, value + 1);
            user.DealDamage(target, damage);
        });
        user.dotBar.Add(dot);
    }
}


public class 增援未来 : BaseCard
{
    protected override int id => 1016;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        int delay = 3;
        int duration = Duration;
        int value = Value;
        var dot = new Dot(user, user, duration + delay, d =>
        {
            if (delay > 0)
            {
                delay--;
                return;
            }
            user.ApplyHealthChange(value, user);
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
        int duration = Duration;
        int value = Value;
        if (duration <= 0) return;
        var dot = new Dot(user, target, duration, d =>
        {
            if (UnityEngine.Random.Range(0, 3) == 0)
            {
                user.DealDamage(target, value);
            }
        });
        user.dotBar.Add(dot);
    }
}


public class 生命彩票 : BaseCard
{
    protected override int id => 1021;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        int duration = Duration;
        int value = Value;
        if (duration <= 0) return;
        var dot = new Dot(user, user, duration, d =>
        {
            if (UnityEngine.Random.Range(0, 3) == 0)
            {
                user.ApplyHealthChange(value, user);
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
        int duration = Duration;
        int value = Value;
        if (duration <= 0) return;
        var dot = new Dot(user, user, duration, d =>
        {
            if (UnityEngine.Random.Range(0, 3) == 0)
            {
                user.ChangeMana(value);
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
        int maxCount = Mathf.Max(0, Value);
        int count = UnityEngine.Random.Range(0, maxCount + 1);
        if (count == 0) return;
        string[] names = { "攻击彩票", "生命彩票", "魔力彩票" };
        for (int i = 0; i < count; i++)
        {
            string name = names[UnityEngine.Random.Range(0, names.Length)];
            user.GainCard(CardFactory.GetThisCard(name));
        }
    }
}


public class 破甲 : BaseCard
{
    protected override int id => 1024;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        if (target == null) return;
        if (Duration <= 0) return;
        float multiplier = Mathf.Max(0, Value);
        target.SetDamageTakenMultiplier(multiplier);
        var dot = new Dot(user, target, Duration, d =>
        {
            target.SetDamageTakenMultiplier(multiplier);
        }, d =>
        {
            target.SetDamageTakenMultiplier(1f);
        });
        user.dotBar.Add(dot);
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
        if (Duration <= 0) return;
        int stealCount = Mathf.Max(0, Value);
        if (stealCount == 0) return;
        var dot = new Dot(user, user, Duration, d =>
        {
            if (target == null || target.dotBar.Count == 0) return;
            int count = Mathf.Min(stealCount, target.dotBar.Count);
            for (int i = 0; i < count; i++)
            {
                if (target.dotBar.Count == 0) break;
                int index = UnityEngine.Random.Range(0, target.dotBar.Count);
                Dot stolen = target.dotBar[index];
                target.dotBar.RemoveAt(index);
                stolen.source = user;
                stolen.target = user;
                user.dotBar.Add(stolen);
            }
        });
        user.dotBar.Add(dot);
    }
}


public class 偷魔 : BaseCard
{
    protected override int id => 1027;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        int duration = Duration;
        int value = Value;
        if (duration <= 0) return;
        var dot = new Dot(user, user, duration, d =>
        {
            if (target == null) return;
            if (target.mana <= 0) return;
            target.ChangeMana(-value);
            user.ChangeMana(value);
        });
        user.dotBar.Add(dot);
    }
}


public class 苦修 : BaseCard
{
    protected override int id => 1028;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        int duration = Duration;
        int value = Value;
        if (duration <= 0) return;
        Action<int, BaseCharacter> handler = (amount, source) =>
        {
            if (source != user) return;
            user.ChangeMana(value);
        };
        user.DamageTaken += handler;
        var dot = new Dot(user, user, duration, d => { }, d => user.DamageTaken -= handler);
        user.dotBar.Add(dot);
    }
}


public class 献祭 : BaseCard
{
    private static int SacrificeCount = 0;
    protected override int id => 1029;

    private int CurrentDamage => Value * (1 << SacrificeCount);

    public override string GetDynamicDescription()
    {
        if (string.IsNullOrEmpty(Description)) return Description;
        return Description
            .Replace("[费用]", Cost.ToString())
            .Replace("[数值]", CurrentDamage.ToString())
            .Replace("[持续时间]", Duration.ToString());
    }

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        int baseValue = Value;
        if (Duration <= 0) return;
        var dot = new Dot(user, user, Duration, d =>
        {
            int damage = baseValue * (1 << SacrificeCount);
            user.DealDamage(user, damage);
            if (target != null) user.DealDamage(target, damage);
            SacrificeCount++;
        });
        user.dotBar.Add(dot);
    }
}


public class 卖血 : BaseCard
{
    protected override int id => 1030;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        int duration = Duration;
        int value = Value;
        if (duration <= 0) return;
        int drawCount = Mathf.Max(0, value / 10);
        var dot = new Dot(user, user, duration, d =>
        {
            user.ApplyHealthChange(-value, user);
            for (int i = 0; i < drawCount; i++)
            {
                user.DrawCard(0);
            }
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
            if (source != user) return;
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
        user.SetImmuneSelfDamage(true);
        var dot = new Dot(user, user, Duration + 1, d =>
        {
            if (d.duration <= 1)
            {
                user.SetImmuneSelfDamage(false);
                return;
            }
            user.SetImmuneSelfDamage(true);
        });
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
        if (user.Cards.Count <= 0) return;
        var discardList = new System.Collections.Generic.List<BaseCard>(user.Cards);
        discardList.Remove(this);
        int discardCount = discardList.Count;
        if (discardCount <= 0) return;
        if (user is Player)
        {
            foreach (var card in discardList)
            {
                user.Cards.Remove(card);
                EventCenter.Publish("Player_PlayCard", card);
            }
        }
        else
        {
            foreach (var card in discardList)
            {
                user.Cards.Remove(card);
            }
        }
        for (int i = 0; i < discardCount; i++)
        {
            user.GainRandomCard();
        }
    }
}
