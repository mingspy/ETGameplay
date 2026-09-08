using System;
using System.Collections.Generic;

namespace ET
{
    /*
    // 技能静态配置
    public partial class SkillConfig: ETObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public float Cooldown { get; set; }
        public int ManaCost { get; set; }
        public float WindupTime { get; set; } // 前摇时间 (秒)
        public float RecoveryTime { get; set; } // 后摇时间 (秒)
        /// <summary>
        /// 命中时施加的Buff ID列表
        /// </summary>
        public List<int> HitBuffs { get; set; } = new List<int>();
        
        /// <summary>
        /// 施法时施加自身的Buff ID列表 (如霸体、无敌)
        /// </summary>
        public List<int> CastBuffs { get; set; } = new List<int>();
    }
    
    // BUFF静态配置
    public partial class Buff:ETObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public BuffType Type { get; set; } // Gain, Debuff, Control
        public float Duration { get; set; } // 持续时间，0为瞬时
        public bool IsStackable { get; set; } // 是否可叠加
        public int MaxStacks { get; set; } // 最大叠加层数
        public BuffEffectType EffectType { get; set; } // 修改属性、持续伤害、控制等
        public float EffectValue { get; set; } // 效果数值
    }
    */
    

    public partial class Root : ETObject
    {
        
    }
    

    public enum BuffEffectType
    {
        ModifyAttribute, // 修改属性
        DamageOverTime,  // 持续伤害
        Stun,            // 眩晕
        Silence          // 沉默
    }
    
    public static class BuffDurationType
    {
        public const int Instant = 0; // 立即执行
        public const int Infinite = 1;  // 永久性的
        public const int HasDuration = 2; // 有持续时间
    }
    
    [Flags]
    public enum BuffType
    {
        Numeric = 0,  // 默认修改属性
        Dead = 1 << 1, // 直接死亡
        Immortal = 1 << 2, // 无敌，不受任何伤害控制和死亡
        Stun = 1 << 3, // 眩晕
        Silence = 1<< 4 // 沉默
        //PersistAfterExpire = 1 << 30, // 过期后保留效果，默认为0，不保留。
    }
    
    // 运行时技能实例
    public class SkillInstance: ETObject
    {
        public int SkillId { get; set; }
        public long CasterId { get; set; } // 施法者ID
        public long TargetId { get; set; } // 目标ID
        public float CastTime { get; set; } // 施法时间点
        public bool IsFinished { get; set; }
    }

    // 运行时BUFF实例
    public class BuffInstance : ETObject
    {
        public int BuffId { get; set; }
        public long OwnerId { get; set; } // BUFF持有者
        public long CasterId { get; set; } // 施加者
        public float StartTime { get; set; }
        public float EndTime { get; set; }
        public float PeriodEndTime { get; set; }
        public int Stacks { get; set; } = 1;
        

        public int [] TotalEffects { get; set; }  // 总伤害，int类型等于原始值，float * 10000
        
        public BuffConfig Config { get; set; } // 引用静态配置
        
    }
    
}

