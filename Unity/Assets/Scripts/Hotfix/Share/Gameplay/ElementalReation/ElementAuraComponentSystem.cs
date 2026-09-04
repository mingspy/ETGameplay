namespace ET.Gameplay
{
    [EntitySystemOf(typeof(ElementAuraComponent))]
    [FriendOf(typeof(ElementAuraComponent))]
    public static partial class ElementAuraComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ElementAuraComponent self)
        {
            self.ElementType = ElementType.None;
            self.Gauge = 0f;
            self.Duration = 0f;
        }
        
        public static void SetAura(this ElementAuraComponent self, ElementType type, float gauge, float duration)
        {
            self.ElementType = type;
            self.Gauge = gauge;
            self.Duration = duration;
        }

        public static void Consume(this ElementAuraComponent self, float amount)
        {
            self.Gauge -= amount;
            if (self.Gauge <= 0)
            {
                self.Clear();
            }
        }

        public static void Clear(this ElementAuraComponent self)
        {
            self.ElementType = ElementType.None;
            self.Gauge = 0;
            self.Duration = 0;
        }
        
        public static bool HasAura(this ElementAuraComponent self)
        {
            return self.ElementType != ElementType.None && self.Gauge > 0;
        }
    }
}

