public static class UnitEvents
{
    public static class Combat
    {
        public delegate void DamageHandler(Unit source, Unit target, float amount);
        public static event DamageHandler OnDamageDealt;
        
        public static void RaiseDamageDealt(Unit source, Unit target, float amount)
        {
            OnDamageDealt?.Invoke(source, target, amount);
        }
    }
    
    public static class Status
    {
        public delegate void UnitStatusHandler(Unit unit);
        public static event UnitStatusHandler OnUnitDeath;
        
        public static void RaiseUnitDeath(Unit unit)
        {
            OnUnitDeath?.Invoke(unit);
        }
    }
} 