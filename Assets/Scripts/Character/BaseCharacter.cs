using System;
using System.Collections.Generic;
using System.Collections;
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
    private bool immuneSelfDamage = false;
    private int overclockMultiplier = 1;
    protected int extraCardDuration = 0;
    private float damageTakenMultiplier = 1f;
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
    public IEnumerator StartTurnRoutine()
    {
        EventCenter.Publish("TurnStart", this);

        IsInTurn = true;
        immuneThisTurn = false;

        shiled = Mathf.Max(0, shiled-3);  // 另一个方案是每回合衰减3

        // Async Apply Dots
        yield return ApplyDotsRoutine();

        if (!IsInTurn) yield break;

        Action();
    }

    // Deprecated synchronous StartTurn
    public void StartTurn()
    {
        // Warn: This should not be called directly by BattleManager anymore for turn logic if we want async dots.
        // But for compatibility, we can just call the routine on a temporary runner if needed, or just run sync logic.
        // For now, let's keep the logic but we expect BattleManager to call StartTurnRoutine.
        // To avoid duplication, we might just have this call StartTurnRoutine via a runner if we had one.
        // But simpler: just implement the old logic synchronously here for fallback.
        
        EventCenter.Publish("TurnStart", this);
        IsInTurn = true;
        immuneThisTurn = false;
        shiled = Mathf.Max(0, shiled-3);
        ApplyDots(); 
        if (!IsInTurn) return;
        Action();
    }

    protected abstract void Action();

    public void EndTurn()
    {
        IsInTurn = false;
        ChangeMana(autoManaPerTurn); // 每回合结束增加自动法力值
        EventCenter.Publish("CharacterEndedTurn");
    }


    // DOT效果
    public List<Dot> dotBar = new List<Dot>();
    
    // Sync version (legacy/fallback)
    private void ApplyDots()
    {
        foreach (var effect in new List<Dot>(dotBar))
        {
            effect.Apply();
        }
    }

    // Async version
    private IEnumerator ApplyDotsRoutine()
    {
        var dotsToProcess = new List<Dot>(dotBar);
        if (dotsToProcess.Count > 0)
        {
            float maxTotalDuration = 10f;
            float delay = Mathf.Min(0.5f, maxTotalDuration / dotsToProcess.Count);

            Transform targetTransform = null;
            if (DamageEffectManager.Instance != null)
            {
                 // Determine transform based on character type
                 // A bit hacky since BaseCharacter doesn't know about Transforms directly usually
                 // But we can check against BattleManager instance
                 if (this == BattleManager.Instance.player) targetTransform = DamageEffectManager.Instance.playerPos;
                 else if (this == BattleManager.Instance.enemy) targetTransform = DamageEffectManager.Instance.enemyPos;
            }

            foreach (var effect in dotsToProcess)
            {
                // Show Description
                if (targetTransform != null && DamageEffectManager.Instance != null)
                {
                    DamageEffectManager.Instance.ShowFloatingText(targetTransform, effect.description?.Invoke() ?? "", Color.yellow);
                }

                effect.Apply();
                
                // Wait
                yield return new WaitForSeconds(delay);
            }
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

    public void SetImmuneSelfDamage(bool value)
    {
        immuneSelfDamage = value;
    }

    public void SetDamageTakenMultiplier(float multiplier)
    {
        damageTakenMultiplier = Mathf.Max(0f, multiplier);
    }

    public void ApplyHealthChange(int amount, BaseCharacter source = null)
    {
        if (amount < 0 && immuneThisTurn) return;
        if (amount < 0 && source == this && immuneSelfDamage) return;

        if (amount < 0 && damageTakenMultiplier != 1f)
        {
            int damage = -amount;
            damage = Mathf.RoundToInt(damage * damageTakenMultiplier);
            if (damage <= 0) return;
            amount = -damage;
        }

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


    public void ApplyOverclock(int factor)
    {
        if (factor == 1) return;
        overclockMultiplier *= factor;


        // 新的超频只对玩家手牌生效
        foreach (var card in Cards)
        {
            card.MultiplyNumbers(factor);
        }
    }

    public void AddGlobalDurationBonus(int amount)
    {
        if (amount == 0) return;
        extraCardDuration += amount;
    }

    private void ApplyBuffsToCard(BaseCard card)
    {
        if (card == null) return;
        if (overclockMultiplier != 1)
        {
            // card.MultiplyNumbers(overclockMultiplier);  // // 新的超频只对玩家手牌生
        }
        if (extraCardDuration != 0)
        {
            card.AddDuration(extraCardDuration);
        }
    }



    // 卡牌逻辑
    public List<BaseCard> Cards = new List<BaseCard>();
    protected virtual int MaxHandSize => int.MaxValue;
    private bool IsHandFull => MaxHandSize > 0 && Cards.Count >= MaxHandSize;


    public BaseCard GainRandomCard()
    {
        if (IsHandFull) return null;
        BaseCard newCard = CardFactory.GetRandomCard();
        if (newCard == null) return null;
        ApplyBuffsToCard(newCard);
        Cards.Add(newCard);
        if (this is Player) EventCenter.Publish("Player_DrawCard", newCard);
        return newCard;
    }

    /// <summary>
    /// 获取指定卡牌（不走抽牌逻辑）
    /// </summary>
    /// <param name="card"></param>
    public void GainCard(BaseCard card)
    {
        if (card == null) return;
        if (IsHandFull) return;
        ApplyBuffsToCard(card);
        Cards.Add(card);
        if (this is Player) EventCenter.Publish("Player_DrawCard", card);
    }


    public BaseCard DrawCard(int cost = 1)
    {
        if (IsHandFull) return null;
        if (mana < cost) return null;  // 抽卡需要消耗法力值
        ChangeMana(-cost);  // 抽卡消耗指定点法力值

        // 随机从牌库中抽取一张卡牌加入手牌
        BaseCard baseCard;
        if (this is Player)
        {  // 玩家从牌组中抽牌
            baseCard = CardFactory.DrawCardFromPlayerDeck();
        }
        else
        {  // 敌人直接随机生成卡牌
            baseCard = CardFactory.GetRandomEnemyCard();
        }
        if (baseCard == null) return null;
        ApplyBuffsToCard(baseCard);
        Cards.Add(baseCard);

        EventCenter.Publish("CardDrawn", baseCard);

        return baseCard;
    }

    public void PlayCard(BaseCard card)
    {
        if (Cards.Contains(card) && mana >= card.Cost)
        {
            // 扣除法力值
            ChangeMana(-card.Cost);

            // 从手牌中移除卡牌
            Cards.Remove(card);

            // 使用卡牌效果
            card.Execute(this, Target);

            EventCenter.Publish("CardPlayed", card);
        }
        else
        {
            Debug.LogWarning("无法使用卡牌: " + card.Name);
        }
    }

    public void RemoveCard(BaseCard card)
    {
        if (Cards.Contains(card))
        {
            Cards.Remove(card);
            if (this is Player)
            {
                EventCenter.Publish("Player_RemoveCard", card);
            }
        }
    }
}
