using System;

public class Dot
{
    public BaseCharacter source;
    public BaseCharacter target;
    public int duration;
    private readonly Action<Dot> onTick;
    private readonly Action<Dot> onExpire;

    public Dot(BaseCharacter s, BaseCharacter t, int duration, Action<Dot> onTick, Action<Dot> onExpire = null)
    {
        source = s;
        target = t;
        this.duration = duration;
        this.onTick = onTick;
        this.onExpire = onExpire;
    }

    public void Apply()
    {
        onTick?.Invoke(this);
        duration--;
        if (duration <= 0)
        {
            onExpire?.Invoke(this);
            target.dotBar.Remove(this);
        }
    }
}
