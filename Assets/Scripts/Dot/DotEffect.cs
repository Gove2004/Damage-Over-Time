using UnityEngine;

public abstract class DotEffect
{
    // 源
    public BaseCharacter source;
    public BaseCharacter target;
    // 数值
    public float amount = 0;  // 默认的数值字段，例如伤害
    public int delay = 0;  // 延迟生效回合数，例如蓄力
    public int interval = 1;  // 间隔回合数，例如每隔几回合生效一次
    public int timer = 0;  // 当前间隔计数
    public int duration = 1;  // 持续回合数

    public DotEffect(BaseCharacter s, BaseCharacter t)
    {
        source = s;
        target = t;
    }

    public DotEffect SetValue(float amount = 0, int delay = 0, int interval = 1, int timer = 0, int duration = 0)
    {
        this.amount = amount;
        this.delay = delay;
        this.interval = interval;
        this.timer = timer;
        this.duration = duration;
        return this;
    }


    public void Apply()
    {
        if (delay > 0)
        {
            delay--;
            return;
        }

        if (timer < interval - 1)
        {
            timer++;
            return;
        }

        // 应用效果
        OnApply();

        // 重置计时器
        timer = 0;

        // 减少持续时间
        duration--;
        if (duration <= 0)
        {
            OnExpire();

            // 从目标的DOT列表中移除自己
            target.dotBar.Remove(this);
        }
    }


    // 具体效果实现由子类重写
    protected abstract void OnApply();

    protected virtual void OnExpire()
    {
        // 可选的过期处理
    }
}
