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
    public int autoManaPerTurn = 2;  // 每回合自动增加的法力值
    
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
        shiled = 0; // 每回合开始重置护盾值
        ChangeMana(autoManaPerTurn); // 每回合增加自动法力值

        ApplyDots();  // 回合开始应用所有Dot效果

        Action();
    }

    protected abstract void Action();

    public void EndTurn()
    {
        EventCenter.Publish("CharacterEndedTurn");
    }


    // DOT效果
    public List<DotEffect> dotBar = new List<DotEffect>();
    private void ApplyDots()
    {
        foreach (var effect in new List<DotEffect>(dotBar))
        {
            effect.Apply();
        }
    }
    public void AddDot(DotEffect dotEffect)
    {
        dotBar.Add(dotEffect);
    }
    public void RemoveDot(Func<DotEffect, bool> condition)
    {
        foreach (var de in new List<DotEffect>(dotBar))
        {
            if (condition.Invoke(de))
            {
                dotBar.Remove(de);
            }
        }
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
        if (Cards.Contains(card) && mana >= card.ManaCost)
        {
            // 扣除法力值
            ChangeMana(-card.ManaCost);

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
