namespace ET;

[EntitySystemOf(typeof(EquipComponent))]
[FriendOf(typeof(EquipComponent))]
public static partial class EquipComponentSystem
{
    [EntitySystem]
    private static void Awake(this EquipComponent self)
    {
        self.Unit = self.GetParent<Unit>();
    }

    /// 穿戴装备
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

        NumericComponent numeric = self.Unit.GetComponent<NumericComponent>();

        // 叠加所有装备的基础属性
        for (int i = 0; i < config.Numerics.Length; i++)
        {
            numeric[config.Numerics[i]] += config.EffectValues[i];
        }

        // TODO: 实现附加效果Buff，根据Buff类型，添加监听器。
    }

    /// 脱下装备
    /// </summary>
    public static void RemoveEquip(this EquipComponent self, int slot)
    {
        if (self.Equips.TryGetValue(slot, out EquipConfig config))
        {
            self.Equips.Remove(slot);
            NumericComponent numeric = self.Unit.GetComponent<NumericComponent>();

            // 叠加所有装备的基础属性
            for (int i = 0; i < config.Numerics.Length; i++)
            {
                numeric[config.Numerics[i]] -= config.EffectValues[i];
            }
        }
    }
}