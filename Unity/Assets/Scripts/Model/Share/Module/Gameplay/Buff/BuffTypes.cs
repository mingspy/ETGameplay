using System;

namespace ET
{
    /// <summary>
    ///     Buff类型枚举
    ///     王者荣耀的技能被动、装备被动、Buff实际上共用同一套机制，只是来源和触发类型不同
    /// </summary>
    [Flags]
    public enum BuffType
    {
        None = 0,

        /// <summary>
        ///     属性修饰型 - 修改角色属性值（攻击力、防御力、移速等）
        ///     例如：破军被动对半血以下敌人增伤、泣血之刃吸血
        /// </summary>
        AttributeModifer = 1 << 7,

        /// <summary>
        ///     持续时间型 - 有持续时间，到期自动移除，如果buff的持续时间设置成0，则是永久buff。
        ///     例如：妲己被动法术穿透叠加、马超拾取冷晖枪后的增益
        /// </summary>
        Duration = 1 << 8,

        /// <summary>
        ///     周期触发型 - 每隔固定时间触发一次效果
        ///     例如：末世的持续伤害？不，末世是命中触发。周期型例如灼烧、中毒
        /// </summary>
        IntervalTick = 3 << 8,

        /// <summary>
        ///     一次性触发型 - 添加后立即触发效果然后移除
        ///     例如：装备购买时立即获得属性
        /// </summary>
        Instant = ~IntervalTick,

        /// <summary>
        ///     事件触发型 - 监听特定事件触发效果
        ///     例如：鲁班普攻叠被动层数、普攻命中触发装备被动
        /// </summary>
        EventTrigger = 1 << 10,
        Passive = Duration|EventTrigger,

        /// <summary>
        ///     条件状态型 - 满足特定条件时生效，条件不满足自动失效
        ///     例如：制裁之刃对回血效果的抑制、破军低血量增伤
        /// </summary>
        Conditional = 1 << 11,

        /// <summary>
        ///     层数叠加型 - 可叠加层数，每层独立计算效果
        ///     例如：妲己被动最多3层法穿、鲁班扫射被动层数
        /// </summary>
        Stackable = 1 << 12,

        // TODO: 旧逻辑整合or去掉
        Control = 1 << 13, // 控制状态，分配255个，足够使用了。
        Stunned, // 眩晕，无法移动，无法释放技能
        Dead, // 直接死亡
        Invincible, // 无敌，不受任何伤害控制和死亡
        Silence, // 沉默， 无法释放技能，可以移动
        Frozen, // 冰冻，无法移动
        SuperArmor, // 霸体（免疫控制）
        Enhanced // 强化状态
    }

    /// <summary>
    ///     Buff来源类型
    /// </summary>
    public enum BuffSourceType
    {
        None = 0,

        /// <summary>
        ///     来自英雄技能被动
        /// </summary>
        HeroPassive = 1,

        /// <summary>
        ///     来自装备被动
        /// </summary>
        Equipment = 2,

        /// <summary>
        ///     来自主动技能效果
        /// </summary>
        Skill = 3,

        /// <summary>
        ///     来自中立生物/环境效果
        /// </summary>
        Environment = 4,

        /// <summary>
        ///     来自Buff效果（红Buff、蓝Buff）
        /// </summary>
        JungleBuff = 5
    }
}