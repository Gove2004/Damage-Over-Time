

public class 基础行动卡牌 : BaseCard
{
    public override string Name { get; protected set; } = "基础行动卡牌";
    public override int ManaCost { get; protected set; } = 1;
    public override string Description { get; protected set; } = "接下来三回合, 每回合额外回复1点魔力";
    public override string ImagePath { get; protected set; } = "。。。";

    private int healAmount = 10; // 基础回复值，硬编码

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        user.AddDot(new MPEffect(user, user).SetValue(amount: 1, duration: 3));
    }
}