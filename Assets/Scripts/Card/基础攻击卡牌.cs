

public class 基础攻击卡牌 : BaseCard
{
    public override string Name { get; protected set; } = "基础攻击卡牌";
    public override int ManaCost { get; protected set; } = 1;
    public override string Description { get; protected set; } = "接下来三回合，每回合对敌人造成10点伤害";
    public override string ImagePath { get; protected set; } = "。。。";

    private int damageAmount = 10; // 基础伤害值，硬编码

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        target.AddDot(new HPEffect(user, target).SetValue(amount: -10, duration: 3));
        
    }
}