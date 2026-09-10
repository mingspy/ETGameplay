using System;
using System.Collections.Generic;

namespace ET;

[FriendOf(typeof(Unit))]
public static class DamageHelper
{
    #region 伤害处理

    public static void ApplySkillDamage(Unit attacker, Unit target, SkillInstance skill)
    {
        float totalFinalDamage = 0;
        var damages = new List<Damage>(skill.Damages.Count);

        foreach (SkillDamageConfig damageConfig in skill.Damages)
        {
            Damage oneDamage = CalcSkillOneDamage(attacker, target, damageConfig);
            damages.Add(oneDamage);
            totalFinalDamage += oneDamage.Value;
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
    private static Damage CalcSkillOneDamage(Unit attacker, Unit target, SkillDamageConfig damageConfig)
    {
        NumericComponent attackerNumeric = attacker.GetComponent<NumericComponent>();
        NumericComponent targetNumeric = target.GetComponent<NumericComponent>();

        bool isCrit = false;
        float numericValue = 0; // 基础属性
        float baseDefence = 0; // 防御 (护甲，魔抗 or 元素抗性)
        float PenetrationPercent = 0; // 穿透比例
        float PenetrationFlat = 0; // 穿透面板属性
        float mitigationFactor = 1.0f; // 防御与穿透计算 (根据伤害类型分支)

        if (damageConfig.DamageType == DamageType.True)
        {
            // 真伤无穿透和暴击
            numericValue = attackerNumeric.GetAsInt(NumericType.TrueDamage);
        }
        else
        {
            numericValue = attackerNumeric.GetAsInt(NumericType.DamageStart + damageConfig.DamageType);
            baseDefence = targetNumeric.GetAsInt(NumericType.ResistStart + damageConfig.DamageType);
            PenetrationPercent = attackerNumeric.GetAsFloat(NumericType.PenetrationPercentStart + damageConfig.DamageType);
            PenetrationFlat = attackerNumeric.GetAsInt(NumericType.PenetrationFlatStart + damageConfig.DamageType);
        }

        // 1. 计算基础伤害
        float finalDamage = CalculateBaseDamage(numericValue, (float)damageConfig.NumericRatio, (float)damageConfig.FlatBaseValue);

        // 2. TODO: 计算元素伤害，只有简单的元素反应和相克，最终结果是时加成、减收益或者挂BUFF。
        // 反应结果应用到双方，元素本身不消耗，只改变。
        float elementExtraDamage = 0;
        if (DamageType.IsElemental(damageConfig.DamageType))
        {
            ReactionResult result = CheckElementalReaction(attacker, target, damageConfig);
            finalDamage *= result.DamageMultiplier;
            elementExtraDamage = result.ExtraDamage;
        }

        // 3. 暴击判定 (真伤无暴击，且仅当允许暴击时)
        if (damageConfig.DamageType != DamageType.True && damageConfig.CanCrit == 1)
        {
            float critRate = attackerNumeric.GetAsFloat(NumericType.CritChance); // 暴击率 (0.0 - 1.0)
            isCrit = RandomGenerator.RandFloat01() < critRate;
            if (isCrit)
            {
                // 触发暴击特效
                float critDamageMultiplier = attackerNumeric.GetAsFloat(NumericType.CritDamage); // 暴击伤害倍率（0.0 - x)
                finalDamage *= 1.0f + critDamageMultiplier; // 加成系数
            }
        }

        // 4. 计算穿透

        if (damageConfig.DamageType != DamageType.True)
        {
            mitigationFactor = CalcMitigationFactor(CalculateEffectiveDefense(baseDefence, PenetrationPercent, PenetrationFlat));
        }

        finalDamage *= mitigationFactor;

        // TODO: 应用全局伤害buff，需要时采用下面方案2即可。
        // 目前为止计算都没考虑角色身上的全局增伤BUFF。Numeric组件每个属性虽然有FinalAdd和FinalPct，但是都是针对基础属性的。
        // 1. 简单做法是把全局增伤BUFF直接加到所有属性上，那么前面获取Numeric属性时附带了，但是不优雅。
        // 2. 同样利用Numeric组件，增加一个全局增伤组件 (finalDamage + finalAdd) * (1 + finalPct)

        finalDamage += elementExtraDamage; // 加上元素额外伤害不参与穿透和防御计算，直接附加。

        finalDamage = Math.Max(finalDamage, 0); // 防止负伤害，即造成回血效果。比如元素克制反而回血，后期打不动。

        return new Damage()
        {
            DamageType = damageConfig.DamageType, Value = finalDamage, IsCritical = isCrit, Config = damageConfig
        };
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
    ///     计算元素伤害
    /// </summary>
    /// <param name="attacker"></param>
    /// <param name="target"></param>
    /// <param name="damageConfig"></param>
    /// <returns>返回伤害加成</returns>
    /// <exception cref="NotImplementedException"></exception>
    private static ReactionResult CheckElementalReaction(Unit attacker, Unit target, SkillDamageConfig damageConfig)
    {
        ReactionResult result = new() { DamageMultiplier = 1.0f };
        ElementalComponent targetElement = target.GetComponent<ElementalComponent>();
        if (targetElement != null)
        {
            targetElement.CheckAndTriggerReaction(attacker, DamageType.ToElementalType(damageConfig.DamageType), (float)damageConfig.FlatBaseValue,
                ref result);
        }

        return result;
    }

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

        foreach (Damage damage in data.DamageDetail)
        {
            // 1. 反甲效果
            if (damage.DamageType == DamageType.Physical)
            {
                float ratio = numeric.GetAsFloat(NumericType.ThornmailRate);
                if (ratio > 0.01)
                {
                    float reflectDamage = damage.Value * ratio;
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

        foreach (Damage damage in data.DamageDetail)
        {
            // 1. 吸血效果
            // 技能吸血怎么处理，用Buff还是直接计算？ 这里直接计算，buff只处理持续效果。
            if (damage.DamageType == DamageType.Physical || damage.DamageType == DamageType.Magical)
            {
                float ratio = damage.DamageType == DamageType.Physical ?
                        numeric.GetAsFloat(NumericType.LifestealRate) :
                        numeric.GetAsFloat(NumericType.MagicLifestealRate);
                if (damage.Config != null)
                {
                    ratio += (float)damage.Config.LifestealRate;
                }

                totalStealLife += damage.Value * ratio;
            }

            // 2. 其他攻击类被动都在这里加...
        }

        // 回血
        numeric[NumericType.Hp] += (long)totalStealLife;
    }

    #endregion
}