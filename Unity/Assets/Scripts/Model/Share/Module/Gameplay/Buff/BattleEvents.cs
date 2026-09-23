namespace ET
{
    /// <summary>
    /// 攻击命中事件参数
    /// </summary>
    public struct AttackHitEvent : IBuffEvent
    {
        public BuffEventType EventType { get; set; }

        /// <summary>
        /// 攻击者ID
        /// </summary>
        public long AttackerId;

        /// <summary>
        /// 被攻击者ID
        /// </summary>
        public long TargetId;

        /// <summary>
        /// 基础伤害值
        /// </summary>
        public float Damage;

        /// <summary>
        /// 是否是强化普攻（鲁班被动）
        /// </summary>
        public bool IsEnhancedAttack;

        /// <summary>
        /// 是否暴击
        /// </summary>
        public bool IsCritical;
    }

    /// <summary>
    /// 技能命中事件参数
    /// </summary>
    public struct SkillHitEvent : IBuffEvent
    {
        public BuffEventType EventType { get; set; }

        /// <summary>
        /// 施法者ID
        /// </summary>
        public long CasterId;

        /// <summary>
        /// 目标ID
        /// </summary>
        public long TargetId;

        /// <summary>
        /// 技能ID
        /// </summary>
        public int SkillId;

        /// <summary>
        /// 技能等级
        /// </summary>
        public int SkillLevel;

        /// <summary>
        /// 基础伤害
        /// </summary>
        public float Damage;
    }

    /// <summary>
    /// 伤害事件参数 - 用于造成伤害/受到伤害时
    /// </summary>
    public struct DamageEvent : IBuffEvent
    {
        public BuffEventType EventType { get; set; }

        /// <summary>
        /// 伤害来源ID
        /// </summary>
        public long SourceId;

        /// <summary>
        /// 受伤目标ID
        /// </summary>
        public long TargetId;

        /// <summary>
        /// 最终伤害值（经过Buff修改后）
        /// </summary>
        public float FinalDamage;

        /// <summary>
        /// 原始伤害值
        /// </summary>
        public float OriginalDamage;

        /// <summary>
        /// 伤害来源类型（普攻/技能/装备/Buff）
        /// </summary>
        public int DamageSourceType;

        /// <summary>
        /// <see cref="ET.DamageType"/>
        /// </summary>
        public int DamageType;
    }

    /// <summary>
    /// 治疗事件参数
    /// </summary>
    public struct HealEvent : IBuffEvent
    {
        public BuffEventType EventType { get; set; }

        /// <summary>
        /// 治疗来源ID
        /// </summary>
        public long SourceId;

        /// <summary>
        /// 被治疗目标ID
        /// </summary>
        public long TargetId;

        /// <summary>
        /// 治疗量
        /// </summary>
        public float HealAmount;

        /// <summary>
        /// 治疗来源类型
        /// </summary>
        public int HealSourceType;
    }

    /// <summary>
    /// 施法事件参数
    /// </summary>
    public struct CastSkillEvent : IBuffEvent
    {
        public BuffEventType EventType { get; set; }

        /// <summary>
        /// 施法者ID
        /// </summary>
        public long CasterId;

        /// <summary>
        /// 技能ID
        /// </summary>
        public int SkillId;

        /// <summary>
        /// 技能等级
        /// </summary>
        public int SkillLevel;
    }

    /// <summary>
    /// 单位死亡事件
    /// </summary>
    public struct UnitDeadEvent : IBuffEvent
    {
        public BuffEventType EventType { get; set; }

        /// <summary>
        /// 死亡单位ID
        /// </summary>
        public long DeadUnitId;

        /// <summary>
        /// 击杀者ID
        /// </summary>
        public long KillerId;
    }

    /// <summary>
    /// 装备变更事件
    /// </summary>
    public struct EquipmentChangedEvent : IBuffEvent
    {
        public BuffEventType EventType { get; set; }

        /// <summary>
        /// 单位ID
        /// </summary>
        public long UnitId;

        /// <summary>
        /// 装备ID，0表示出售/卸下
        /// </summary>
        public int EquipId;

        /// <summary>
        /// 是否是添加装备
        /// </summary>
        public bool IsAdd;
    }

    /// <summary>
    /// 血量变化事件
    /// </summary>
    public struct HpChangedEvent : IBuffEvent
    {
        public BuffEventType EventType { get; set; }

        /// <summary>
        /// 单位ID
        /// </summary>
        public long UnitId;

        /// <summary>
        /// 变化前血量
        /// </summary>
        public float OldHp;

        /// <summary>
        /// 变化后血量
        /// </summary>
        public float NewHp;

        /// <summary>
        /// 变化量
        /// </summary>
        public float Delta;
    }
}