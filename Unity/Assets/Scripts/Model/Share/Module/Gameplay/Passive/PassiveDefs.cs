namespace ET
{

    // 被动触发时机枚举，覆盖游戏全流程节点
    public enum PassiveType
    {
        None = 0,
        // 战斗类触发点
        OnBeforeDamage,      // 造成伤害前
        OnAfterDamage,       // 造成伤害后
        OnTakeDamage,        // 受到伤害时
        OnKillUnit,          // 击杀单位时
        OnElementReaction,   // 触发元素反应时
        OnCrit,              // 暴击时
        OnSkillCast,         // 释放技能时
        OnBuffAdd,           // 获得Buff时
        OnNormalAttackHit, // 普攻命中目标时触发（攻击者侧）
        OnBeAttacked,      // 被普攻命中时触发（目标侧）
        // 状态类触发点
        OnHpChange,          // 生命值变化时
        OnHpBelowThreshold, // 生命值低于阈值时
        OnEnterBattle,      // 进入战斗时
        OnLeaveBattle,      // 脱离战斗时
        OnMove,             // 移动时
        OnTimer,            // 定时触发（每N秒触发一次）

    }
    

    public abstract class PassiveDataBase: ETObject
    {
        public int PassiveId { get; set; }
        public PassiveType TriggerType { get; set; }
        /// <summary>
        /// 被动优先级，值越大越先触发
        /// </summary>
        public int Priority { get; set; }
        
        /// <summary>
        /// 冷却毫秒数
        /// </summary>
        public int Cooldown  { get; set; }
        public long LastTriggerTime  { get; set; }
        public bool IsEnabled { get; set; } = true;
    }

    public interface IPassiveEvent
    {
        public PassiveType TriggerType { get; }
    }
    
}