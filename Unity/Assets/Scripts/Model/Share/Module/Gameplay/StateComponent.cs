using System;

namespace ET
{
    [Flags]
    public enum State
    {
        Idle = 0, // 空闲
        Moving = 1 << 0, // 移动
        Attacking = 1 << 1, // 普通攻击 Atk1-N
        Casting = 1 << 2, // 释放技能 Spell0-N
        Stunned = 1 << 3, // 被击中（硬直), 无法移动，无法释放魔法
        Dead = 1 << 4, // 死亡
        Invincible = 1 << 5, // 无敌
        Silence = 1 << 6,  // 沉默：无法释放魔法
        Frozen = 1 << 7,
        SuperArmor = 1 << 8, // 霸体（免疫控制）
        Enhanced = 1 << 9, // 强化状态
        IsCasting = Attacking | Casting,
        Abnormal = Stunned | Dead
        
    }
    /// <summary>
    /// 管理用户状态，直接单独设置
    /// </summary>
    [ComponentOf(typeof(Unit))]
    public class StateComponent: Entity, IAwake
    {
        public State State {get;set;}
    }
    
    /// <summary>
    /// 派发冻结事件（UI/特效/被动）
    /// </summary>
    public struct FrozenEvent
    {
        public Unit Target { get; set; }
        /// <summary>
        /// 冻结时间，单位秒
        /// </summary>
        public float Duration { get; set; }
    }
    
    /// <summary>
    /// 区分冰冻的原因是，冰冻可以有元素反应。
    /// </summary>
    public struct StunnedEvent
    {
        public Unit Target { get; set; }
        /// <summary>
        /// 冻结时间，单位秒
        /// </summary>
        public float Duration { get; set; }
    }
}