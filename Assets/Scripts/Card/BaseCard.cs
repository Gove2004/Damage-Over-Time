

/// <summary>
/// 卡牌的基类, 定义了卡牌的基本属性和方法。
/// </summary>
public abstract class BaseCard
{
    protected abstract int id { get; }


    public BaseCard()
    {
        LoadCardData();
    }
    
    private void LoadCardData()
    {
        // 从数据库获取卡牌数据
        var data = CardDatabase.GetCardData(id);
        
        if (data != null)
        {
            Name = data.name;
            Cost = data.cost;
            Value = data.value;
            Duration = data.duration;
            Description = data.effect;
            ImagePath = data.imagePath;
        }
        else
        {
            // 设置默认值
            Name = "未知卡牌";
            Cost = 0;
            Value = 0;
            Duration = 0;
            Description = "无效果";
            ImagePath = "卡牌/default";
        }
    }
    
    public string Name { get; private set; }
    public int Cost { get; private set; }
    public int Value { get; private set; }
    public int Duration { get; private set; }
    public string Description { get; private set; }
    public string ImagePath { get; private set; }

    /// <summary>
    /// 使用卡牌的效果
    /// </summary>
    /// <param name="user">使用卡牌的角色。</param>
    /// <param name="target">卡牌的目标角色。</param>
    public abstract void Execute(BaseCharacter user, BaseCharacter target);
}
