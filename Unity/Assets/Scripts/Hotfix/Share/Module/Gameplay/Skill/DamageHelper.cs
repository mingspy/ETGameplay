using System;

namespace ET
{
    #region Timer

    [Invoke(TimerInvokeType.DelayTakeDamage)]
    public class SkillDalyDamageTimer : ATimer<DamageContext>
    {
        protected override void Run(DamageContext context)
        {
            try
            {
                context.Target.TakeDamage(context.Source, context);
                ObjectPool.Instance.Recycle(context);
            }
            catch (Exception e)
            {
                Log.Error($"SkillDalyDamageTimer error:\n{e}");
            }
        }
    }

    #endregion

    [FriendOf(typeof(Unit))]
    public static class DamageHelper
    {
        /// <summary>
        ///     技能释放命中目标时调用
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="target"></param>
        /// <param name="skill"></param>
        public static void DealSkillDamage(this Unit attacker, Unit target, SkillNode skill)
        {
            BuffComponent buffComp = attacker.GetComponent<BuffComponent>();
            
            DamageContext skillContext = ObjectPool.Instance.Fetch<DamageContext>();
            skillContext.Skill = skill;
            buffComp.PublishEvent(new DamageEvent(DamageStage.BeforeHit, skillContext));
            
            foreach (SkillDamageConfig damageConfig in skill.Damages)
            {
                DamageContext context = ObjectPool.Instance.Fetch<DamageContext>();
                context.Reset();
                context.Source = attacker;
                context.Target = target;
                context.DamageType = (DamageType)damageConfig.DamageType;
                context.BaseDamage = damageConfig.BaseDamage;
                context.Coefficient = (float)damageConfig.Coefficient;
                context.SetStage(DamageStage.GatherCriticalRoll, damageConfig.CanCrit != 0); 
                context.Skill = skill;
                context.DamageTriggerType = (DamageTriggerType)damageConfig.DamageTriggerType; // TODO: 迁移到Skill配置
                context.DamageDelayTime = damageConfig.DelayTime; // TODO: 迁移到Skill配置

                ResolveDamage(attacker, target, context);
            }
            
            buffComp.PublishEvent(new DamageEvent(DamageStage.AfterHit, skillContext));
            ObjectPool.Instance.Recycle(skillContext);
        }

        #region Calculate Damage

        public static void ResolveDamage(Unit attacker, Unit target, DamageContext context)
        {
            if (target == null) return;
            
            CalcDamage(attacker, target, context);
            if (context.IsHandled)
            {
                ObjectPool.Instance.Recycle(context);
                return;
            }
            
            switch (context.DamageTriggerType)
            {
                case DamageTriggerType.Immediate:
                    // 瞬时伤害，直接立即执行
                    target.TakeDamage(attacker, context);
                    ObjectPool.Instance.Recycle(context);
                    break;
                case DamageTriggerType.Delay:
                    // 延迟伤害，注册定时器到时间后执行
                    attacker.Root().GetComponent<TimerComponent>().NewOnceTimer(TimeHelper.Now() + context.DamageDelayTime, TimerInvokeType.DelayTakeDamage, context);
                    break;
                case DamageTriggerType.OverTime:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
        ///     计算过程：RawDamage → 元素反应倍率/debuff应用 → 暴击判定 → 穿透/防御减伤 → 控制/附加效果<br />
        ///     根据反应类型给当前伤害加对应倍率、给目标挂载对应debuff，然后继续走原有的暴击、穿透、防御计算即可。<br />
        ///     【简化元素反应】，数值上只做简单的加成和反应BUFF，不消耗元素。视觉上，保留元素反应结果，增加趣味性。
        /// </summary>
        public static void CalcDamage(Unit attacker, Unit target, DamageContext context)
        {
            BuffComponent buffComp = attacker.GetComponent<BuffComponent>();
            NumericComponent attackerNumeric = attacker.GetComponent<NumericComponent>();
            // Step 1. 计算原始伤害  Raw Damage = Base + (AD/AP × Ratio) = 技能基础伤害 + 攻击力 * 技能系数
            if (context.HasStage(DamageStage.BeforeCalcRawDamage))
            {
                buffComp.PublishEvent(new DamageEvent(DamageStage.BeforeCalcRawDamage, context));
                context.FinalDamage = context.RawDamage = CalcRawDamage(attackerNumeric, context.DamageType, context.Coefficient, context.BaseDamage);
            }
            
            // Step 2. 暴击判定，一般只有普攻和强化普攻有暴击，法术伤害和真伤无暴击。不过这里通过配置控制。
            if (context.HasStage(DamageStage.GatherCriticalRoll))
            {
                float critRate = attackerNumeric.GetAsFloat(NumericType.CritChance); // 暴击率 (0.0 - 1.0)
                if (RandomGenerator.RandFloat01() < critRate)
                {
                    // 触发暴击特效
                    context.IsCritical = true;
                    context.CriticalMultiplier = Math.Max(1.0f, attackerNumeric.GetAsFloat(NumericType.CritDamagePct));
                    // 通知修改暴击效果，比如猴子初始暴击效果限定150%，随等级增长
                    buffComp.PublishEvent(new DamageEvent(DamageStage.GatherCriticalRoll, context));
                    context.FinalDamage *= context.CriticalMultiplier;
                }
            }

            // Step 3.0 计算元素伤害。
            if (context.HasStage(DamageStage.CalcElement))
            {
                ReactionInfo reactionResult = CalcElementDamage(attacker, target, context.DamageType, context.BaseDamage);
                if (reactionResult != null)
                {
                    context.FinalDamage *= reactionResult.DamageMultiplier;
                    context.FinalDamage += reactionResult.ExtraDamage;
                    context.ReactionResult = reactionResult;
                }
            }
            
            // 剩余阶段目前都不绕过

            // Step 3.1 计算伤害增强，主要用于计算强化普攻额外附加的那段伤害，以及装备法球，在这里并入输出值。注意规则差异：
            //  强化普攻的额外附伤‌：全额加入，但‌不享受暴击‌
            //  装备法球‌（末世破败、闪电匕首电弧、吸血类附伤）：作为‌独立伤害实例‌加入，走同一条后续流程
            buffComp.PublishEvent(new DamageEvent(DamageStage.GatherBonusDamage, context));
            context.FinalDamage += context.BonusDamage;

            //  Step 4. 受害者防御减免计算
            NumericComponent targetNumeric = target.GetComponent<NumericComponent>();
            context.MitigationMultiplier = CalcDefenseMitigation(attackerNumeric, targetNumeric, context.DamageType);
            buffComp.PublishEvent(new DamageEvent(DamageStage.GatherDamageMitigation, context));
            context.FinalDamage *= context.MitigationMultiplier;

            /*  Step 5. 攻击者全局增强 buff
               攻击方所有"造成伤害提升"类效果在此‌线性相加‌成一个系数：
               增伤系数 = 1 + Σ(攻击方增伤)
               代表：暴烈之甲无畏被动（最高+10%）、狂暴召唤技、逐日之弓等。实测已证实‌暴烈之甲与破军同时触发时是加法‌（10%+30%=40%）。
               破军本质是"‌条件型增伤‌"，也在此处理。社区实测还发现一个异常：破军对‌末世法球‌的加成约为127.8%，而非理论130%，说明装备法球与破军的联动存在小额偏差。
            */
            buffComp.PublishEvent(new DamageEvent(DamageStage.GatherOutgoingDamage, context));
            context.FinalDamage *= context.AmplifyMultiplier;

            context.FinalDamage = Math.Max(context.FinalDamage, 0); // 防止负伤害，即造成回血效果。比如元素克制反而回血，后期打不动。
        }

        /// <summary>
        ///     根据角色攻击力计算原始伤害，原始伤害 = 角色攻击力值 * 技能加成比例 + 技能基础伤害。
        /// </summary>
        /// <param name="attackerNumeric"></param>
        /// <param name="damageType">伤害类型</param>
        /// <param name="coefficient">技能加成比例</param>
        /// <param name="baseDamage">技能基础伤害</param>
        /// <returns></returns>
        private static float CalcRawDamage(NumericComponent attackerNumeric, DamageType damageType, float coefficient, float baseDamage)
        {
            // 伤害类型获取对应的攻击力
            int numericAttr = DamageType.True == damageType ? NumericType.TrueDamage : NumericType.Placeholder_DamageStart + (int)damageType;
            float attackPower = attackerNumeric.GetAsInt(numericAttr);
            return attackPower * coefficient + baseDamage;
        }

        private static ReactionInfo CalcElementDamage(Unit attacker, Unit target, DamageType damageType, float baseDamage, int currentDepth = 0)
        {
            if (!DamageTypeHelper.IsElementDamage(damageType))
            {
                return null;
            }

            ElementalComponent targetElement = target.GetComponent<ElementalComponent>();
            return targetElement.TryReactOrAppendElement(attacker, DamageTypeHelper.ToElement(damageType), baseDamage, currentDepth);
        }

        /// <summary>
        ///     计算防御对伤害的降低系数。一般真伤防御设置成0，所以走这步计算也是可行的，如果后期真伤也可以防御..不需要改动逻辑。
        /// </summary>
        /// <param name="attackerNumeric"></param>
        /// <param name="targetNumeric"></param>
        /// <param name="damageType"></param>
        /// <returns></returns>
        private static float CalcDefenseMitigation(NumericComponent attackerNumeric, NumericComponent targetNumeric, DamageType damageType)
        {
            int numericDelta = (int)damageType;
            // 目标抗性：(护甲，魔抗 or 元素抗性)
            float targetDefense = targetNumeric.GetAsInt(NumericType.Placeholder_ResistStart + numericDelta);
            // 攻击者穿透比例
            float PenetrationPercent = attackerNumeric.GetAsFloat(NumericType.Placeholder_PctPenStart + numericDelta);
            // 攻击者 穿透面板属性
            float PenetrationFlat = attackerNumeric.GetAsInt(NumericType.Placeholder_FlatPenStart + numericDelta);

            //  1. 计算目标有效防御
            float finalDefence = CalcEffectiveDefense(targetDefense, PenetrationPercent, PenetrationFlat);
            // 2. 应用经典减伤公式
            return ClassicalDefenseMitigationFactor(finalDefence, SkillDefs.DefenseMitigation_C);
        }

        /// <summary>
        ///     有效防御 = 基础防御 * ( 1 - 穿透比例) - 穿透固定值
        /// </summary>
        /// <param name="baseDef"></param>
        /// <param name="penPercent"></param>
        /// <param name="penFlat"></param>
        /// <returns></returns>
        private static float CalcEffectiveDefense(float baseDef, float penPercent, float penFlat)
        {
            float afterFlat = baseDef * (1 - penPercent) - penFlat;
            return Math.Max(afterFlat, 0); // 防御最低为0
        }

        // 辅助：经典减伤系数
        private static float ClassicalDefenseMitigationFactor(float defense, float C)
        {
            return C / (C + defense);
        }

        #endregion

        #region Unit take damage

        /// <summary>
        ///     受到伤害，减血，并触发被动技能，如反甲
        /// </summary>
        public static void TakeDamage(this Unit target, Unit attacker, DamageContext context)
        {
            BuffComponent targetBuffComp = target.GetComponent<BuffComponent>();
            /*Step 6｜受害者全局 buff（减伤 / 易伤）
            受击方的"降低敌方输出"类效果在此线性相加：
               减伤‌（作用目标是敌方，描述为"减少敌人输出"）：项羽二技能最高45%、专精张飞二技能50%、娜可露露大招等
               易伤‌（增加受到的伤害）：如部分英雄的"受到伤害增加"debuff
            关键规则：‌减伤没有上限‌，理论上最多可降低敌人100%输出（专精张飞+项羽实测可达95%）。
            */
            targetBuffComp.PublishEvent(new DamageEvent(DamageStage.GatherIncomingDamage, context));
            context.FinalDamage *= context.ReduceMultiplier;

            /*Step 7｜受害者免伤触发（名刀、免疫等）
               这一层是"自身减少受到的伤害"，与 Step 6 属于‌不同类‌，因此与 Step 6 是‌相乘‌关系：
               免伤后伤害 = 伤害 × (1 - Σ自身免伤)
               自身免伤上限 = 90%
               代表：老夫子二技能、花木兰重剑形态、纯净苍穹主动、夏洛特被动。
               纯净苍穹同时带"自身免伤"与"降低敌方输出"两种效果，正好能验证"异类相乘"：
               ‌名刀司命是这一层的特例‌——它不是按百分比减免，而是"致命伤害拦截"：当受到‌致命伤害‌时触发，进入短暂无敌（近战1秒/远程0.5秒）并加30%移速，冷却90秒。它判定的是"这一击是否致死"，因此本质上是在结算最末端的一次"拦截判定"，而不是普通免伤。
            */
            targetBuffComp.PublishEvent(new DamageEvent(DamageStage.JudgeDamageNegation, context));
            context.FinalDamage *= 1 - context.NegationFactor; // 如纯净苍穹伤害减免

            if (context.IsNegated) //触发名刀 或者 净化
            {
                context.FinalDamage = 0;
                return;
            }

            //这里的作用是施加元素或者触发扩散等。元素的伤害已经包含在FinalDamage内了。
            if (DamageTypeHelper.IsElementDamage(context.DamageType) && context.ReactionResult != null)
            {
                target.GetComponent<ElementalComponent>().ApplyReactionEffects(attacker, context.ReactionResult, context.ReactionResult.CurrentDepth);
            }

            BuffComponent attackerBuffComp = attacker.GetComponent<BuffComponent>();
            // Step 8｜攻击者斩杀判定
            attackerBuffComp.PublishEvent(new DamageEvent(DamageStage.JudgeExecute, context));

            // 扣血处理
            NumericComponent numeric = target.GetComponent<NumericComponent>();
            int oldHp = numeric.GetAsInt(NumericType.Hp);
            int currentHp = oldHp - (int)context.FinalDamage;

            if (context.IsExecuted || currentHp <= 0)
            {
                target.Dead(context);
                currentHp = 0;
            }

            // 扣血
            numeric[NumericType.Hp] = currentHp;
            int realDamage = oldHp - currentHp;
            context.FinalDamage = realDamage;
            Log.Info($"[Damage] {target.Id} 受到伤害: {realDamage:F1}, 剩余血量: {currentHp:F1}/{numeric[NumericType.MaxHp]:F1}");

            /* Step 9｜吸血装备
               吸血发生在最终伤害确定之后，按实际造成的伤害回血：
               装备回血 = 最终伤害 × 装备吸血率
               - 面板吸血率上限100%（再多装备也封顶）
               - 泣血之刃提供 25% 物理吸血，是物理续航核心
             */
            attackerBuffComp.PublishEvent(new DamageEvent(DamageStage.BeforeLifeSteal, context));
            /* TODO: Step 10｜吸血技能

               技能吸血与普攻吸血分开算，且按技能范围打折：
               | 技能范围 | 吸血系数 |
               |---------|---------|
               | 单体技能 | 100% |
               | 小范围技能 | 75% |
               | 中范围技能 | 50% |
               | 大范围技能 | 35% |
               技能回血 = 最终伤害 × 技能吸血率 × 范围系数
               重伤会削弱回复（含吸血）：S29 起统一为加法公式：
               实际回复 = 基础回复 × (1 + 治疗增益率 - 重伤率)
               制裁之刃与梦魇之牙是两件重伤装备（当前版本重伤比例约35%，早期为50%）：
             */

            attacker.Heal(context.LifeStealAmount);

            /* Step 11｜攻击者和受击者被动

               伤害落地后触发各类"响应型"被动：
               - 受击方：反伤刺甲反弹物理伤害、不祥征兆减攻速移速、极寒风暴减速
               - 攻击方：末世破败已在前置结算，其余命中触发的增益/减益在此落地
               反伤刺甲是受击方被动反弹的代表：
             */
            targetBuffComp.PublishEvent(new DamageEvent(DamageStage.OnDamageTaken, context));
            attackerBuffComp.PublishEvent(new DamageEvent(DamageStage.OnDamageApplied, context)); // 技能命中后事件（触发妲己被动等）
        }

        public static void Dead(this Unit target, DamageContext context)
        {
        }

        /// <summary>
        ///     治疗
        /// </summary>
        public static float Heal(this Unit self, float healAmount)
        {
            NumericComponent numeric = self.GetComponent<NumericComponent>();
            // 制裁效果：治疗降低
            float healReduction = numeric.GetAsFloat(NumericType.HealReduction);
            healAmount *= 1 - healReduction;
            int oldHp = numeric.GetAsInt(NumericType.Hp);
            int maxHp = numeric.GetAsInt(NumericType.MaxHp);
            int currentHp = Math.Min(maxHp, oldHp + (int)healAmount);
            numeric[NumericType.Hp] = currentHp;
            int realHeal = currentHp - oldHp;
            Log.Info($"[Heal] {self.Id} 恢复血量: {realHeal:F1}, 当前血量: {currentHp:F1}");
            return realHeal;
        }

        #endregion
    }
}