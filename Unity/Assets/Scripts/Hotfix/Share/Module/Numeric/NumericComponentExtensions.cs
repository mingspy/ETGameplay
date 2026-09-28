namespace ET
{
    /// <summary>
    /// NumericComponent buff事件和伤害处理扩展。
    /// </summary>
    [FriendOf(typeof (NumericComponent))]
    public static class NumericComponentExtensions
    {
        public static void InitBaseAttributes(this NumericComponent self, int heroConfigId, int level)
        {
            // 示例：基础属性，实际项目读配置表 需要根据英雄configId，获取
            // 这里以一个典型射手为例
            self[NumericType.MaxHp] = 3000 + level * 200;
            self[NumericType.Attack] = 170 + level * 10;
            self[NumericType.Magic] = 0;
            self[NumericType.Armor] = 90 + level * 5;
            self[NumericType.MagicResist] = 50 + level * 3;
            self.Set(NumericType.AttackSpeed,1.0f);
            self[NumericType.Speed] = 360;
            self[NumericType.CritChance] = 0;
            self.Set(NumericType.CritDamagePct, 2.0f); // 默认200%暴击伤害
            //self[NumericType.CritDamagePct]= NumericComponent.ValueAsLong(2.0f); //通过转换设置
            self[NumericType.LifeStealRate] = 0;
            self[NumericType.MagicLifeStealRate] = 0;
            self[NumericType.ArmorFlatPen] = 0;
            self[NumericType.MagicFlatPen] = 0;
            self[NumericType.DamageReduction] = 0;
            self[NumericType.HealReduction] = 0;
            self[NumericType.BonusDamagePercent] = 0;

            self[NumericType.Hp] = self[NumericType.MaxHp];
        }
    }
}