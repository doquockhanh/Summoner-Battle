public static class UnitEvents
{
    public static class Combat
    {
        public delegate void DamageHandler(Unit source, Unit target, float amount);
        public static event DamageHandler OnDamageDealt;

        public static event System.Action<Unit, Unit> OnDeath;
        
        public static void RaiseDamageDealt(Unit source, Unit target, float amount)
        {
            OnDamageDealt?.Invoke(source, target, amount);
        }

        public static void RaiseDeath(Unit source, Unit target) {
            OnDeath?.Invoke(source, target);
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

        public delegate void SoulAbsorbHandler(Unit absorber, Unit soul);
        public static event SoulAbsorbHandler OnSoulAbsorbed;
        
        public static void RaiseSoulAbsorbed(Unit absorber, Unit soul)
        {
            OnSoulAbsorbed?.Invoke(absorber, soul);
        }

        public delegate void SkillActivateHandler(Unit unit, Skill skill);
        public static event SkillActivateHandler OnSkillActivated;
        
        public static void RaiseSkillActivated(Unit unit, Skill skill)
        {
            OnSkillActivated?.Invoke(unit, skill);
        }
    }
} 