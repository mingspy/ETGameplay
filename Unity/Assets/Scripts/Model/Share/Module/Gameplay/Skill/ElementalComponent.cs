using System.Collections.Generic;
using MemoryPack;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    /// <summary>
    /// 单位身上的元素附着状态管理组件。<br/>
    /// 元素反应系统参考塞尔达传说和原神，两者结合起来使用。
    /// 塞尔达主要是把元素用于材质，元素对材质产生物理效果，用于视觉和特性的改变上，一般不产生伤害。
    /// 原神实现了几个主要的元素反应，主要是用于伤害系数的扩大和相克。<br/>
    /// 此组件实现主要功能包括：<br/>
    /// - 材质和元素的反应及产生状态变化，加成伤害。实现方法是两个材质属性，一个主材质，一个表层材质。比如角色穿了金属装甲，下雨时表面材质会积水。<br/>
    /// - 元素与元素的反应及产生变化，加成伤害，元素消耗。元素附着在角色(玩家，NPC，场景物体)，主要用于和其他元素发生反应，元素可以被逐渐消耗或者到期就消失。<br/>
    /// - 角色身上的元素主要来自场景或者其他场景的释放，技能攻击可以附带元素伤害，如果目标角色身上没有可以反应的元素，则被附着到目标身上（一般持续3秒左右），
    /// 下次不同元素攻击可以产生元素伤害。这样就能实现团队配合产生Combo伤害，也能支持辅助使用技能解除元素附着。<br/>
    /// - 为了简化，元素与元素的反应消耗量都是1：1，产生相应数量的伤害。
    /// 比如技能附带50单位🔥元素，目标身上100单位🪵元素，那么直接消耗掉50单位木元素，参数50单位伤害，技能释放只消耗魔法，不消耗释放者身上的元素。
    /// 当然火攻击木头可以给木头身上继续挂上燃烧buff，每秒消耗1单位木头产生1点伤害。<br/>
    /// - 元素克制，通过伤害放大系数DamageMultiply系数控制，如果>1则是放大伤害，如果是小于1，则是降低伤害，相当于防御。<br/>
    /// </summary>
    [ComponentOf(typeof (Unit))]
    public class ElementalComponent : Entity, IAwake<MaterialType>, IDestroy, IUpdate
    {
        [MemoryPackIgnore]
        [BsonIgnore]
        public Unit Unit {get; set;}
        
        // ========== 元素附着管理 ==========
        /// <summary>
        /// 当前激活的元素列表（支持多元素共存）
        /// </summary>
        public List<ElementalAttachment> ActiveElements = new(4);
        

        // ========== 材质状态 ==========
        /// <summary>
        /// 基础材质（物体固有材质）
        /// </summary>
        public ElementalAttachment BaseMaterial;
        
        /// <summary>
        /// 表面材质覆盖（临时状态，如淋湿、结冰）
        /// </summary>
        public ElementalAttachment SurfaceMaterial;
        
    }
}

