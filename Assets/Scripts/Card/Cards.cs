using UnityEngine;


public class 测试: BaseCard
{
    protected override int id => 1000;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        Debug.Log("测试白板卡牌被使用了！");
    }
}



public class 流血 : BaseCard
{
    protected override int id => 1001;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        var dot = new HPEffect(user, target).SetValue(amount: -10, duration: 3);
        target.dotBar.Add(dot);
    }
}


public class 治疗 : BaseCard
{
    protected override int id => 1002;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        var dot = new HPEffect(user, user).SetValue(amount: +30, delay: 3);
        target.dotBar.Add(dot);
    }
}



public class 入魔 : BaseCard
{
    protected override int id => 1003;

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        var dot = new MPEffect(user, user).SetValue(amount: +1, duration: 3);
        target.dotBar.Add(dot);
    }
}