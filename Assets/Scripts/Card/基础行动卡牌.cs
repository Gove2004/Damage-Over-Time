

public class 基础行动卡牌 : BaseCard
{
    public override string Name { get; protected set; } = "基础行动卡牌";
    public override int ManaCost { get; protected set; } = 1;
    public override string Description { get; protected set; } = "接下来三回合每回合回复1点魔力";
    public override string ImagePath { get; protected set; } = "。。。";

    private int healAmount = 10; // 基础回复值，硬编码

    public override void Excute(BaseCharacter user, BaseCharacter target)
    {
        // 对目标造成伤害
        user.manaBar.AddDot(1, 1);
        user.manaBar.AddDot(2, 1);
        user.manaBar.AddDot(3, 1);
    }
}