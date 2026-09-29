namespace ET.Module.Gameplay.Passive
{
    /// <summary>
    ///     鲁班七号被动技能 - 火力压制
    ///     效果：
    ///     1. 连续普通攻击4次后，第五次普攻变为扫射（强化普攻）
    ///     2. 扫射对前方范围内敌人造成3次伤害，每次造成最大生命值6%的物理伤害
    ///     3. 每级成长：额外AD加成
    ///     驱动机制：事件驱动
    ///     - 监听 AfterAttackHit 事件，普攻命中后计数+1
    ///     - 计数满4次后激活强化普攻状态
    ///     - 监听 BeforeAttackHit 事件，消耗强化状态执行强化普攻
    /// </summary>
    [FriendOf(typeof(BuffNode))]
    public class LubanPassiveHandler : ABuffHandler<BuffNode>
    {
        private const float MAX_HP_PERCENT_DAMAGE = 0.06f; // 6%最大生命值伤害
        private const int ENHANCE_OUT_TIME = 3000; // 3秒

        public const int NORMAL_ATTACK_TRIGGER_COUNT = 4; // 4次普攻后触发扫射
        public const int BURST_SEGMENT_COUNT = 3; // 扫射共3段
        public const float HERO_MAX_HP_PERCENT_BASE = 0.045f; // 对英雄基础百分比4.5%
        public const float HERO_EXTRA_AD_PERCENT_PER_POINT = 0.0001f; // 每点额外AD+0.01%
        public const float PRECISION_RATIO = 0.3f; // 每段吃30%精准
        public const float NON_HERO_AD_RATIO = 0.5f; // 非英雄享受50%AD

        public const float SECTOR_RANGE = 8f; // 扫射扇形范围800码（单位：米/格按项目比例）
        public const float SECTOR_ANGLE = 60f; // 扇形角度60度

        public LubanPassiveHandler(int handlerId = BuffConfigId.LubanPassive, int priority = 0) : base(handlerId, priority)
        {
        }

        // 对英雄固定基础值：1级70，15级140（简单线性成长示例）
        public static float GetHeroBaseDamage(int level)
        {
            return 70f + 5f * (level - 1);
        }

        // 对非英雄固定基础值：1级120，15级240
        public static float GetNonHeroBaseDamage(int level)
        {
            return 120f + 120f / 14f * (level - 1);
        }

        public override async ETTask OnAdd(BuffComponent self, BuffNode buffNode)
        {
            buffNode.Name = "火力压制";
            buffNode.Type = BuffType.EventTrigger;
            buffNode.IsDispellable = false; // 英雄被动不能被驱散
            buffNode.Tags.Add("passive");
            buffNode.Tags.Add("luban");

            // 监听普攻命中事件（计数）和普攻前事件（强化普攻触发）
            buffNode.ListenEvents.Add(DamageStage.BeforeHit);
            buffNode.ListenEvents.Add(DamageStage.AfterHit);
            buffNode.ListenEvents.Add(DamageStage.ModifyCalcRawDamage);
            //buffNode.ListenEvents.Add(DamageStage.GatherBonusDamage);

            Unit owner = self.GetParent<Unit>();
            StateComponent state = owner.GetComponent<StateComponent>();
            if (state != null)
            {
                state.SetPassiveData("luban_normal_attack_count", 0);
                state.SetPassiveData("luban_enhanced_ready", false);
            }

            Log.Info($"[鲁班被动] 火力压制已激活");

            await ETTask.CompletedTask;
        }

        public override async ETTask OnRemove(BuffComponent self, BuffNode buffNode)
        {
            Log.Info($"[鲁班被动] 火力压制被移除");
            await ETTask.CompletedTask;
        }

        public override async ETTask OnIntervalTick(BuffComponent self, BuffNode buffNode)
        {
            await ETTask.CompletedTask;
        }

        public override async ETTask OnEvent<T>(BuffComponent self, BuffNode buffNode, T eventData)
        {
            Unit owner = self.GetParent<Unit>();
            StateComponent state = owner.GetComponent<StateComponent>();
            if (state == null)
            {
                return;
            }

            NumericComponent numeric = owner.GetComponent<NumericComponent>();
            if (numeric == null)
            {
                return;
            }

            bool isEnhanced = state.GetPassiveData<bool>("luban_enhanced_ready");

            void SetEnhance(bool flag)
            {
                state.SetPassiveData("luban_enhanced_ready", flag);
                state.SetPassiveData("luban_normal_attack_count", 0);
                state.SetPassiveData("luban_normal_attack_time", TimeHelper.Now());
            }

            DamageContext context = eventData.Context;
            switch (eventData.Stage)
            {
                case DamageStage.ModifyCalcRawDamage:
                    // 鲁班三段强化普攻时，重新修改计算公式
                    if (context.Data.Skill.SkillType == SkillType.Common && isEnhanced)
                    {
                        int ownerLevel = numeric.GetAsInt(NumericType.Level);
                        // ====== 按目标类型套用对应公式计算裸伤 ======
                        if (context.Target.Type() == UnitType.Player)
                        {
                            // 英雄公式：单段裸伤 = 固定基础值(随等级成长) 
                            // + 目标最大生命值 × (基础百分比 + 额外物理攻击 × 0.01%)
                            // + 精准值 × 30%

                            float percent = HERO_MAX_HP_PERCENT_BASE + numeric.GetAsInt(numeric.AttributeAdd(NumericType.Attack)) * HERO_EXTRA_AD_PERCENT_PER_POINT;
                            context.Data.FinalDamage = context.Data.RawDamage = GetHeroBaseDamage(ownerLevel) + context.Data.TargetMaxHp * percent
                                    + PRECISION_RATIO * numeric.GetAsInt(NumericType.Precision);
                        }
                        else
                        {
                            // 非英雄公式：单段裸伤 = 120 ~ 240(随等级) + 50% × 物理攻击
                            context.Data.FinalDamage = context.Data.RawDamage = GetNonHeroBaseDamage(ownerLevel)
                                    + numeric.GetAsInt(NumericType.Attack) * NON_HERO_AD_RATIO;
                        }
                    }
                    break;
                case DamageStage.AfterHit:
                    // 技能命中后，设置强化普攻
                    if (context.Data.Skill.SkillType == SkillType.Skill)
                    {
                        SetEnhance(true);
                    }
                    else if (isEnhanced) // 消化强普攻
                    {
                        SetEnhance(false);
                    }
                    else
                    {
                        // 普攻命中后：计数+1
                        int count = state.GetPassiveData<int>("luban_normal_attack_count");
                        count++;
                        Log.Info($"[鲁班被动] 普攻命中计数: {count}/{NORMAL_ATTACK_TRIGGER_COUNT}");
                        if (count >= NORMAL_ATTACK_TRIGGER_COUNT)
                        {
                            // 获得强化普攻
                            SetEnhance(true);
                            count = 0;
                            Log.Info($"[鲁班被动] === 强化扫射已就绪！下一次普攻变为扫射 ===");
                        }

                        state.SetPassiveData("luban_normal_attack_count", count);
                    }

                    break;
                case DamageStage.BeforeHit:
                    // 检查强化普攻时间
                    if (context.Data.Skill.SkillType == SkillType.Common)
                    {
                        long lastTime = state.GetPassiveData<long>("luban_normal_attack_time");
                        if (isEnhanced && TimeHelper.Now() - lastTime > ENHANCE_OUT_TIME)
                        {
                            SetEnhance(false);
                            isEnhanced = false;
                        }

                        // 强化普攻
                        if (isEnhanced)
                        {
                            // 1. 拦截原普攻，阻止默认普攻伤害落地。
                            // TODO: 这种拦截设计不合理，应该由【逻辑动画事件】触发三次扫射。计算伤害应该是线性的，简单计算，而不是业务逻辑。
                            eventData.IsHandled = true;

                            // 2. 获取扇形范围内所有敌对单位（AOE判定）
                            var targets = BattleHelper.GetUnitsInSector(owner.Position, owner.Forward, SECTOR_RANGE, SECTOR_ANGLE, owner.Camp, CampType.Enemy);
                            // 3. 对每个目标发射3段独立伤害（每段单独走管线）
                            //    注：实际项目应根据攻速档位添加段间延迟，支持移动/技能打断，示例简化为瞬发
                            //  TODO: 分成三段触发，而不是立刻执行；这里只是简化实现，为了验证BUFF的事件系统。
                            for (int seg = 0; seg < BURST_SEGMENT_COUNT; seg++)
                            {
                                foreach (Unit target in targets)
                                {
                                    NumericComponent targetNumeric = target.GetComponent<NumericComponent>();
                                    DamageContext ctx = ObjectPool.Instance.Fetch<DamageContext>();
                                    ctx.Copy(context);
                                    ctx.Target = target;
                                    ctx.Data.TargetMaxHp = targetNumeric[NumericType.MaxHp];
                                    ctx.Data.TargetCurrentHp = targetNumeric[NumericType.Hp];

                                    // 4. 每段单独走完整伤害管线！！
                                    //    这一句就自动处理了：暴击、法球、末世、防御、破军、吸血、反伤...
                                    float segDamage = DamageHelper.ResolveDamage(owner, target, ctx);
                                    Log.Debug($"鲁班扫射第{seg + 1}段对{target.Id}造成{segDamage:F1}伤害");
                                }
                            }
                        }
                    }

                    break;
            }
        }
    }
}