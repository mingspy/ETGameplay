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
        Passive, // 被动技能
        Active, // 主动技能
        Talent, // 天赋技能
        Equip, // 装备
        Rune,// 符文
        Dead, // 直接死亡
        Immortal, // 无敌，不受任何伤害控制和死亡
        Silence, // 沉默， 无法释放技能，可以移动
        Stunned,  // 眩晕，无法移动，无法释放技能
        Frozen, // 冰冻，无法移动
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
    public class BuffDataBase
    {
        /// <summary>
        /// Buff所有者
        /// </summary>
        public int BuffId { get; set; }
        public long CasterId { get; set; } // 施加者
        public BuffType Type { get; protected set; }
        public BuffDurationType DurationType { get; protected set; }
        
        /// <summary>
        /// 持续时间，单位毫秒， Buff的时间单位都是毫秒。
        /// </summary>
        public int Duration { get; protected set; }
        
        /// <summary>
        /// 周期时间，0，无周期性。
        /// </summary>
        public int Period { get; protected set; }

        /// <summary>
        ///     开始时间
        /// </summary>
        public long StartTime { get; set; }
        public long EndTime { get; set; }
        public long PeriodEndTime { get; set; }
        public int MaxStacks { get; protected set; } = 1;
        public int Stacks { get; set; } = 1;
        

        [MemoryPackIgnore]
        [BsonIgnore]
        public BuffConfig BuffConfig { get; set; }

        public BuffDataBase(BuffType buffType = BuffType.None, int durationMs = 0, long casterId = -1)
        {
            this.Type = buffType;
            this.Duration = durationMs;
            this.CasterId = casterId;
            if (durationMs > 0)
            {
                this.DurationType = BuffDurationType.HasDuration;
            }
        }

        public BuffDataBase(BuffConfig buffConfig)
        {
            InitFromConfig(buffConfig);
        }
        
        public BuffDataBase(int configId)
        {
            BuffConfig buffConfig = BuffConfigCategory.Instance.Get(configId);
            InitFromConfig(buffConfig);
        }

        private void InitFromConfig(BuffConfig config)
        {
            BuffId = config.Id;
            Type  = (BuffType) config.BuffType;
            Duration = config.Duration;
            DurationType =  (BuffDurationType) config.DurationType;
            Period = config.Period;
            MaxStacks = config.MaxStacks;
            BuffConfig = config;
        }
    }

    public class ControlBuff : BuffDataBase
    {
        public State  State { get;}
        public ControlBuff(State state, BuffType buffType, int durationMs, long casterId): base(buffType, durationMs, casterId)
        {
            this.State = state;
        }
        
    }
    
    /// <summary>
    /// 属性修改器
    /// </summary>
    public class NumericBuff : BuffDataBase
    {
        /// <summary>
        /// 修改的属性，即 NumericType
        /// </summary>
        private int [] Numerics { get; set; }
        
        /// <summary>
        /// 修改的属性数值，int 设置时为原始值，float设置时 要 * 10000， 比如要设置攻击加速比例为 1.5f，需要设置为 15000
        /// </summary>
        private int [] NumericValues { get; set; }
        
        public int[] TotalEffects { get; set; } 
        public NumericBuff(int [] numerics, int [] numericValues, int durationMs, long casterId): base(BuffType.Numeric, durationMs, casterId)
        {
            this.Numerics = numerics;
            this.NumericValues = numericValues;
            this.TotalEffects = new int [numerics.Length];
        }
        
    }


    /*
    public abstract class BuffSystemBase : ETObject, IDisposable, IBuffRunner
    {
        public abstract void OnAdd(long now);
        public abstract void OnUpdate(long now);
        public abstract void OnRemove(long now);
        public abstract void OnFinished(long now);

        public BuffDataBase BuffData { get; set; }
        public abstract void Dispose();
    }
    */
}