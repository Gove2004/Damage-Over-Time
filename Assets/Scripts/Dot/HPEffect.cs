


public class HPEffect : DotEffect
{
    public HPEffect(BaseCharacter s, BaseCharacter t) : base(s, t)
    {
    }

    protected override void OnApply()
    {
        target.ChangeHealth((int)amount);
    }
}