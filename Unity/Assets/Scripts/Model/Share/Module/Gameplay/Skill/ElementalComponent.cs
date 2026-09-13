using System.Collections.Generic;
using MemoryPack;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    /// <summary>
    ///     单位身上的元素附着状态管理组件。<br />
    ///     元素反应系统参考塞尔达传说和原神，两者结合起来使用。
    ///     塞尔达主要是把元素用于材质，元素对材质产生物理效果，用于视觉和特性的改变上，一般不产生伤害。
    ///     原神实现了几个主要的元素反应，主要是用于伤害系数的扩大和相克。<br />
    ///     此组件实现主要功能包括：<br />
    ///     - 材质和元素的反应，加成伤害，并可以变化材质属性，造成持续效果。实现方法是主材质+表层材质，比如角色穿了金属装甲，下雨时表面材质会积水。<br />
    ///     - 元素与元素的反应，加成伤害，元素消耗，可以产生新元素，一般不造成持续效果。元素可以被逐渐消耗或者到期就消失。<br />
    ///     - 角色身上的元素主要来自场景、天赋，技能攻击可以附带元素伤害，如果目标角色身上没有可以反应的元素，则被附着到目标身上（一般持续3秒左右），
    ///     下次不同元素攻击可以产生元素伤害。这样就能实现团队配合产生Combo伤害，也能支持辅助使用技能解除元素附着。<br />
    ///     - 为了简化，元素与元素的反应消耗量都是1：1，产生相应数量的伤害。
    ///     比如技能附带50单位🔥元素，目标身上100单位🪵元素，那么直接消耗掉50单位木元素，产生50单位伤害，技能释放只消耗魔法，不消耗释放者身上的元素。<br />
    ///     - 元素克制，通过伤害放大系数DamageMultiply系数控制，如果>1则是放大伤害，如果是小于1，则是降低伤害，相当于防御。<br />
    ///     - 为了简化，元素与材质反应可产生持续和传导效果，可以传导到自身角色和周围单位，传导自身时可以消耗元素，传导周围时只在材质上传导。<br />
    ///     - 元素与角色附带的元素反应，大部分情况只产生一次性伤害，比如火元素可以点燃木材质，但是只会消耗掉角色身上附带的木元素，不产生持续燃烧和传导。
    ///     技术上可以实现元素的持续反应和传导，但是从感官上，角色身上挂载的元素量一般比较少且时间短。（TODO:待定是否支持元素与元素传导，在配置上控制）。<br />
    ///     - 优先计算材质反应，然后计算附着元素，一次攻击只产生一次效果。<br />
    /// </summary>
    [ComponentOf(typeof(Unit))]
    public class ElementalComponent : Entity, IAwake<MaterialType>, IDestroy, IUpdate
    {
        // ========== 元素附着管理 ==========
        /// <summary>
        ///     当前激活的元素列表（支持多元素共存）
        /// </summary>
        public List<Elemental> AttachedElements = new(4);

        // ========== 材质状态 ==========
        /// <summary>
        ///     基础材质（物体固有材质）
        /// </summary>
        public Elemental BaseMaterial = new Elemental{AffixType = ElementalType.Material};

        /// <summary>
        ///     表面材质覆盖（临时状态，如淋湿、结冰）
        /// </summary>
        public Elemental SurfaceMaterial =  new Elemental{AffixType = ElementalType.Material};
        

        /// <summary>
        ///     元素连锁反应最大深度（防止团战无限连锁导致性能问题/数值溢出）
        /// </summary>
        public int MaxReactionDepth { get; set; } = 3;

        /// <summary>
        ///     扩散元素强度衰减系数
        /// </summary>
        public float SpreadIntensityDecay { get; set; } = 0.7f;

        /// <summary>
        ///     扩散默认范围（码）
        /// </summary>
        public float DefaultSpreadRadius { get; set; } = 3.5f;
    }
    /// <summary>
    /// 超载：火雷反应，额外范围伤害+小击退
    /// </summary>
    public struct OverloadEvent
    {
        public Unit Target { get; set; }
        public float KnockbackDistance { get; set; }
        public float AoeRadius { get; set; }
        public float Damage { get; set; }
    }

    /// <summary>
    /// 超导：冰雷反应，雷元素攻击冰属性，减雷抗
    /// </summary>
    public struct SuperConductEvent
    {
        public Unit Target { get; set; }
        public float ResistanceReduction  { get; set; }
        public float Duration { get; set; }
    }

    public struct SwirlEvent
    {
        public Unit Caster  { get; set; }
        public ElementType SpreadElement  { get; set; }
        public float Radius { get; set; } 
        public List<long> AffectedUnits{ get; set; }
    }
}