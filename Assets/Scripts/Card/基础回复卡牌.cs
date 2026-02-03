

public class 基础回复卡牌 : BaseCard
{
    public override string Name { get; protected set; } = "基础回复卡牌";
    public override int ManaCost { get; protected set; } = 1;
    public override string Description { get; protected set; } = "1回合后，自身回复10点生命。";
    public override string ImagePath { get; protected set; } = "。。。";

    private int healAmount = 10; // 基础回复值，硬编码

    public override void Excute(BaseCharacter user, BaseCharacter target)
    {
        // 对目标造成伤害
        user.healBar.AddDot(1, healAmount);
    }
}