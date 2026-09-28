namespace ET.Module.Gameplay.Passive
{
    /// <summary>
    /// 鲁班七号被动技能 - 火力压制
    /// 效果：
    /// 1. 连续普通攻击4次后，第五次普攻变为扫射（强化普攻）
    /// 2. 扫射对前方范围内敌人造成3次伤害，每次造成最大生命值6%的物理伤害
    /// 3. 每级成长：额外AD加成
    /// 
    /// 驱动机制：事件驱动
    /// - 监听 AfterAttackHit 事件，普攻命中后计数+1
    /// - 计数满4次后激活强化普攻状态
    /// - 监听 BeforeAttackHit 事件，消耗强化状态执行强化普攻
    /// </summary>
    [FriendOf(typeof(BuffNode))]
    public class LubanPassiveHandler : ABuffHandler<BuffNode>
    {
        private const int ATTACK_COUNT_NEEDED = 4;
        private const float MAX_HP_PERCENT_DAMAGE = 0.06f; // 6%最大生命值伤害
        private const int ENHANCE_OUT_TIME = 3000; // 3秒

        public LubanPassiveHandler(int handlerId = BuffConfigId.LubanPassive, int priority = 0) : base(handlerId, priority)
        {
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
            buffNode.ListenEvents.Add(DamageStage.BeforeCalcRawDamage);
            buffNode.ListenEvents.Add(DamageStage.GatherBonusDamage);

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
            if (state == null) return;

            NumericComponent numeric = owner.GetComponent<NumericComponent>();
            if (numeric == null) return;
            
            bool isEnhanced = state.GetPassiveData<bool>("luban_enhanced_ready");
            
            void SetEnhance(bool flag)
            {
                state.SetPassiveData("luban_enhanced_ready", flag);
                state.SetPassiveData("luban_normal_attack_count", 0);
                state.SetPassiveData("luban_normal_attack_time", TimeHelper.Now());
            }
            
            switch (eventData.Stage)
            {
                case DamageStage.OnSkillCast:
                    break;
                case DamageStage.AfterHit:
                    // 技能命中后，设置强化普攻
                    if ( eventData.Context.Skill.SkillType == SkillType.Skill) 
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
                        Log.Info($"[鲁班被动] 普攻命中计数: {count}/{ATTACK_COUNT_NEEDED}");
                        if (count >= ATTACK_COUNT_NEEDED)
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
                    if ( eventData.Context.Skill.SkillType == SkillType.Common) 
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
                            // 1. 拦截原普攻，阻止默认普攻伤害落地
                            eventData.IsHandled = true;
                            
                            // 2. 获取扇形范围内所有敌对单位（AOE判定）
                            var targets = BattleHelper.GetUnitsInSector(
                                owner.Position, owner.Forward,
                                LubanPassiveConfig.SECTOR_RANGE,
                                LubanPassiveConfig.SECTOR_ANGLE,
                                owner.Camp, CampType.Enemy);
                            // 3. 对每个目标发射3段独立伤害（每段单独走管线）
                            //    注：实际项目应根据攻速档位添加段间延迟，支持移动/技能打断，示例简化为瞬发
                            for (int seg = 0; seg < LubanPassiveConfig.BURST_SEGMENT_COUNT; seg++)
                            {
                                foreach (Unit target in targets)
                                {
                                    var targetNumeric = target.GetComponent();
                                    var ctx = new DamageContext
                                    {
                                        AttackerId = owner.Id,
                                        DefenderId = target.Id,
                                        IsPhysical = true,
                                        IsNormalAttack = true,
                                        IsEnhancedAttack = true,
                                        CanCrit = true, // 每段独立判定暴击
                                        TargetType = target.UnitType,
                                        TargetCurrentHp = targetNumeric.CurrentHp,
                                        TargetMaxHp = targetNumeric.GetFinalValue(NumericType.MaxHp),
                                        PrecisionMultiplier = LubanPassiveConfig.PRECISION_RATIO, // 30%精准
                                    };

                                    // ====== 按目标类型套用对应公式计算裸伤 ======
                                    if (target.UnitType == UnitType.Hero)
                                    {
                                        // 英雄公式：固定值 + 最大生命百分比
                                        float percent = LubanPassiveConfig.HERO_MAX_HP_PERCENT_BASE
                                                + numeric.GetExtraAd() * LubanPassiveConfig.HERO_EXTRA_AD_PERCENT_PER_POINT;
                                        ctx.RawBaseDamage = LubanPassiveConfig.GetHeroBaseDamage(owner.Level)
                                                + ctx.TargetMaxHp * percent;
                                    }
                                    else
                                    {
                                        // 非英雄公式：固定值 + 50%AD
                                        ctx.RawBaseDamage = LubanPassiveConfig.GetNonHeroBaseDamage(owner.Level)
                                                + numeric.GetFinalValue(NumericType.Attack)
                                                * LubanPassiveConfig.NON_HERO_AD_RATIO;
                                    }

                                    // 4. 每段单独走完整伤害管线！！
                                    //    这一句就自动处理了：暴击、法球、末世、防御、破军、吸血、反伤...
                                    float segDamage = DamageResolver.Resolve(scene, ref ctx);
                                    Log.Debug($"鲁班扫射第{seg + 1}段对{target.Name}造成{segDamage:F1}伤害");
                                }
                            }

                            SetEnhance(false);
                        }
                    }

                    break;
            }
            
        }
    }
}