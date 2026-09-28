using System.Collections.Generic;

namespace ET
{
    public class Root : ETObject
    {
    }

    [EnableClass]
    public class DamageReq
    {
        /// <summary>伤害类型</summary>
        public DamageType DamageType { get; set; }

        /// <summary>
        ///     伤害加成比例。计算基础伤害 (Base Damage) = 攻击力 * 伤害系数 + 固定伤害值 = 攻击力 * Coefficient + RawDamage
        /// </summary>
        public double Coefficient { get; set; }

        /// <summary>基础伤害</summary>
        public double BaseDamage { get; set; }

        /// <summary>是否可暴击,1可以</summary>
        public bool CanCrit { get; set; }

        /// <summary>吸血比例</summary>
        public double LifeStealRate { get; set; }

        #region 待配置文件支持

        public DamageTriggerType TriggerType; // 瞬时/延迟/持续每帧
        public long DelayTime; // 延迟触发时间
        public long Duration; // 持续伤害总时长
        public long TickInterval; // 持续伤害间隔

        #endregion
    }

    // 运行时技能实例
    [EnableClass]
    public class SkillNode
    {
        public SkillType SkillType { get; set; }
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