using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseCharacter
{
    // 目标
    public BaseCharacter Target { get; set; }

    // 基础属性
    public int health = 30;
    public int mana = 0;
    public int shiled = 0; // 护盾值
    public int autoManaPerTurn = 3;  // 每回合自动增加的法力值
    public bool IsInTurn { get; private set; }
    private bool immuneThisTurn = false;
    public event Action<int, BaseCharacter> DamageTaken;
    public event Action<int, BaseCharacter> DamageDealt;
    
    public abstract void ChangeHealth(int amount);
    

    public virtual void ChangeMana(int amount)
    {
        mana += amount;
        if (mana < 0)
        {
            mana = 0;
        }
    }
    

    // 行动
    public void StartTurn()
    {
        IsInTurn = true;
        immuneThisTurn = false;
        shiled = 0; // 每回合开始重置护盾值
        ApplyDots();  // 回合开始应用所有Dot效果

        Action();
    }

    protected abstract void Action();

    public void EndTurn()
    {
        IsInTurn = false;
        immuneThisTurn = false;
        ChangeMana(autoManaPerTurn); // 每回合结束增加自动法力值
        EventCenter.Publish("CharacterEndedTurn");
    }


    // DOT效果
    public List<Dot> dotBar = new List<Dot>();
    private void ApplyDots()
    {
        foreach (var effect in new List<Dot>(dotBar))
        {
            effect.Apply();
        }
    }
    public void AddDot(Dot dotEffect)
    {
        dotBar.Add(dotEffect);
    }
    public void RemoveDot(Func<Dot, bool> condition)
    {
        foreach (var de in new List<Dot>(dotBar))
        {
            if (condition.Invoke(de))
            {
                dotBar.Remove(de);
            }
        }
    }
    public void TriggerDotsOnce()
    {
        ApplyDots();
    }

    public void SetImmuneThisTurn(bool value)
    {
        immuneThisTurn = value;
    }

    public void ApplyHealthChange(int amount, BaseCharacter source = null)
    {
        if (amount < 0 && immuneThisTurn) return;

        ChangeHealth(amount);

        if (amount < 0)
        {
            int damage = -amount;
            DamageTaken?.Invoke(damage, source);
            source?.DamageDealt?.Invoke(damage, this);
        }
    }

    public void DealDamage(BaseCharacter target, int amount)
    {
        if (target == null || amount <= 0) return;
        target.ApplyHealthChange(-amount, this);
    }

    public BaseCard GainRandomCard()
    {
        BaseCard newCard = CardFactory.GetRandomCard();
        if (newCard == null) return null;
        Cards.Add(newCard);
        if (this is Player) EventCenter.Publish("Player_DrawCard", newCard);
        return newCard;
    }

    public void GainCard(BaseCard card)
    {
        if (card == null) return;
        Cards.Add(card);
        if (this is Player) EventCenter.Publish("Player_DrawCard", card);
    }


    // 卡牌逻辑
    public List<BaseCard> Cards = new List<BaseCard>();

    public BaseCard DrawCard()
    {
        if (mana <= 0) return null;  // 抽卡需要消耗法力值
        ChangeMana(-1);  // 抽卡消耗1点法力值

        // 随机从牌库中抽取一张卡牌加入手牌
        BaseCard newCard = CardFactory.GetRandomCard();
        Cards.Add(newCard);

        EventCenter.Publish("CardDrawn", newCard);

        return newCard;
    }

    public void PlayCard(BaseCard card)
    {
        if (Cards.Contains(card) && mana >= card.Cost)
        {
            // 扣除法力值
            ChangeMana(-card.Cost);

            // 使用卡牌效果
            card.Execute(this, Target);

            // 从手牌中移除卡牌
            Cards.Remove(card);

            EventCenter.Publish("CardPlayed", card);
        }
        else
        {
            Debug.LogWarning("无法使用卡牌: " + card.Name);
        }
    }
}
