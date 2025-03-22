public class StatsCalculator
{
    public static float CalculateFinalDamage(float rawDamage, DamageType damageType, IStats stats)
    {
        float damage = rawDamage;

        if (damageType == DamageType.Physical)
        {
            float armor = stats.GetArmor();
            damage *= 100f / (100f + armor);
        }
        else if (damageType == DamageType.Magic)
        {
            float magicResist = stats.GetMagicResist();
            damage *= 100f / (100f + magicResist);
        }

        // Áp dụng giảm sát thương chung
        float reduction = stats.GetDamageReduction();
        damage *= (100f - reduction) / 100f;

        return damage;
    }

    public static float CalculateHealing(float amount, IStats stats)
    {
        float healingReceived = stats.GetHealingReceived();
        return amount * (healingReceived / 100f);
    }

    public static float CalculateLifeSteal(float damage, IStats stats)
    {
        float lifeSteal = stats.GetLifeSteal();
        return damage * (lifeSteal / 100f);
    }
} 