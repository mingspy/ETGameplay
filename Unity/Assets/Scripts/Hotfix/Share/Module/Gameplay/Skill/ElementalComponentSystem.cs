using System;
using System.Linq;

namespace ET
{
    [EntitySystemOf(typeof(ElementalComponent))]
    [FriendOf(typeof(ElementalComponent))]
    public static partial class ElementalComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ElementalComponent self, MaterialType materialType)
        {
            self.Unit = self.GetParent<Unit>();

            // 默认血肉材质
            self.BaseMaterial.Material = materialType; //MaterialType.Meat;
        }

        [EntitySystem]
        private static void Destroy(this ElementalComponent self)
        {
            self.ActiveElements.Clear();
            self.BaseMaterial = null;
            self.SurfaceMaterial = null;
        }

        [EntitySystem]
        private static void Update(this ElementalComponent self)
        {
            long now = TimeHelper.Now();

            if (self.SurfaceMaterial != null)
            {
                self.TickElementalReaction(self.SurfaceMaterial, now);
                if (UpdateElemental(self.SurfaceMaterial, now))
                {
                    self.SurfaceMaterial = null;
                }
            }

            if (self.BaseMaterial != null)
            {
                self.TickElementalReaction(self.BaseMaterial, now);
                if (UpdateElemental(self.BaseMaterial, now))
                {
                    self.BaseMaterial = null;
                }
            }

            // 更新元素持续时间和剩余量
            for (int i = self.ActiveElements.Count - 1; i >= 0; i--)
            {
                ElementalAttachment elem = self.ActiveElements[i];
                self.TickElementalReaction(elem, now);
                if (UpdateElemental(self.BaseMaterial, now))
                {
                    self.ActiveElements.RemoveAt(i);
                }
            }
        }

        private static bool UpdateElemental(ElementalAttachment elem, long now)
        {
            if (now > elem.ReactionEndTime)
            {
                elem.ResultType = ReactionType.None;
            }

            // 这里只做元素的消耗和处理，伤害由Buff自动处理。
            if (elem.DecayPerSecond > 0 && now > elem.LastUpdateTime + TimeHelper.OneSecond)
            {
                elem.Gauge -= elem.DecayPerSecond;
                elem.LastUpdateTime = now;
            }

            if ((now > elem.EndTime || elem.Gauge <= 0) && elem.ResultType == ReactionType.None)
            {
                return true;
            }

            return false;
        }

        public static ElementReactionConfig TryGetElementReactionConfig(ElementalType fromElement, ElementalType toElement)
        {
            int from = (int)fromElement;
            int to = (int)toElement;
            return ElementReactionConfigCategory.Instance.GetAll().Values
                    .FirstOrDefault(kv => kv.FromElement == from && kv.ToElement == to);
        }

        /// <summary>
        ///     获取材质引发的特殊反应
        /// </summary>
        public static MaterialReactionConfig TryGetMaterialReactionConfig(ElementalType fromElement, MaterialType toMaterial)
        {
            int from = (int)fromElement;
            int to = (int)toMaterial;
            return MaterialReactionConfigCategory.Instance.GetAll().Values
                    .FirstOrDefault(kv => kv.FromElement == from && kv.ToMaterial == to);
        }

        /// <summary>
        ///     判定并触发元素反应（重载：支持反应深度参数）
        /// </summary>
        public static ReactionInfo CheckAndTriggerReaction(this ElementalComponent self, Unit attacker, ElementalType attackElement, int attackElementAmount, int currentDepth = 0)
        {
            if (attackElement == ElementalType.None || currentDepth > self.MaxReactionDepth || attackElementAmount < 1)
            {
                return null;
            }

            ReactionInfo reactionResult = null;
            // 先检查与材质是否反应，目前规则：表面材质存在时，不穿透到主材质。
            ElementalAttachment material = self.SurfaceMaterial ?? self.BaseMaterial;
            if (material != null)
            {
                reactionResult = TryMaterialReaction(material, attackElement, attackElementAmount);
                if (reactionResult != null)
                {
                    reactionResult.OnMaterial = true;
                    reactionResult.OnWhichAttachment = material;
                    reactionResult.CurrentDepth =  currentDepth;
                    return reactionResult;
                }
            }

            // 遍历目标已附着元素，检查是否能触发反应
  
            for (int i = self.ActiveElements.Count - 1; i >= 0; i--)
            {
                ElementalAttachment elem = self.ActiveElements[i];
                reactionResult = TryElementReaction(elem, attackElement, attackElementAmount);
                
                if (reactionResult != null)
                {
                    reactionResult.OnWhichAttachment = elem;
                    reactionResult.CurrentDepth = currentDepth;

                    return reactionResult;
                }
            }

            // 如果没有触发反应，则附着该元素（深度超过上限时只附着不触发反应）
            ElementReactionConfig reactionConfig = TryGetElementReactionConfig(attackElement, ElementalType.None);
            
            long duration = TimeHelper.ToMS(reactionConfig?.Duration ?? 3); // 默认3秒存活
            int decay = reactionConfig?.DecayPerSecond ?? 0;
            if (currentDepth > 0)
            {
                // 扩散的元素持续时间衰减
                duration = (long)(self.SpreadIntensityDecay * duration);
            }

            self.AttachElement(attackElement, attackElementAmount, duration, decay, attacker.Id);
            return null;
        }

        public static void AttachElement(this ElementalComponent self, ElementalType attackElement, int amount, long duration, int decay, long sourceId)
        {
            ElementalAttachment elem = self.ActiveElements.FirstOrDefault(x => x.ElementalType == attackElement);
            if (elem == null)
            {
                elem = new ElementalAttachment()
                {
                    ElementalType = attackElement,
                    Gauge = amount,
                    EndTime = duration + TimeHelper.Now(),
                    DecayPerSecond = decay,
                    SourceId = sourceId
                };
                self.ActiveElements.Add(elem);
            }
            else
            {
                elem.Gauge += amount;
                elem.EndTime = duration + TimeHelper.Now();
                elem.DecayPerSecond = decay;
                elem.SourceId = sourceId;
            }
        }

        private static ReactionInfo TryMaterialReaction(ElementalAttachment elem, ElementalType attackElement, int attackElementAmount)
        {

            MaterialReactionConfig materialConfig = TryGetMaterialReactionConfig(attackElement, elem.Material);
            if (materialConfig == null)
            {
                return null;
            }

            ReactionInfo result = new ReactionInfo()
            {
                AttackElement = attackElement, AttackAmount = attackElementAmount,
                MaterialReactionConfig = materialConfig,
                ResultType = (ReactionType)materialConfig.Reaction,
                DamageMultiplier = (float)materialConfig.DamageMultiplier
            };
            result.DamageMultiplier = (float)materialConfig.DamageMultiplier;
            CalcReactionAmount(elem, attackElementAmount, materialConfig.Intensity, ref result);
            return result;

        }

        private static ReactionInfo TryElementReaction(ElementalAttachment elem, ElementalType attackElement, int attackElementAmount)
        {
            if (elem.Gauge < 1)
            {
                return null; 
            }
            
            ElementReactionConfig config = TryGetElementReactionConfig(attackElement, elem.ElementalType);
            if (config == null)
            {
                return null;
            }

            ReactionInfo result = new ReactionInfo()
            {
                AttackElement = attackElement, AttackAmount = attackElementAmount,
                ElementReactionConfig = config,
                ResultType = (ReactionType)config.Reaction,
                DamageMultiplier = (float)config.DamageMultiplier
            };
            
            CalcReactionAmount(elem, attackElementAmount, config.Intensity, ref result);
            return result;
        }

        private static void CalcReactionAmount(ElementalAttachment elem, float attackElementAmount, double intensity, ref ReactionInfo result)
        {
            double amount = attackElementAmount * intensity;
            if (intensity == 0 || amount > elem.Gauge) // 不消耗攻击元素或者攻击元素量多余当前元素
            {
                result.ReactionAmount = elem.Gauge;
                double usage = intensity == 0 ? 0 : elem.Gauge / intensity;
                result.RestAmount = (int)(attackElementAmount - usage);
            }
            else
            {
                result.ReactionAmount = (int)amount;
                result.RestAmount = 0;
            }

            // 造成1 ：1的伤害
            result.ExtraDamage = result.ReactionAmount;
        }

        /// <summary>
        ///     应用元素反应结果更新，继续扩散。
        /// </summary>
        /// <param name="self"></param>
        /// <param name="elem"></param>
        /// <param name="now"></param>
        private static void TickElementalReaction(this ElementalComponent self, ElementalAttachment elem, long now)
        {
            if (elem == null || elem.ResultType == ReactionType.None)
            {
                return;
            }

            ReactionType resultType = elem.ResultType & ReactionType.MASK;
            switch (resultType)
            {
                case ReactionType.Burning:
                    break;
                case ReactionType.Conductive:
                    break;
            }

            throw new NotImplementedException();
        }

        /// <summary>
        ///     实现元素效果的应用，包括扩散效果。
        /// </summary>
        /// <param name="self"></param>
        /// <param name="elem"></param>
        /// <param name="attack"></param>
        /// <param name="result"></param>
        /// <param name="currentDepth"></param>
        /// <exception cref="NotImplementedException"></exception>
        public static void ApplyReactionEffects(this ElementalComponent self, Unit attacker, ReactionInfo result, int currentDepth)
        {
            // 消耗当前元素
            if (!result.OnMaterial)
            {
                result.OnWhichAttachment.Gauge -= result.ReactionAmount;
                long durationRest = TimeHelper.ToMS(result.ElementReactionConfig.Duration);
                if (currentDepth > 0)
                {
                    durationRest = (long)(self.SpreadIntensityDecay * durationRest);
                }

                self.AttachElement(result.AttackElement, result.RestAmount, durationRest, result.ElementReactionConfig.DecayPerSecond, attacker.Id);
            }

            throw new NotImplementedException();
        }
    }
}