using System;
using MemoryPack;
using MongoDB.Bson.Serialization.Attributes;
using Sirenix.Utilities.Editor;

namespace ET
{
    public enum BuffDurationType
    {
        Instant = 0, // 立即执行
        Infinite = 1, // 永久性的
        HasDuration = 2 // 有持续时间
    }

    [Flags]
    public enum BuffType
    {
        None = 0,
        Numeric = 1, // 默认修改属性
        Control = 1 << 8, // 控制状态，分配255个，足够使用了。
        Stunned,  // 眩晕，无法移动，无法释放技能
        Dead, // 直接死亡
        Invincible, // 无敌，不受任何伤害控制和死亡
        Silence, // 沉默， 无法释放技能，可以移动
        Frozen, // 冰冻，无法移动
        SuperArmor, // 霸体（免疫控制）
        Enhanced, // 强化状态
        Passive =  1 << 9, // 被动技能
        Active = 1 << 10, // 主动技能
        Talent= 1 << 11, // 天赋技能
        Equip= 1 << 12, // 装备
        Rune= 1 << 13,// 符文
    }

    // 运行时BUFF实例
    public class BuffInstance : ETObject
    {
        public int BuffId { get; set; }
        public long OwnerId { get; set; } // BUFF持有者
        public long CasterId { get; set; } // 施加者

        /// <summary>
        ///     开始时间，单位毫秒数，以下都为毫秒。
        /// </summary>
        public long StartTime { get; set; }

        public long EndTime { get; set; }
        public long PeriodEndTime { get; set; }
        /// <summary>
        /// 当前BUFF的层数
        /// </summary>
        public int Stacks { get; set; }
        /// <summary>
        /// 最高层数，默认1。 如果是0，表示立即执行的BUFF，不会添加到BuffSystem中管理
        /// </summary>
        public int MaxStacks { get; set; }

        public int[] TotalEffects { get; set; } // 总伤害，int类型等于原始值，float * 10000

        public BuffConfig Config { get; set; } // 引用静态配置
    }
    
    [EnableClass]
    public abstract class BuffDataBase
    {
        /// <summary>
        /// Buff所有者
        /// </summary>
        public int BuffId { get; set; }
        public long CasterId { get; set; } // 施加者
        public BuffType Type { get; set; }
        public BuffDurationType DurationType { get; set; }
        
        /// <summary>
        /// 持续时间，单位毫秒， Buff的时间单位都是毫秒。
        /// </summary>
        public long Duration { get; set; }
        
        /// <summary>
        /// 周期时间，0，无周期性。
        /// </summary>
        public int Period { get; set; }

        /// <summary>
        ///     开始时间
        /// </summary>
        public long StartTime { get; set; }
        public long EndTime { get; set; }
        public long PeriodEndTime { get; set; }
        public int MaxStacks { get; set; } = 1;
        public int Stacks { get; set; } = 1;
        

        [MemoryPackIgnore]
        [BsonIgnore]
        public BuffConfig BuffConfig { get; set; }

        public void Init(long currentMs)
        {
            BuffConfig config = this.BuffConfig;
            if (config != null)
            {
                this.Duration = TimeHelper.ToMS(config.Duration);
                this.DurationType = (BuffDurationType)config.DurationType;
                this.BuffId = config.Id;
                this.Period = config.Period;
                this.MaxStacks = config.MaxStacks;
                
            }
            this.StartTime = currentMs;
            this.EndTime = currentMs + Duration;
            this.PeriodEndTime = currentMs + Period;
            if (this.Duration > 0 && this.DurationType == BuffDurationType.Instant)
            {
                this.DurationType = BuffDurationType.HasDuration;
            }
            
            this.PartialInit();
        }

        protected virtual void PartialInit()
        {
            
        }
    }
    
}