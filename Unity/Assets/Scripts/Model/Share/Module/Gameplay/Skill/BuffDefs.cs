using System;
using MemoryPack;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    [EnableClass]
    public class BuffDataBase
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
        public int Stacks { get; set; } = 1;

        public int[] TotalEffects { get; set; } // 总伤害，int类型等于原始值，float * 10000

        [MemoryPackIgnore]
        [BsonIgnore]
        public BuffConfig BuffConfig { get; set; }
    }

    public interface IBuffSystem
    {
        public BuffDataBase BuffData { get; set; }
        public void Init(long now);
        public void Update(long now);
        public void Finished(long now);
    }

    public abstract class ABuffBase : ETObject, IDisposable, IBuffSystem
    {
        public abstract void Init(long now);
        public abstract void Update(long now);
        public abstract void Finished(long now);
        public BuffDataBase BuffData { get; set; }
        public abstract void Dispose();
    }

    public enum BuffEffectType
    {
        ModifyAttribute, // 修改属性
        DamageOverTime, // 持续伤害
        Stun, // 眩晕
        Silence // 沉默
    }

    public static class BuffDurationType
    {
        public const int Instant = 0; // 立即执行
        public const int Infinite = 1; // 永久性的
        public const int HasDuration = 2; // 有持续时间
    }

    public static class BuffType
    {
        public const uint Numeric = 0; // 默认修改属性
        public const uint Passive = 1 << 1; // 被动技能
        public const uint Active = 1 << 2; // 主动技能
        public const uint Talent = 1 << 3; // 天赋技能
        public const uint Equip = 1 << 4; // 装备
        public const uint Rune = 1 << 5; // 符文
        public const uint Dead = 1 << 10; // 直接死亡
        public const uint Immortal = 1 << 11; // 无敌，不受任何伤害控制和死亡
        public const uint Stun = 1 << 12; // 眩晕
        public const uint Silence = 1 << 13; // 沉默
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
        public int Stacks { get; set; } = 1;

        public int[] TotalEffects { get; set; } // 总伤害，int类型等于原始值，float * 10000

        public BuffConfig Config { get; set; } // 引用静态配置
    }
}