using System.Collections.Generic;

namespace ET
{
    /// <summary>
    ///     超载：火雷反应，额外范围伤害+小击退
    /// </summary>
    public struct OverloadEvent
    {
        public Unit Target { get; set; }
        public float KnockbackDistance { get; set; }
        public float AoeRadius { get; set; }
        public float Damage { get; set; }
    }

    /// <summary>
    ///     超导：冰雷反应，雷元素攻击冰属性，减雷抗
    /// </summary>
    public struct SuperConductEvent
    {
        public Unit Target { get; set; }
        public float ResistanceReduction { get; set; }
        public long DurationMS { get; set; }
    }

    public struct SwirlEvent
    {
        public Unit Caster { get; set; }
        public ElementType SpreadElement { get; set; }
        public float Radius { get; set; }
        public List<long> AffectedUnits { get; set; }
    }

    /// <summary>
    ///     感电：雷水反应，麻痹+持续伤害
    /// </summary>
    public struct ElectroChargedEvent
    {
        public Unit Target { get; set; }
        public float ResistanceReduction { get; set; }
        public long DurationMS { get; set; }
    }

    /// <summary>
    ///     派发冻结事件（UI/特效/被动）
    /// </summary>
    public struct FrozenEvent
    {
        public Unit Target { get; set; }

        /// <summary>
        ///     冻结时间，单位毫秒
        /// </summary>
        public long DurationMS { get; set; }
    }

    /// <summary>
    ///     区分冰冻的原因是，冰冻可以有元素反应。
    /// </summary>
    public struct StunnedEvent
    {
        public Unit Target { get; set; }

        /// <summary>
        ///     冻结时间，单位毫秒
        /// </summary>
        public long DurationMS { get; set; }
    }
}