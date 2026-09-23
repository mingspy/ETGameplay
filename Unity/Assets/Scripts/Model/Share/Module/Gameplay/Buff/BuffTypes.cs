using System;

namespace ET
{
    /// <summary>
    /// Buff类型枚举
    /// 王者荣耀的技能被动、装备被动、Buff实际上共用同一套机制，只是来源和触发类型不同
    /// </summary>
    [Flags]
    public enum BuffType
    {
        None = 0,
        /// <summary>
        /// 属性修饰型 - 修改角色属性值（攻击力、防御力、移速等）
        /// 例如：破军被动对半血以下敌人增伤、泣血之刃吸血
        /// </summary>
        AttributeModifer = 1 << 7,
        
        
        /// <summary>
        /// 持续时间型 - 有持续时间，到期自动移除
        /// 例如：妲己被动法术穿透叠加、马超拾取冷晖枪后的增益
        /// </summary>
        Duration = 1<<8,

        /// <summary>
        /// 周期触发型 - 每隔固定时间触发一次效果
        /// 例如：末世的持续伤害？不，末世是命中触发。周期型例如灼烧、中毒
        /// </summary>
        IntervalTick = 3<<8,
        
        /// <summary>
        /// 一次性触发型 - 添加后立即触发效果然后移除
        /// 例如：装备购买时立即获得属性
        /// </summary>
        Instant = ~IntervalTick,

        /// <summary>
        /// 事件触发型 - 监听特定事件触发效果
        /// 例如：鲁班普攻叠被动层数、普攻命中触发装备被动
        /// </summary>
        EventTrigger = 1 << 10,

        /// <summary>
        /// 条件状态型 - 满足特定条件时生效，条件不满足自动失效
        /// 例如：制裁之刃对回血效果的抑制、破军低血量增伤
        /// </summary>
        Conditional = 1<<11,

        /// <summary>
        /// 层数叠加型 - 可叠加层数，每层独立计算效果
        /// 例如：妲己被动最多3层法穿、鲁班扫射被动层数
        /// </summary>
        Stackable = 1<<12,
        
        // TODO: 旧逻辑整合or去掉
        Control = 1 << 13, // 控制状态，分配255个，足够使用了。
        Stunned,  // 眩晕，无法移动，无法释放技能
        Dead, // 直接死亡
        Invincible, // 无敌，不受任何伤害控制和死亡
        Silence, // 沉默， 无法释放技能，可以移动
        Frozen, // 冰冻，无法移动
        SuperArmor, // 霸体（免疫控制）
        Enhanced, // 强化状态
    }

    /// <summary>
    /// Buff来源类型
    /// </summary>
    public enum BuffSourceType
    {
        None = 0,
        /// <summary>
        /// 来自英雄技能被动
        /// </summary>
        HeroPassive = 1,

        /// <summary>
        /// 来自装备被动
        /// </summary>
        Equipment = 2,

        /// <summary>
        /// 来自主动技能效果
        /// </summary>
        Skill = 3,

        /// <summary>
        /// 来自中立生物/环境效果
        /// </summary>
        Environment = 4,

        /// <summary>
        /// 来自Buff效果（红Buff、蓝Buff）
        /// </summary>
        JungleBuff = 5,
    }

    /// <summary>
    /// Buff触发事件类型 - 用于事件驱动机制
    /// </summary>
    public enum BuffEventType
    {
        None = 0,
        /// <summary>
        /// 添加Buff时
        /// </summary>
        OnAdd = 1,

        /// <summary>
        /// 移除Buff时
        /// </summary>
        OnRemove = 2,

        /// <summary>
        /// 普通攻击命中前
        /// </summary>
        BeforeAttackHit = 3,

        /// <summary>
        /// 普通攻击命中后
        /// </summary>
        AfterAttackHit = 4,

        /// <summary>
        /// 技能命中前
        /// </summary>
        BeforeSkillHit = 5,

        /// <summary>
        /// 技能命中后
        /// </summary>
        AfterSkillHit = 6,

        /// <summary>
        /// 造成伤害前
        /// </summary>
        BeforeDealDamage = 7,

        /// <summary>
        /// 造成伤害后
        /// </summary>
        AfterDealDamage = 8,

        /// <summary>
        /// 受到伤害前
        /// </summary>
        BeforeTakeDamage = 9,

        /// <summary>
        /// 受到伤害后
        /// </summary>
        AfterTakeDamage = 10,

        /// <summary>
        /// 击杀单位时
        /// </summary>
        OnKillUnit = 11,

        /// <summary>
        /// 死亡时
        /// </summary>
        OnDead = 12,

        /// <summary>
        /// 移动时
        /// </summary>
        OnMove = 13,

        /// <summary>
        /// 装备变更时
        /// </summary>
        OnEquipmentChanged = 14,

        /// <summary>
        /// 每帧更新 - 用于轮询驱动
        /// </summary>
        OnUpdate = 15,

        /// <summary>
        /// 施放技能时
        /// </summary>
        OnCastSkill = 16,

        /// <summary>
        /// 血量变化时
        /// </summary>
        OnHpChanged = 17,
    }
    
    public interface IBuffEvent
    {
        /// <summary>
        /// 触发事件类型
        /// </summary>
        public BuffEventType EventType { get; set; }
    }
    
    /// <summary>
    /// Buff配置ID常量定义
    /// </summary>
    public static class BuffConfigId
    {
        // 英雄被动
        public const int LubanPassive = 10001;      // 鲁班七号被动-火力压制
        public const int MachaoPassive = 10002;     // 马超被动-魔影突袭
        public const int DajiPassive = 10003;       // 妲己被动-失心

        // 装备被动
        public const int Equipment_Moshi_Passive = 20001;    // 末世-破败
        public const int Equipment_Qixue_Passive = 20002;    // 泣血之刃-吸血
        public const int Equipment_Zhicai_Passive = 20003;   // 制裁之刃-重伤
        public const int Equipment_Pojun_Passive = 20004;    // 破军-破军
    }
}