namespace ET
{
#if DEF_NPBehave
using NPBehave;

    public class SkillDamageAction : Action
    {
        private readonly string _casterKey;
        private readonly string _targetKey;
        private readonly int _skillId;
        
        public SkillDamageAction(string casterKey, string targetKey, int skillId) 
                : base("SkillDamageAction")
        {
            _casterKey = casterKey;
            _targetKey = targetKey;
            _skillId = skillId;
        }
        
        protected override Status DoStart(Blackboard blackboard)
        {
            var caster = blackboard.Get<Unit>(_casterKey);
            var target = blackboard.Get<Unit>(_targetKey);

            if (caster == null || target == null)
            {
                return Status.Failure;
            }

            // 异步执行伤害计算，避免阻塞行为树 Tick
            // 在实际 ET 项目中，建议通过 EventSystem 发布事件，或在 Action 内部调用异步方法
            ExecuteDamageLogic(caster, target);

            return Status.Success;
        }

        private void ExecuteDamageLogic(Unit caster, Unit target)
        {
            // 1. 获取技能配置 (假设从 ConfigComponent 获取)
            // var skillConfig = ConfigHelper.Instance.GetConfig<SkillConfig>(_skillId);
            // 为演示简化，硬编码一些值
            ElementType skillElement = ElementType.Fire; 
            float baseDamage = 100f;

            // 2. 获取目标当前元素状态
            var auraComp = target.GetComponent<ElementAuraComponent>();
            ElementType currentAura = ElementType.None;
            float currentGauge = 0f;
            
            if (auraComp != null && auraComp.HasAura())
            {
                currentAura = auraComp.ElementType;
                currentGauge = auraComp.Gauge;
            }
            
            // 3. 获取目标材质 (如果有 MaterialComponent)
            MaterialType targetMaterial = MaterialType.None;
            var matComp = target.GetComponent<MaterialComponent>();
            if (matComp != null)
            {
                targetMaterial = matComp.MaterialType;
            }

            // 4. 判定反应
            ReactionData reaction = null;
            bool isMaterialReaction = false;

            // 优先检查材质反应 (如果目标没有元素附着，但材质敏感)
            if (currentAura == ElementType.None)
            {
                reaction = ElementReactionManager.GetMaterialReaction(targetMaterial, skillElement);
                if (reaction != null) isMaterialReaction = true;
            }

            // 其次检查元素反应
            if (reaction == null && currentAura != ElementType.None)
            {
                ElementReactionManager.TryGetReaction(skillElement, currentAura, out reaction);
            }

            // 5. 计算最终伤害
            float multiplier = reaction?.DamageMultiplier ?? 1.0f;
            float finalDamage = baseDamage * multiplier;

            // 6. 应用伤害 (通过 NumericComponent 或 Event)
            ApplyDamage(target, finalDamage);

            // 7. 更新元素状态
            if (reaction != null)
            {
                HandleElementUpdate(target, reaction, skillElement, auraComp, isMaterialReaction);
            }
            else if (skillElement != ElementType.None)
            {
                // 无反应，直接附着
                ApplyAura(target, skillElement, 50f, 10f); // 50 gauge, 10s duration
            }

            // 8. 表现层通知 (客户端播放特效)
            NotifyVisuals(caster.Id, target.Id, reaction);
        }

        private void ApplyDamage(Unit target, float damage)
        {
            // 实际项目中：target.GetComponent<NumericComponent>().Subtract(NumericType.Hp, damage);
            Log.Debug($"Unit {target.Id} took {damage} damage.");
        }

        private void HandleElementUpdate(Unit target, ReactionData reaction, ElementType triggerElem, ElementAuraComponent auraComp, bool isMaterial)
        {
            // 消耗原有元素
            if (!isMaterial && auraComp != null && auraComp.HasAura())
            {
                auraComp.Consume(reaction.GaugeConsumption * 100f); // 假设满槽100
            }

            // 施加新元素 (如果有)
            if (reaction.ResultingElement != ElementType.None)
            {
                // 如果反应后还有残留元素，或者新生成的元素
                ApplyAura(target, reaction.ResultingElement, 50f, 8f);
            }
            else
            {
                // 如果反应结果是 None，且原元素被消耗完，则清除
                if (auraComp != null && !auraComp.HasAura())
                {
                    auraComp.Clear();
                }
            }
        }

        private void ApplyAura(Unit target, ElementType elem, float gauge, float duration)
        {
            var auraComp = target.AddComponent<ElementAuraComponent>();
            auraComp.SetAura(elem, gauge, duration);
        }

        private void NotifyVisuals(long casterId, long targetId, ReactionData reaction)
        {
            // 发布事件，客户端监听此事件播放特效
            // Game.EventSystem.Run(new CombatHitEvent { ... });
            if (reaction != null)
            {
                Log.Debug($"Reaction Triggered: {reaction.Type}, VFX: {reaction.VfxName}");
            }
        }
        
    }
#endif
}