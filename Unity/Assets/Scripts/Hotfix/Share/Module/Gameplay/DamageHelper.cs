using System;
using System.Collections.Generic;

namespace ET
{
    [FriendOf(typeof(Unit))]
    public static class DamageHelper
    {
        #region 伤害处理


        /// <summary>
        /// TODO: 获取用户状态，可以专门搞一个组件管理用户状态。
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static int GetStates(Unit unit)
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// TODO: 应用技能前，要先检查目标状态，无敌 等。
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="target"></param>
        /// <param name="skill"></param>
        public static void ApplySkillDamage(Unit attacker, Unit target, SkillInstance skill)
        {
            float totalFinalDamage = 0;
            var damages = new List<DamageInfo>(skill.Damages.Count);

            foreach (SkillDamageConfig damageConfig in skill.Damages)
            {
                DamageInfo info = new()
                {
                    DamageType = damageConfig.DamageType,
                    NumericRatio = damageConfig.NumericRatio,
                    FlatBaseValue = damageConfig.FlatBaseValue,
                    CanCrit = damageConfig.CanCrit == 1,
                    LifeStealRate = damageConfig.LifestealRate
                };
                CalcDamage(attacker, target, info);
                damages.Add(info);
                totalFinalDamage += info.FinalDamage;
            }

            ApplyDamage(new BattleEventData()
            {
                Attacker = attacker,
                Target = target,
                Note = skill.SkillName,
                TotalDamage = totalFinalDamage,
                DamageDetail = damages,
                Arg = skill
            });
        }

        /// <summary>
        ///     计算最终伤害 <br />
        ///     【通用减伤公式】, 大多数MOBA使用以下公式计算实际受到的伤害： 实际伤害= 原始伤害 × C / (C+防御属性) <br />
        ///     ‌防御属性‌：护甲（Armor）对应物理伤害，魔抗（Magical Resist）对应法术伤害。<br />
        ///     常数 C‌：一个平衡系数，不同游戏取值不同。<br />
        ///     - 《英雄联盟》早期/经典模型‌：C=100。即100点护甲提供50%减伤，200点护甲提供66.7%减伤。<br />
        ///     - 《DOTA2》‌：C 随等级变化，或采用更复杂的线性近似公式：减伤比例 = 护甲 * 0.06 /(1 + 护甲 * 0.06)<br />
        ///     - 《王者荣耀》‌：大致遵循: 减伤比例 = 602 / (护甲 + 602） （具体系数随版本微调，旨在让前期收益高，后期收益递减）。<br />
        ///     【核心特性】：边际收益递减<br />
        ///     ‌线性收益的有效生命值‌：虽然减伤百分比是递减的，但每增加1点护甲，角色能承受的‌额外物理伤害总量‌是固定的（线性增长）。<br />
        ///     【元素反应伤害】：<br />
        ///     计算过程：BaseDamage → 元素反应倍率/debuff应用 → 暴击判定 → 穿透/防御减伤 → 控制/附加效果<br />
        ///     根据反应类型给当前伤害加对应倍率、给目标挂载对应debuff，然后继续走原有的暴击、穿透、防御计算即可。<br />
        ///     【简化元素反应】，数值上只做简单的加成和反应BUFF，不消耗元素。视觉上，保留元素反应结果，增加趣味性。
        /// </summary>
        public static void CalcDamage(Unit attacker, Unit target, DamageInfo info)
        {
            // 1. 计算基础伤害
            CalculateBaseDamage(attacker, info);
            // 1. 计算最终伤害
            CalcFinalDamage(attacker, target, info);
        }

        /// <summary>
        ///     根据角色本身的属性，计算基础攻击力 = 角色攻击面板 * 技能加成比例 + 技能基础攻击。
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="info"></param>
        public static void CalculateBaseDamage(Unit attacker, DamageInfo info)
        {
            NumericComponent attackerNumeric = attacker.GetComponent<NumericComponent>();
            float numericValue = info.DamageType == DamageType.True ? attackerNumeric.GetAsInt(NumericType.TrueDamage)
                    : attackerNumeric.GetAsInt(NumericType.Placeholder_DamageStart + info.DamageType);
            info.BaseDamage = CalculateBaseDamage(numericValue, (float)info.NumericRatio, (float)info.FlatBaseValue);
        }

        /// <summary>
        ///     计算基础伤害 (Base Damage) = 攻击力 * 伤害系数 + 固定伤害值
        ///     注意：此处不包含暴击、穿透、防御减伤、增伤Buff等后续乘区。
        /// </summary>
        private static float CalculateBaseDamage(float panelAttack, float DamageRatio, float flatValue)
        {
            return panelAttack * DamageRatio + flatValue;
        }

        /// <summary>
        ///     根据攻击者和受害者的防御，穿刺，元素伤害等计算最终伤害。 <br/>
        /// TODO: 暂时未考虑全局BUFF效果， 考虑全局BUFF
        /// </summary>
        /// <param name="attacker">攻击者</param>
        /// <param name="target">受击者</param>
        /// <param name="info"></param>
        /// <param name="currentDepth">控制元素伤害的传递深度</param>
        public static void CalcFinalDamage(Unit attacker, Unit target, DamageInfo info, int currentDepth = 0)
        {
            // 1. 计算基础伤害
            float finalDamage = info.BaseDamage;
            if (info.DamageType == DamageType.True)
            {
                finalDamage = Math.Max(finalDamage, 0); // 防止负伤害，即造成回血效果。比如元素克制反而回血，后期打不动。
                info.FinalDamage = finalDamage;
                return;
            }

            NumericComponent attackerNumeric = attacker.GetComponent<NumericComponent>();
            NumericComponent targetNumeric = target.GetComponent<NumericComponent>();

            // 防御 (护甲，魔抗 or 元素抗性)
            float baseDefence = targetNumeric.GetAsInt(NumericType.Placeholder_ResistStart + info.DamageType);
            // 穿透比例
            float PenetrationPercent = attackerNumeric.GetAsFloat(NumericType.Placeholder_PctPenStart + info.DamageType);
            // 穿透面板属性
            float PenetrationFlat = attackerNumeric.GetAsInt(NumericType.Placeholder_FlatPenStart + info.DamageType);

            // 2. TODO: 计算元素伤害，只有简单的元素反应和相克，最终结果是时加成、减收益或者挂BUFF。
            // 反应结果应用到双方，元素本身不消耗，只改变。
            float elementExtraDamage = 0;
            if (DamageType.IsElemental(info.DamageType))
            {
                ElementalComponent targetElement = target.GetComponent<ElementalComponent>();
                if (targetElement != null)
                {
                    ReactionInfo reactionResult = targetElement.TryReactOrAppendElement(attacker, DamageType.ToElement(info.DamageType),
                        (int)info.FlatBaseValue, currentDepth);
                    if (reactionResult != null)
                    {
                        finalDamage *= reactionResult.DamageMultiplier;
                        elementExtraDamage = reactionResult.ExtraDamage;
                        info.ReactionResult = reactionResult;
                    }
                }
            }

            // 3. 暴击判定 (真伤无暴击，且仅当允许暴击时)
            if (info.CanCrit)
            {
                float critRate = attackerNumeric.GetAsFloat(NumericType.CritChance); // 暴击率 (0.0 - 1.0)
                if (RandomGenerator.RandFloat01() < critRate)
                {
                    info.IsCritical = true;
                    // 触发暴击特效
                    float critDamagePct = attackerNumeric.GetAsFloat(NumericType.CritDamagePct); // 暴击伤害倍率（0.0 - x)
                    finalDamage *= 1.0f + critDamagePct; // 加成系数
                }
            }

            // 4. 计算穿透
            finalDamage *= CalcMitigationFactor(CalculateEffectiveDefense(baseDefence, PenetrationPercent, PenetrationFlat));
            ;

            // TODO: 应用全局伤害buff，需要时采用下面方案2即可。
            // 目前为止计算都没考虑角色身上的全局增伤BUFF。Numeric组件每个属性虽然有FinalAdd和FinalPct，但是都是针对基础属性的。
            // 1. 简单做法是把全局增伤BUFF直接加到所有属性上，那么前面获取Numeric属性时附带了，但是不优雅。
            // 2. 同样利用Numeric组件，增加一个全局增伤组件 (finalDamage + finalAdd) * (1 + finalPct)

            finalDamage += elementExtraDamage; // 加上元素额外伤害不参与穿透和防御计算，直接附加。

            info.FinalDamage = Math.Max(finalDamage, 0); // 防止负伤害，即造成回血效果。比如元素克制反而回血，后期打不动。
        }

        /// <summary>
        ///     有效防御 = 基础防御 * ( 1 - 穿透比例) - 穿透固定值
        /// </summary>
        /// <param name="baseDef"></param>
        /// <param name="penPercent"></param>
        /// <param name="penFlat"></param>
        /// <returns></returns>
        private static float CalculateEffectiveDefense(float baseDef, float penPercent, float penFlat)
        {
            float afterFlat = baseDef * (1 - penPercent) - penFlat;
            return Math.Max(afterFlat, 0); // 防御最低为0
        }

        // 辅助：经典减伤系数
        private static float CalcMitigationFactor(float defense, float C = 100f)
        {
            return C / (C + defense);
        }

        /// <summary>
        ///     应用伤害，这里只是示例，先完成相应功能。<br />
        ///     实际系统时，
        ///     如果是MOBA，则先计算所有玩家打出的伤害，防止玩家死亡导致其发出的伤害判定无效，然后统一应用伤害。
        ///     如果是RPG,则计算AOI范围内的伤害，然后再应用。<br />
        ///     另外一种解法是，玩家打出的技能不随死亡销毁，更简单一些。即死亡与技能不会量子纠缠 :P。
        /// </summary>
        /// <param name="eventData"></param>
        public static void ApplyDamage(BattleEventData eventData)
        {
            // 目标扣血
            eventData.Target.TakeDamage(eventData);
            // 同时触发「造成伤害」事件（用于吸血等被动）
            eventData.Attacker.OnMakeDamage(eventData);

            // 【关键】触发「受到伤害」事件，通知所有监听者，如日志记录；
            // TODO: 验证 EventSystem发布的事件是否能实现针对当前攻击者飘血显示多事多样的效果。
            // TODO: 服务端计算结果，客户端接受结果。（当前玩家可以先预测再回滚， UI效果不用回滚 )
            EventSystem.Instance.Publish(eventData.Attacker.Scene(), eventData);
        }

        #endregion

        #region unit伤害处理-扣血和触发被动

        /// <summary>
        ///     受到伤害，减血，并触发被动技能，如反甲
        /// </summary>
        /// <param name="self"></param>
        /// <param name="data"></param>
        public static void TakeDamage(this Unit self, BattleEventData data)
        {
            NumericComponent numeric = self.GetComponent<NumericComponent>();
            numeric[NumericType.Hp] -= (long)data.TotalDamage;

            if (data.DamageDetail == null)
            {
                return;
            }

            foreach (DamageInfo damage in data.DamageDetail)
            {
                // 0. 检查元素伤害，并应用。
                if (DamageType.IsElemental(damage.DamageType) && damage.ReactionResult != null)
                {
                    self.GetComponent<ElementalComponent>()
                            .ApplyReactionEffects(data.Attacker, damage.ReactionResult, damage.ReactionResult.CurrentDepth);
                }

                // 1. 反甲效果
                if (damage.DamageType == DamageType.Physical)
                {
                    float ratio = numeric.GetAsFloat(NumericType.ThornMailRate);
                    if (ratio > 0.01)
                    {
                        float reflectDamage = damage.FinalDamage * ratio;
                        // 反弹伤害给攻击者，简化计算，为真伤。
                        BattleEventData reflectData = new()
                        {
                            Attacker = self,
                            Target = data.Attacker,
                            Note = "Thornmail",
                            TotalDamage = reflectDamage
                        };
                        data.Attacker.TakeDamage(reflectData);
                    }
                }

                // 2. 其他受击类被动都在这里加...
            }
        }

        /// <summary>
        ///     造成伤害时，触发被动技能，如吸血
        /// </summary>
        /// <param name="self"></param>
        /// <param name="data"></param>
        public static void OnMakeDamage(this Unit self, BattleEventData data)
        {
            NumericComponent numeric = self.GetComponent<NumericComponent>();

            if (data.DamageDetail == null)
            {
                return;
            }

            float totalStealLife = 0;

            foreach (DamageInfo damage in data.DamageDetail)
            {
                // 1. 吸血效果
                // 技能吸血怎么处理，用Buff还是直接计算？ 这里直接计算，buff只处理持续效果。
                if (damage.DamageType == DamageType.Physical || damage.DamageType == DamageType.Magical)
                {
                    float ratio = damage.DamageType == DamageType.Physical ?
                            numeric.GetAsFloat(NumericType.LifeStealRate) :
                            numeric.GetAsFloat(NumericType.MagicLifeStealRate);
                    ratio += (float)damage.LifeStealRate;

                    totalStealLife += damage.FinalDamage * ratio;
                }

                // 2. 其他攻击类被动都在这里加...
            }

            // 回血
            numeric[NumericType.Hp] += (long)totalStealLife;
        }

        #endregion
    }
}