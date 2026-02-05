


public class MPEffect : DotEffect
{
    public MPEffect(BaseCharacter s, BaseCharacter t) : base(s, t)
    {
    }

    protected override void OnApply()
    {
        target.ChangeMana((int)amount);
    }
}