using System;

namespace ET
{
    /// <summary>
    ///     伤害类型
    /// </summary>
    public enum DamageType
    {
        True = 0, // 真伤
        Physical = 1, // 物理伤害
        Magical = 2, // 法术伤害
        Metal = 3, // 金 
        Wood = 4, // 木 
        Water = 5, // 水
        Fire = 6, // 火
        Earth = 7, // 土/地
        Wind = 8, // 风
        Lightning = 9, // 雷 / 电 
        Light = 10, // 光 / 圣
        Dark = 11, // 暗 / 邪
        Poison = 12, // 毒 (或 Toxin)
        Oil = 13, //  油，易燃，易爆 
        Ice = 14 // 冰
    }

    public static class DamageTypeHelper
    {
        /// <summary>
        ///     是否元素伤害
        /// </summary>
        /// <param name="damageType"></param>
        /// <returns></returns>
        public static bool IsElementDamage(DamageType damageType)
        {
            return damageType >= DamageType.Metal && damageType <= DamageType.Ice;
        }

        public static ElementType ToElement(DamageType damageType)
        {
            int value = (int)damageType;
            return (ElementType)value;
        }

        public static bool Equal(DamageType dtype, int damageType)
        {
            return (DamageType)damageType == dtype;
        }
    }

    public enum DamageTriggerType
    {
        /// <summary>
        ///     瞬时伤害
        /// </summary>
        Immediate = 0,

        /// <summary>
        ///     延迟伤害
        /// </summary>
        Delay = 1,

        /// <summary>
        ///     每帧都执行，持续伤害
        /// </summary>
        OverTime = 2
    }

    public enum SkillType
    {
        Common = 0, // 普攻
        Skill = 1, // 主动技能
        Passive = 2, // 被动
        EquipPassive = 3 // 装备被动
    }

    /// <summary>
    ///     伤害管线阶段事件
    ///     命名规范：Modify*=可写管道钩子, On*/After*=只读通知
    ///     视角：Outgoing=攻击方, Incoming=受击方
    /// </summary>
    [Flags]
    public enum DamageStage
    {
        None = 0,

        // 以下的读写限制是指 DamageContext 属性
        OnSkillCast = 1 << 0,
        // ===== 计算阶段（可写） =====
        BeforeHit = 1 << 1, // 技能命中前，技能已经释放，如鲁班普攻前，判定是否强化普攻
        BeforeCalcRawDamage = 1<< 2, // Step1
        GatherCriticalRoll = 1<< 3, // Step2
        CalcElement = 1<< 4, // Step3.0 计算元素伤害
        GatherBonusDamage = 1<< 5, // Step3  末世在此生成法球
        GatherDamageMitigation = 1<< 6, // Step4
        GatherOutgoingDamage = 1<< 7, // Step5  ★ 破军/暴烈之甲在此施加增伤
        GatherIncomingDamage = 1<< 8, // Step6
        JudgeDamageNegation = 1<< 9, // Step7  名刀/纯净苍穹
        JudgeExecute = 1<< 10, // Step8  斩杀判定
        BeforeLifeSteal = 1<< 11, // 吸血装备

        // ===== 通知阶段（只读） =====
        AfterHit= 1<< 12,  // 技能命中后事件，如鲁班普攻后，统计计数
        AllCalcStages = 0x1FFF,
        
        AfterBaseDamageCalculated= 1<< 20,
        AfterCriticalResolved,
        AfterBonusDamageApplied,
        AfterDamageMitigated,
        AfterOutgoingDamageAmplified,
        AfterIncomingDamageReduced,
        AfterDamageNegated,

        // ===== 事后（只读） =====
        OnLifeStealCalculated,
        OnDamageApplied, // 攻击方视角，触发被动效果
        OnDamageTaken // 受击方视角（反伤刺甲在此）
    }

    


    /// <summary>
    ///     伤害结算累加器 —— 引用类型，各阶段共享同一实例
    ///     生命周期由对象池管理，用完 Recycle
    /// </summary>
    [EnableClass]
    public sealed class DamageContext
    {
        // ===== 输入：本次结算的不可变事实 =====
        public Unit Source { get; set; }
        public Unit Target { get; set; }
        public DamageType DamageType { get; set; }
        public float TargetCurrentHp { get; set; }
        public float TargetMaxHp { get; set; }
        public float Coefficient { get; set; }
        public float BaseDamage { get; set; }

        public float LifeStealRate { get; set; }
        
        public SkillNode Skill { get; set; }
        
        public DamageTriggerType  DamageTriggerType { get; set; }
        public long DamageDelayTime { get; set; }
        //public SkillDamageConfig CurrentSkillDamageConfig { get; set; }
        public DamageStage CalcStages { get; set; }

        // ===== 各阶段可写结果（默认值即"无修正"） =====
        public bool IsHandled { get; set; } // 已经被处理，则拦截后续事件
        public float RawDamage { get; set; } // Step1
        public float CriticalMultiplier { get; set; } = 1f; // Step2
        public bool IsCritical { get; set; }
        public ReactionInfo ReactionResult { get; set; } // 元素伤害 Step3.0
        public bool IsEnhancedAttack { get; set; } // 是否强化普攻
        public float BonusDamage { get; set; } // Step3  末世法球
        public float MitigationMultiplier { get; set; } = 1f; // Step4
        public float AmplifyMultiplier { get; set; } = 1f; // Step5  破军 / 暴烈之甲
        public float ReduceMultiplier { get; set; } = 1f; // Step6  项羽 / 专精张飞
        public float NegationFactor { get; set; } // Step7  纯净苍穹,主动释放时，挂上buff
        public bool IsNegated { get; set; } // Step7  名刀致死拦截
        public bool IsExecuted { get; set; } // Step8  是否斩杀
        public float LifeStealAmount { get; set; } // Step9/10

        // ===== 输出 =====
        public float FinalDamage { get; set; }

        public bool HasStage(DamageStage stage)
        {
            return (this.CalcStages & stage) == stage;
        }
        
        public void SetStage(DamageStage stage, bool flag)
        {
            if (flag)
            {
                this.CalcStages |= stage;
            }
            else
            {
                this.CalcStages &= ~stage;
            }

        }
        
        public void Reset()
        {
            this.Source = this.Target = null;
            this.DamageType = DamageType.True;
            this.TargetCurrentHp = this.TargetMaxHp = 0f;
            this.IsCritical = this.IsEnhancedAttack = false;
            this.BaseDamage = this.FinalDamage = this.BonusDamage = this.NegationFactor = this.LifeStealRate = this.LifeStealAmount = 0f;
            this.CriticalMultiplier = this.MitigationMultiplier = this.AmplifyMultiplier = this.ReduceMultiplier = 1f;
            this.IsNegated = this.IsExecuted = false;
            this.CalcStages = DamageStage.AllCalcStages;
        }
        
    }

    public interface IDamageEvent
    {
        public DamageStage Stage { get; }
        public DamageContext Context { get; }
        public bool IsHandled { get; set; }
    }

    /// <summary>
    ///     伤害管线阶段事件参数
    ///     注意：结构体本身会被拷贝，但 Context 是引用，修改对调用方可见
    /// </summary>
    public struct DamageEvent : IDamageEvent
    {
        public DamageStage Stage { get; }
        public DamageContext Context { get; }
        
        public bool IsHandled { get; set; }
        public DamageEvent(DamageStage stage, DamageContext context)
        {
            this.Stage = stage;
            this.Context = context;
        }
    }
    
    
}