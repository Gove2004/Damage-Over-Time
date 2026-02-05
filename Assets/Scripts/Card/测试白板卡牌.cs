

using UnityEngine;

public class 测试白板卡牌 : BaseCard
{
    public override string Name { get; protected set; } = "Test White Card";
    public override int ManaCost { get; protected set; } = 0;
    public override string Description { get; protected set; } = "No Effect";
    public override string ImagePath { get; protected set; } = "。。。";

    public override void Execute(BaseCharacter user, BaseCharacter target)
    {
        // 无效果
        Debug.Log("测试白板卡牌被使用，但无任何效果。");
    }
}