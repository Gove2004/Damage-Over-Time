using System.Collections.Generic;
using Unity.Android.Gradle.Manifest;
using Unity.VisualScripting;
using UnityEngine;

public abstract class BaseCharacter
{
    // 事件
    public event System.Action OnTurnStart;
    public event System.Action<BaseCard> OnCardPlayed;
    public event System.Action<BaseCard> OnCardDrawn;
    public event System.Action<int> OnHealthChanged;
    public event System.Action<int> OnManaChanged;
    public event System.Action OnTurnEnd;
    public event System.Action OnDeath;

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
            OnDeath?.Invoke();
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
        OnTurnStart?.Invoke();
        
        ChangeMana(2); // 每回合增加2点法力值

        ApplyDots();

        Action();
    }

    protected virtual void Action()
    {
        // 这里可以添加AI逻辑或玩家输入处理
        EndTurn();
    }

    public void EndTurn()
    {
        OnTurnEnd?.Invoke();
    }

    // DOT条
    public DotBar damageBar = new DotBar();
    public DotBar healBar = new DotBar();
    public DotBar manaBar = new DotBar();
    // 更多DOT条可以在这里添加

    public void ApplyDots()
    {
        // 应用伤害DOT
        float damage = damageBar.GetDot(1);
        ChangeHealth(-(int)damage);
        damageBar.StepTurn();

        // 应用回复DOT
        float heal = healBar.GetDot(1);
        ChangeHealth((int)heal);
        healBar.StepTurn();

        // 应用法力DOT
        float manaGain = manaBar.GetDot(1);
        ChangeMana((int)manaGain);
        manaBar.StepTurn();

        // 这里可以添加更多DOT类型的应用逻辑
    }


    // 卡牌逻辑
    public List<BaseCard> handCards = new List<BaseCard>();

    public void DrawCard(int nums = 1)
    {
        for (int i = 0; i < nums; i++)
        {
            // 随机从牌库中抽取一张卡牌加入手牌
            BaseCard newCard = CardFactory.GetRandomCard();
            handCards.Add(newCard);

            mana -= 1; // 抽卡消耗1点法力值
        
            OnCardDrawn?.Invoke(newCard);
        }
    }


    public void PlayCard(BaseCard card)
    {
        if (handCards.Contains(card) && mana >= card.ManaCost)
        {
            // 使用卡牌效果
            card.Excute(this, Target);

            // 扣除法力值
            mana -= card.ManaCost;

            // 从手牌中移除卡牌
            handCards.Remove(card);

            OnCardPlayed?.Invoke(card);
        }
        else
        {
            Debug.LogWarning("无法使用卡牌: " + card.Name);
        }
    }
}
