using System;

public class Dot
{
    public BaseCharacter source;
    public BaseCharacter target;
    public int duration;
    private readonly Action<Dot> onTick;
    private readonly Action<Dot> onExpire;

        public readonly Func<string> description;  // 用于界面显示，实时生成

        public Dot(BaseCharacter s, BaseCharacter t, int duration, Action<Dot> onTick, Action<Dot> onExpire = null, Func<string> description = null)
        {
            source = s;
            target = t;
            this.duration = duration;
            this.onTick = onTick;
            this.onExpire = onExpire;

            this.description = description ?? (() => "");
        }

    public void Apply()
    {
        onTick?.Invoke(this);
        duration--;
        if (duration <= 0)
        {
            onExpire?.Invoke(this);
            if (source != null)
            {
                source.dotBar.Remove(this);
            }
            else if (target != null)
            {
                target.dotBar.Remove(this);
            }
        }
    }
}
