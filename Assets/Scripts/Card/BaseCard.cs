

/// <summary>
/// 卡牌的基类, 定义了卡牌的基本属性和方法。
/// </summary>
public abstract class BaseCard
{
    public abstract string Name { get; protected set; }
    public abstract int ManaCost { get; protected set; }
    public abstract string Description { get; protected set; }
    public abstract string ImagePath { get; protected set; }

    /// <summary>
    /// 使用卡牌的效果
    /// </summary>
    /// <param name="user">使用卡牌的角色。</param>
    /// <param name="target">卡牌的目标角色。</param>
    public abstract void Excute(BaseCharacter user, BaseCharacter target);
}
