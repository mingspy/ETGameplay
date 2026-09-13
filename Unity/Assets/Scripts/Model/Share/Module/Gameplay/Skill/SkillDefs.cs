using System.Collections.Generic;

namespace ET
{
    public class Root : ETObject
    {
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
        public Unit Attacker { get; set; } // 攻击者
        public Unit Target{ get; set; } // 受击者
        public string Note{ get; set; }// 伤害来源
        public float TotalDamage{ get; set; }
        public List<DamageInfo> DamageDetail{ get; set; }
        public object Arg{ get; set; }// 其他参数
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
                Lightning = 6, // 雷 / 电
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

        public static ElementType ToElementalType(int damageType)
        {
            return (ElementType)(damageType - Magical);
        }
    }

    [EnableClass]
    public class DamageInfo
    {
        /// <summary>伤害类型</summary>
        public int DamageType { get; set; }
        /// <summary>Numeric伤害加成比例</summary>
        public double NumericRatio { get; set; }
        /// <summary>基础伤害</summary>
        public double FlatBaseValue { get; set; }
        /// <summary>是否可暴击,1可以</summary>
        public bool CanCrit { get; set; }
        /// <summary>吸血比例</summary>
        public double LifestealRate { get; set; }
        
         // Result
         public float BaseDamage { get; set; }
         public bool IsCritical { get; set; } //// 是否暴击
         public float FinalDamage { get; set; } // 伤害数值
         public ReactionInfo ReactionResult { get; set; } // 元素伤害
    }
    

    // 运行时技能实例
    [EnableClass]
    public class SkillInstance
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
}