namespace ET
{
    [EntitySystemOf(typeof(ElementalComponent))]
    [FriendOf(typeof(ElementalComponent))]
    public static partial class ElementalComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ElementalComponent self)
        {
            Log.Info($"ElementalComponent System Awake :  ElementReactionConfig {ElementReactionConfigCategory.Instance.GetAll().Count}"
                +$"  ElementMaterial {ElementMaterialReactionConfigCategory.Instance.GetAll().Count}");
            self.ElementalType = ElementalType.None;
            self.Gauge = 0f;
            self.Duration = 0f;
        }
        
        public static void SetAura(this ElementalComponent self, ElementalType type, float gauge, float duration)
        {
            self.ElementalType = type;
            self.Gauge = gauge;
            self.Duration = duration;
        }

        public static void Consume(this ElementalComponent self, float amount)
        {
            self.Gauge -= amount;
            if (self.Gauge <= 0)
            {
                self.Clear();
            }
        }

        public static void Clear(this ElementalComponent self)
        {
            self.ElementalType = ElementalType.None;
            self.Gauge = 0;
            self.Duration = 0;
        }
        
        public static bool HasAura(this ElementalComponent self)
        {
            return self.ElementalType != ElementalType.None && self.Gauge > 0;
        }
    }
}

