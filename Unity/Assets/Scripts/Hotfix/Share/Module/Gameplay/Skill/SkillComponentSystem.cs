using System.Collections.Generic;

namespace ET
{
    [EntitySystemOf(typeof(SkillComponent))]
    [FriendOf(typeof(SkillComponent))]
    public static partial class SkillComponentSystem
    {
        [EntitySystem]
        private static void Awake(this SkillComponent self)
        {
            Log.Info($"SkillComponentSystem Awake, total skills {SkillConfigCategory.Instance.GetAll().Count}");
            self.Unit = self.GetParent<Unit>();
#if DEF_NPBehave
            self.ActiveTrees = new Dictionary<int, Root>();
#endif
            self.Cooldowns = new Dictionary<int, long>();
        }

        public static bool CanCast(this SkillComponent self, int skillId)
        {
            // 1. 检查CD
            if (self.Cooldowns.TryGetValue(skillId, out long cdEnd))
            {
                if (TimeHelper.Now() < cdEnd)
                {
                    return false;
                }
            }

            // 2. 检查Buff限制 (例如: 沉默状态下不可施法)
            BuffComponent buffComp = self.Unit.GetComponent<BuffComponent>();
            if (buffComp != null && buffComp.HasBuff(9999)) // 假设9999是沉默Buff
            {
                return false;
            }

            return true;
        }

        public static void CastSkill(this SkillComponent self, int skillId, long targetId)
        {
            if (!self.CanCast(skillId))
            {
                return;
            }

            Unit target = self.Unit.GetParent<UnitComponent>().Get(targetId);
            if (target == null)
            {
                return;
            }

            SkillConfig config = SkillConfigCategory.Instance.Get(skillId);
            if (config == null)
            {
            }
            // 设置CD
#if DEF_NPBehave
            self.Cooldowns[skillId] = TimeHelper.Now() + TimeHelper.ToMS(config.CoolDown);

            // 创建并启动服务端行为树

            var serverTree = SkillBehaviorTreeFactory.CreateSkillTree(config, self.Unit, target);
            serverTree.Start();
            self.ActiveTrees[skillId] = serverTree;
#endif
        }

        /// <summary>
        ///     中断技能后摇
        /// </summary>
        public static void CancelRecovery(this SkillComponent self, int skillId)
        {
#if DEF_NPBehave
            if (self.ActiveTrees.TryGetValue(skillId, out var tree))
            {
                tree.Stop(); // NPBehave Stop会中断当前节点
                self.ActiveTrees.Remove(skillId);
            }
#endif
        }
    }
}