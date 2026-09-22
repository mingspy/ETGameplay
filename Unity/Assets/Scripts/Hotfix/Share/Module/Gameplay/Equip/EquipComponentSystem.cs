namespace ET
{
    [EntitySystemOf(typeof(EquipComponent))]
    [FriendOf(typeof(EquipComponent))]
    public static partial class EquipComponentSystem
    {
        [EntitySystem]
        private static void Awake(this EquipComponent self)
        {
        }

        /// <summary>
        ///     穿戴装备
        /// </summary>
        public static void WearEquip(this EquipComponent self, EquipConfig config)
        {
            if (self.Equips.TryGetValue(config.Slot, out EquipConfig oldConfig))
            {
                if (config.Id == oldConfig.Id)
                {
                    return;
                }

                self.RemoveEquip(oldConfig.Slot);
            }

            self.Equips[config.Slot] = config;

            NumericComponent numeric = self.GetParent<Unit>().GetComponent<NumericComponent>();

            // 叠加所有装备的基础属性
            for (int i = 0; i < config.Numerics.Length; i++)
            {
                numeric[config.Numerics[i]] += config.NumericValues[i];
            }

            // TODO: 实现附加效果Buff，根据Buff类型，添加监听器。
            BuffComponent buffComponent = self.GetParent<Unit>().GetComponent<BuffComponent>();
            foreach (int buffId in config.BuffIds)
            {
                buffComponent.AddBuff(self.Id, buffId);
            }
        }

        /// <summary>
        ///     脱下装备
        /// </summary>
        public static void RemoveEquip(this EquipComponent self, int slot)
        {
            if (self.Equips.Remove(slot, out EquipConfig config))
            {
                NumericComponent numeric = self.GetParent<Unit>().GetComponent<NumericComponent>();

                // 叠加所有装备的基础属性
                for (int i = 0; i < config.Numerics.Length; i++)
                {
                    numeric[config.Numerics[i]] -= config.NumericValues[i];
                }

                BuffComponent buffComponent = self.GetParent<Unit>().GetComponent<BuffComponent>();
                foreach (int buffId in config.BuffIds)
                {
                    buffComponent.RemoveBuff(buffId);
                }
            }
        }
    }
}