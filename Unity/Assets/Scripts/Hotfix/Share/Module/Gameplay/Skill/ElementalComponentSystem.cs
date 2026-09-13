using System;
using System.Collections.Generic;
using System.Linq;
using ET.Server;
using Unity.Mathematics;

namespace ET
{
    [EntitySystemOf(typeof(ElementalComponent))]
    [FriendOf(typeof(ElementalComponent))]
    public static partial class ElementalComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ElementalComponent self, MaterialType materialType)
        {
            // 设置默认材质
            self.BaseMaterial.Type = (int)materialType;
            self.AttachedElements.Add(self.SurfaceMaterial);
            self.AttachedElements.Add(self.BaseMaterial);
        }

        [EntitySystem]
        private static void Destroy(this ElementalComponent self)
        {
            self.AttachedElements.Clear();
            self.BaseMaterial = null;
            self.SurfaceMaterial = null;
        }

        [EntitySystem]
        private static void Update(this ElementalComponent self)
        {
            long now = TimeHelper.Now();

            // 更新元素持续时间和剩余量
            for (int i = self.AttachedElements.Count - 1; i >= 0; i--)
            {
                Elemental elem = self.AttachedElements[i];
                self.TickElementalReaction(elem, now);
                TickElemental(elem, now);
                // 材质不移除
                if (i > 1 && elem.IsExpired(now))
                {
                    self.AttachedElements.RemoveAt(i);
                }
            }
        }

        private static void TickElemental(Elemental elem, long now)
        {
            if (now > elem.ReactionEndTime)
            {
                elem.Reaction = ReactionType.None;
            }

            // 这里只做元素的消耗和处理，伤害由Buff自动处理。
            if (elem.DecayPerSecond > 0 && now > elem.LastUpdateTime + TimeHelper.OneSecond)
            {
                elem.Gauge -= elem.DecayPerSecond;
                elem.LastUpdateTime = now;
            }

            if (now > elem.EndTime || elem.Gauge <= 0)
            {
                elem.Clear();
            }
        }

        private static ReactionConfig TryGetReactionConfig(int fromElement, int toElement, int affixType)
        {
            return ReactionConfigCategory.Instance.GetAll().Values
                    .FirstOrDefault(kv => kv.FromElement == fromElement && kv.To == toElement && kv.ToType ==  affixType);
        }
        

        /// <summary>
        ///     判定并触发元素反应（重载：支持反应深度参数）
        /// </summary>
        public static ReactionInfo TryReactOrAppendElement(this ElementalComponent self, Unit attacker, ElementType attackElement,
        int attackElementAmount, int currentDepth = 0)
        {
            if (attackElement == ElementType.None || currentDepth > self.MaxReactionDepth || attackElementAmount < 1)
            {
                return null;
            }

            // 先检查与材质是否反应，目前规则：表面材质存在时，不穿透到主材质。
            Elemental material = self.SurfaceMaterial.Type != 0 ? self.SurfaceMaterial : self.BaseMaterial;
            ReactionInfo reactionResult = TryElementalReaction(material, attackElement, attackElementAmount);
            if (reactionResult != null)
            {
                reactionResult.CurrentDepth = currentDepth;
                return reactionResult;
            }

            // 遍历目标已附着元素，检查是否能触发反应
            for (int i = self.AttachedElements.Count - 1; i >= 2; i--)
            {
                Elemental elem = self.AttachedElements[i];
                reactionResult = TryElementalReaction(elem, attackElement, attackElementAmount);

                if (reactionResult != null)
                {
                    reactionResult.CurrentDepth = currentDepth;
                    return reactionResult;
                }
            }

            // 如果没有触发反应，则附着该元素（深度超过上限时只附着不触发反应）
            ReactionConfig reactionConfig = TryGetReactionConfig((int)attackElement, 0, (int)ElementalType.Elemental);

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

        private static void AttachElement(this ElementalComponent self, ElementType attackElement, int amount, long duration, int decay,
        long sourceId)
        {
            if (amount < 1)
            {
                return;
            }

            int eType = (int)attackElement;
            Elemental elem = self.AttachedElements.FirstOrDefault(x => x.Type == eType);
            if (elem == null)
            {
                elem = new Elemental
                {
                    Type = eType,
                    Gauge = amount,
                    EndTime = duration + TimeHelper.Now(),
                    DecayPerSecond = decay,
                    SourceId = sourceId
                };
                self.AttachedElements.Add(elem);
            }
            else
            {
                elem.Gauge += amount;
                elem.EndTime = Math.Max(duration + TimeHelper.Now(), elem.EndTime);
                elem.DecayPerSecond = decay;
                elem.SourceId = sourceId;
            }
        }
 

        private static ReactionInfo TryElementalReaction(Elemental elem, ElementType attackElement, int attackElementAmount)
        {
            
            // 风元素，并且上次结果是燃烧，产生扩散
            if (attackElement == ElementType.Wind && elem.Reaction == ReactionType.Burning)
            {
                return new()
                {
                    SourceElement = attackElement,
                    SourceAmount = attackElementAmount,
                    Reactant = elem,
                    Result = ReactionType.Swirl,
                    DamageMultiplier = 1.0f
                };
            }
            
            if (elem.Gauge < 1 || elem.Type == 0)
            {
                return null;
            }

            ReactionConfig config = TryGetReactionConfig((int)attackElement, elem.Type, (int)elem.AffixType);
            if (config == null)
            {
                return null;
            }

            ReactionInfo result = new()
            {
                SourceElement = attackElement,
                SourceAmount = attackElementAmount,
                Reactant = elem,
                ReactionConfig = config,
                Result = (ReactionType)config.Reaction,
                DamageMultiplier = (float)config.DamageMultiplier
            };

            CalcReactionAmount(elem, attackElementAmount, config.Intensity, ref result);
            return result;
        }

        private static void CalcReactionAmount(Elemental elem, float attackElementAmount, double intensity, ref ReactionInfo result)
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
        private static void TickElementalReaction(this ElementalComponent self, Elemental elem, long now)
        {
            if (elem == null || elem.Reaction == ReactionType.None)
            {
                return;
            }

            ReactionType resultType = elem.Reaction & ReactionType.MASK;
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
        public static void ApplyReactionEffects(this ElementalComponent self, Unit attacker, ReactionInfo result, int currentDepth)
        {
            // 消耗当前元素
            result.Reactant.Gauge -= result.ReactionAmount;
            ReactionConfig config = result.ReactionConfig;
            long durationRest = TimeHelper.ToMS(config.Duration);
            if (currentDepth > 0)
            {
                durationRest = (long)(self.SpreadIntensityDecay * durationRest);
            }

            if(result.RestAmount > 0)
                self.AttachElement(result.SourceElement, result.RestAmount, durationRest, config.DecayPerSecond, attacker.Id);


            StateComponent stateComponent = self.GetParent<Unit>().GetComponent<StateComponent>();
            switch (result.Result)
            {
                //case ReactionType.Vaporize: // 蒸发：火水反应，伤害倍率
                case ReactionType.Melt: // 融化：火冰反应，更高倍率
                    // TODO: 增加新元素 水，或者增加到表面Material
                    stateComponent.Unfreeze();
                    break;

                case ReactionType.Overload: // 超载：火雷反应，额外范围伤害+小击退
                    // 派发超载击退事件
                    EventSystem.Instance.Publish(self.Scene(), new OverloadEvent
                    {
                        Target = self.GetParent<Unit>(),
                        KnockbackDistance = 2f,
                        AoeRadius = 2f,
                        Damage = result.ExtraDamage
                    });
                    break;

                case ReactionType.Frozen: // 冻结：冰水反应，控制效果
                    float frozenDuration = currentDepth == 0 ? 1.5f : 0.8f;
                    // 挂载冻结Buff，阻止移动和施法        派发冻结事件（UI/特效/被动）
                    stateComponent.Freeze(frozenDuration, attacker.Id);
                    
                    break;

                case ReactionType.SuperConduct: // 超导：冰雷反应，减雷抗百分比
                    // TODO : 减抗和BuffId都改到配置文件实现。
                    // TODO : 迁移到 StateComponent?
                    float resistanceReduction = currentDepth == 0 ? 0.3f : 0.15f;
                    self.GetParent<Unit>().GetComponent<BuffComponent>().AddBuff( 
                            new NumericBuff(
                                new int [1]{NumericType.Pct(NumericType.LightningResist)}, new int[1]{NumericType.AsInt(resistanceReduction)},
                                (int)TimeHelper.ToMS(4f), attacker.Id
                            )
                        );
                    // 派发超导事件（供装备/被动监听）
                    EventSystem.Instance.Publish(self.Scene(), new SuperConductEvent
                    {
                        Target = self.GetParent<Unit>(),
                        ResistanceReduction = resistanceReduction,
                        Duration = 4f
                    });
                    
                    break;

                case ReactionType.ElectroCharged: // 感电：雷水反应，麻痹+持续伤害
                    float stunDuration = currentDepth == 0 ? 0.5f : 0.2f;
                    //result.ExtraDamage = currentDepth == 0 ? 30f * intensity : 15f * intensity;
                    stateComponent.Stun(stunDuration, attacker.Id);
                    break;

                case ReactionType.Swirl: // 扩散：风+任意，范围扩散元素+群体挂元素
                    // result.DamageMultiplier = 1.0f;
                    // 找到被扩散的元素类型: TODO: 风扩散元素 和 部分材质的反应结果，比如燃烧。
                    ElementType spreadElement = result.Reactant.AffixType == ElementalType.Elemental?(ElementType) result.Reactant.Type: result.Reactant.NewElement;
                    if (spreadElement != ElementType.None)
                    {
                        // 获取范围内所有敌方单位
                        // TODO: ET中的AOI只在服务器端存在，所以应该重新考虑最近实现的Elemental、Buff、Skill相关逻辑，
                        //      服务端权威，下发结果给客户端，客户端只负责表现，参考MovementComponent的做法
                        Dictionary<long, EntityRef<AOIEntity>> nearbyUnits = self.GetParent<Unit>().GetSeeUnits();
                        //List<Unit> nearbyUnits = aoi?.FindUnitsInRadius(self.Position, DefaultSpreadRadius, self.CampId, true) ?? new List();
                        // 给周围所有单位附着被扩散的元素（深度+1，标记为扩散源）
                        float distThresholdSq = self.DefaultSpreadRadius * self.DefaultSpreadRadius;
                        foreach (AOIEntity u in nearbyUnits.Values)
                        {
                            if (u.Unit.Id == attacker.Id || u.Unit.Id == self.Id) // 不扩散给攻击者和攻击者阵营，也不扩散给自己。
                            {
                                continue;
                            }

                            float distSq = math.distancesq(u.Unit.Position, self.GetParent<Unit>().Position);

                            if (distSq > distThresholdSq) continue;
                            
                            // TODO 阵营判断
                            

                            ElementalComponent unitElem = u.Unit.GetComponent<ElementalComponent>();
                            if (unitElem == null)
                            {
                                continue;
                            }

                            // 扩散附着元素，递归触发反应但深度限制
                            int spreadIntensity =  (int)(result.Reactant.Gauge * self.SpreadIntensityDecay);
                            unitElem.TryReactAndApply(attacker, spreadElement, spreadIntensity, currentDepth + 1);
                        }

                        // 派发扩散事件
                        EventSystem.Instance.Publish(self.Scene(),new SwirlEvent
                        {
                            Caster = attacker,
                            SpreadElement = spreadElement,
                            Radius = self.DefaultSpreadRadius,
                            AffectedUnits = nearbyUnits.Keys.ToList()
                        });
                    }
                    
                    // TODO: 扩散上次反应的结果

                    break;
            }
        }

        private static void TryReactAndApply(this ElementalComponent self, Unit attacker, ElementType attackElement,
            int attackElementAmount, int currentDepth = 0)
        {
            ReactionInfo result = self.TryReactOrAppendElement(attacker, attackElement, attackElementAmount, currentDepth);
            if (result != null)
            {
                self.ApplyReactionEffects(attacker, result, currentDepth);
            }
        }
    }
}