using System.Collections.Generic;
using System.Linq;

namespace ET
{
    /// <summary>
    /// 元素反应管理器Helper类
    /// </summary>
    public static class ElementalReactionHelper
    {
        public static bool TryGetElementReaction(ElementalType trigger, ElementalType aura, out ReactionRule rule)
        {
            rule = null;
            int from = (int)trigger;
            int to = (int)aura;
            ElementReactionConfig config = ElementReactionConfigCategory.Instance.GetAll().Values.FirstOrDefault(kv => kv.FromElement == from && kv.ToElement == to);
            if (config == null)
            {
                return false;
            }

            rule = new ReactionRule
            {
                Result = (ReactionType)config.Reaction,
                DamageMultiplier = (float)config.DamageMultiplier,
                Remain = (ElementalType)config.RemainElement,
                GaugeConsumption = (float)config.GaugeConsumption,
                VfxName = config.VfxName,
                IsConsumed = config.IsConsumed != 0,
                ApplyBuffs = config.ApplyBuffs,
            };
            return true;

        }
        
        /// <summary>
        /// 获取材质引发的特殊反应
        /// </summary>
        public static bool TryGetMaterialReaction(ElementalType trigger, MaterialType material, out ReactionRule rule)
        {
            rule = null;
            int from = (int)trigger;
            int to = (int)material;
            ElementMaterialReactionConfig  config = ElementMaterialReactionConfigCategory.Instance.GetAll().Values.FirstOrDefault(kv => kv.Element == from && kv.Material == to);
            if (config == null)
            {
                return false;
            }

            rule = new ReactionRule
            {
                Result = (ReactionType)config.Reaction,
                DamageMultiplier = (float)config.DamageMultiplier,
                Remain = (ElementalType)config.RemainElement,
                GaugeConsumption = (float)config.GaugeConsumption,
                VfxName = config.VfxName,
                IsConsumed = config.IsConsumed != 0,
                ApplyBuffs = config.ApplyBuffs,
            };
            return true;
        }
    }
}
