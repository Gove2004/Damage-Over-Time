

public class 基础回复卡牌 : BaseCard
{
    public override string Name { get; protected set; } = "基础回复卡牌";
    public override int ManaCost { get; protected set; } = 1;
    public override string Description { get; protected set; } = "三回合后回复30点生命";
    public override string ImagePath { get; protected set; } = "。。。";

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        user.AddDot(new HPEffect(user, user).SetValue(delay: 3, amount: 30));
    }
}