using UnityEngine;

public class DotBar
{
    public const int MaxDelayTurn = 10;
    private float[] dots = new float[MaxDelayTurn];

    private void IsValidDelayTurn(int delayTurn)
    {
        if (delayTurn < 1 || delayTurn > MaxDelayTurn)
        {
            Debug.LogError("延迟回合数超出范围: " + delayTurn + ". 有效范围是1到" + MaxDelayTurn);
        }
    }

    public void StepTurn()
    {
        // 将所有DOT值向前移动一位
        for (int i = 0; i < MaxDelayTurn - 1; i++)
        {
            dots[i] = dots[i + 1];
        }
        // 最后一位清零
        dots[MaxDelayTurn - 1] = 0;
    }

    public void AddDot(int delayTurn, float dot)
    {
        IsValidDelayTurn(delayTurn);

        dots[delayTurn - 1] += dot;
    }

    public void SetDot(int delayTurn, float dot)
    {
        IsValidDelayTurn(delayTurn);

        dots[delayTurn - 1] = dot;
    }

    public void ClearDot(int delayTurn)
    {
        IsValidDelayTurn(delayTurn);

        dots[delayTurn - 1] = 0;
    }

    public float GetDot(int delayTurn)
    {
        IsValidDelayTurn(delayTurn);

        return dots[delayTurn - 1];
    }

    public override string ToString()
    {
        string result = "";
        for (int i = 0; i < MaxDelayTurn; i++)
        {
            result += $"{dots[i]} ";
        }
        return result;
    }
}
