

public class 基础攻击卡牌 : BaseCard
{
    public override string Name { get; protected set; } = "基础攻击卡牌";
    public override int ManaCost { get; protected set; } = 1;
    public override string Description { get; protected set; } = "1回合后，对目标造成10点伤害。";
    public override string ImagePath { get; protected set; } = "。。。";

    private int damageAmount = 10; // 基础伤害值，硬编码

    public override void Excute(BaseCharacter user, BaseCharacter target)
    {
        // 对目标造成伤害
        target.damageBar.AddDot(1, damageAmount);
    }
}