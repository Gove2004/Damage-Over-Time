using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseCharacter
{
    // 目标
    public BaseCharacter Target { get; set; }


    // 属性
    public int health = 100;
    public int mana = 1;
    
    public void ChangeHealth(int amount)
    {
        health += amount;
        if (health <= 0)
        {
            health = 0;
            
            EventCenter.Publish("CharacterDied", this);
        }
    }

    public void ChangeMana(int amount)
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
        EventCenter.Publish("TurnStarted", this);
        
        ChangeMana(2); // 每回合增加2点法力值

        ApplyDots();  // 回合开始应用所有Dot效果

        Action();
    }

    protected abstract void Action();

    public void EndTurn()
    {
        EventCenter.Publish("TurnEnded", this);
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
    public List<BaseCard> handCards = new List<BaseCard>();

    public void DrawCard()
    {

        // 随机从牌库中抽取一张卡牌加入手牌
        BaseCard newCard = CardFactory.GetRandomCard();
        handCards.Add(newCard);

        mana -= 1; // 抽卡消耗1点法力值
    
        EventCenter.Publish("CardDrawn", newCard);
    }


    public void PlayCard(BaseCard card)
    {
        if (handCards.Contains(card) && mana >= card.ManaCost)
        {
            // 使用卡牌效果
            card.Execute(this, Target);

            // 扣除法力值
            ChangeMana(-card.ManaCost);

            // 从手牌中移除卡牌
            handCards.Remove(card);

            EventCenter.Publish("CardPlayed", card);
        }
        else
        {
            Debug.LogWarning("无法使用卡牌: " + card.Name);
        }
    }
}
