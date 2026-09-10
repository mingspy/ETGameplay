using System.Collections.Generic;

namespace ET;

public class Root : ETObject
{
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

/// <summary>
///     战斗事件类型
/// </summary>
public enum BattleEventType
{
    OnTakeDamage = 1, // 受到伤害
    OnDealDamage = 2, // 造成伤害
    OnKill = 3, // 击杀
    OnDeath = 4 // 死亡
}

/// <summary>
///     战斗事件数据
/// </summary>
public struct BattleEventData
{
    public Unit Attacker; // 攻击者
    public Unit Target; // 受击者
    public string Note; // 伤害来源
    public float TotalDamage;
    public List<Damage> DamageDetail;
    public object Arg; // 其他参数
}

/// <summary>
///     伤害类型
/// </summary>
public static class DamageType
{
    public const int True = 0, // 真伤
            Physical = 1, // 物理伤害
            Magical = 2, // 法术伤害
            Fire = 3, // 元素伤害 火，属于法术伤害的一种，但是单独拿出来，增加趣味性(如塞尔达传说)
            Water = 4, // 水
            Ice = 5, // 冰
            Electricity = 6, // 雷 / 电
            Wind = 7, // 风
            Earth = 8, // 地 / 土
            Light = 9, //  光 / 圣
            Dark = 10, //  暗 / 邪
            Nature = 11, // 自然 (木)
            Poison = 12, // 毒 (或 Toxin)
            Oil = 13; // 油

    public static bool IsElemental(int damageType)
    {
        return damageType >= Fire && damageType <= Dark;
    }

    public static ElementalType ToElementalType(int damageType)
    {
        return (ElementalType)(damageType - Magical);
    }
}

public class Damage : ETObject
{
    public int DamageType; // 伤害类型
    public float Value; // 伤害数值
    public bool IsCritical; //// 是否暴击
    public SkillDamageConfig Config;
}

// 运行时技能实例
public class SkillInstance : ETObject
{
    public SkillConfig Config { get; set; }
    public int Level { get; private set; } // 当前等级
    public List<SkillDamageConfig> Damages { get; set; }
    private SkillLevelConfig LevelConfig { get; set; }

    public float CD { get; private set; }
    public string SkillName => this.Config?.Name;
    public int SkillId => this.Config?.Id ?? 0;

    public void SetLevel(int level)
    {
        if (level == this.Level || level < 1 || level > this.Config.MaxLevel)
        {
            return;
        }

        this.Level = level;
        // 更新Buff
        // 更新Damage Config
        this.LevelConfig = SkillLevelConfigCategory.Instance.Get(this.Config.LevelConfigIds[level - 1]);

        if (this.Damages == null)
        {
            this.Damages = new List<SkillDamageConfig>(this.LevelConfig.SkillDamageIds.Length);
        }
        else
        {
            this.Damages.Clear();
        }

        foreach (int skillDamageId in this.LevelConfig.SkillDamageIds)
        {
            this.Damages.Add(SkillDamageConfigCategory.Instance.Get(skillDamageId));
        }

        this.CD = (float)this.LevelConfig.CoolDown;
    }
}