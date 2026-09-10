namespace ET
{
    public static class SkillBehaviorTreeFactory
    {
        public static Root CreateSkillTree(SkillConfig config, Unit caster, Unit target)
        {
#if DEF_NPBehave
            var blackboard = new Blackboard();
            blackboard.Set("Caster", caster);
            blackboard.Set("Target", target);
            blackboard.Set("Config", config);

            // 1. 前摇
            var windup = new Wait(config.CastTime);

            // 2. 执行伤害与元素反应
            var executeDamage = new SkillDamageActionExtensions("Caster", "Target", "Config");

            // 3. 后摇
            var recovery = new Wait(0.5f);

            var sequence = new Sequence(windup, executeDamage, recovery);
            return new Root(sequence, blackboard: blackboard);
#endif
            return null;
        }
    }
}