public class StatModifier
{
    private const int fixedPercentBonus = 1;
    private float flatBonus;
    private float percentBonus = fixedPercentBonus;

    public void AddFlat(float value) => flatBonus += value;
    public void AddPercent(float value) => percentBonus += value;
    public void Reset() { flatBonus = 0; percentBonus = 1; }

    public float Calculate(float baseValue)
    {
        return (baseValue + flatBonus) * percentBonus;
    }

    public float CalculateForPercentStat(float baseValue)
    {
        return baseValue + percentBonus - fixedPercentBonus;
    }
} 