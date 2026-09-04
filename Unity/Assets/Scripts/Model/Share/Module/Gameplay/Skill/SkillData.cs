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
    public partial class BuffConfig:ETObject
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
    
    public enum BuffType
    {
        Gain,   // 增益
        Debuff, // 减益
        Control // 控制
    }

    public enum BuffEffectType
    {
        None,
        ModifyAttribute, // 修改属性
        DamageOverTime,  // 持续伤害
        Stun,            // 眩晕
        Silence          // 沉默
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
        public int Stacks { get; set; } = 1;
        public double Duration { get; set; }
        
        //public BuffConfig Config { get; set; } // 引用静态配置

        public bool IsExpired(float currentTime)
        {
            if (Duration <= 0) return true; // 瞬时BUFF立即过期
            return currentTime >= EndTime;
        }
    }
    
}

