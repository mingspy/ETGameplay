using System.Collections.Generic;

namespace ET
{
    [EntitySystemOf(typeof(BuffComponent))]
    [FriendOf(typeof(BuffComponent))]
    public static partial class BuffComponentSystem
    {
        [EntitySystem]
        private static void Awake(this BuffComponent self)
        {
            self.Unit = self.GetParent<Unit>();
            self.Buffs = new Dictionary<int, BuffInstance>();

            int cnt = BuffConfigCategory.Instance.GetAll().Count;
            Log.Info($"BuffComponentSystem  Awake, total BuffCount: {cnt} IScene {self.Scene()} {self.Fiber()} AppType {Options.Instance.AppType}");
        }

        [EntitySystem]
        private static void Update(this BuffComponent self)
        {
            // 每帧检查过期Buff，TODO: 后续改成用 TimerComponent 定时器完成。
            long now = TimeHelper.Now();
            var expiredKeys = new List<int>();
            foreach (var kvp in self.Buffs)
            {
                BuffConfig config = kvp.Value.Config;
                // 检查周期性
                if (config.Period > 0 && now >= kvp.Value.PeriodEndTime)
                {
                    self.ApplyBuffEffect(config, 1, kvp.Value);
                    kvp.Value.PeriodEndTime = now + TimeHelper.ToMS(config.Period);
                }

                // 检查过期
                if (config.DurationType == BuffDurationType.HasDuration && now >= kvp.Value.EndTime)
                {
                    expiredKeys.Add(kvp.Key);
                }
            }

            foreach (int key in expiredKeys)
            {
                self.RemoveBuff(key);
            }
        }

        public static bool HasBuff(this BuffComponent self, int buffId)
        {
            if (self.Buffs.TryGetValue(buffId, out BuffInstance info))
            {
                if (TimeHelper.Now() < info.EndTime)
                {
                    return true;
                }

                // 过期移除
                self.Buffs.Remove(buffId);
            }

            return false;
        }

        public static void AddBuff(this BuffComponent self, BuffConfig config, int stack = 1, int casterId = -1)
        {
            //var tp = (BuffType)config.BuffType;
            if (config.DurationType == BuffDurationType.Instant)
            {
                self.ApplyBuffEffect(config, stack, null);
                return;
            }

            long now = TimeHelper.Now();
            int buffId = config.Id;
            if (self.Buffs.TryGetValue(buffId, out BuffInstance info))
            {
                // 同类型可叠加则叠加层数，不可叠加则刷新时间
                if (info.Stacks < info.Config.MaxStacks)
                {
                    info.Stacks += stack;
                    self.ApplyBuffEffect(config, stack, info);
                }
                else
                {
                    info.EndTime = now + TimeHelper.ToMS(config.Duration);
                }
            }
            else
            {
                //var config = BuffConfigCategory.Instance.Get(buffId);
                self.Buffs[buffId] = new BuffInstance()
                {
                    BuffId = buffId,
                    OwnerId = self.Unit.Id,
                    CasterId = casterId,
                    Stacks = stack,
                    StartTime = now,
                    EndTime = now + TimeHelper.ToMS(config.Duration),
                    PeriodEndTime = now + TimeHelper.ToMS(config.Duration),
                    TotalEffects = new int [config.Numerics.Length],
                    Config = config
                };
                self.ApplyBuffEffect(config, stack, self.Buffs[buffId]);
            }

            // 触发Buff添加事件，可用于更新UI或行为树条件
            EventSystem.Instance.PublishAsync(self.Scene(), new OnBuffAddedEvent() { Unit = self.Unit, Buff = self.Buffs[buffId] }).Coroutine();
        }

        public static void RemoveBuff(this BuffComponent self, int buffId)
        {
            if (self.Buffs.ContainsKey(buffId))
            {
                BuffInstance info = self.Buffs[buffId];
                self.Buffs.Remove(buffId);
                self.RemoveBuffEffect(info);
                EventSystem.Instance.PublishAsync(self.Scene(), new OnBuffRemovedEvent() { Unit = self.Unit, Buff = self.Buffs[buffId] }).Coroutine();
            }
        }

        private static void ApplyBuffEffect(this BuffComponent self, BuffConfig config, int stack, BuffInstance instance)
        {
            NumericComponent NumericComponent = self.Unit.GetComponent<NumericComponent>();
            // TODO : 根据Buff类型设置状态
            if ((config.BuffType & BuffType.Numeric) == BuffType.Numeric)
            {
                for (int i = 0; i < config.Numerics.Length; i++)
                {
                    //var modifyType = config.ModifyTypes[i]; 
                    int numeric = config.Numerics[i];
                    int effect = config.EffectValues[i] * stack;
                    NumericComponent[numeric] += effect;
                    if (instance != null)
                    {
                        instance.TotalEffects[i] += effect;
                    }
                }
            }
        }

        private static void RemoveBuffEffect(this BuffComponent self, BuffInstance instance)
        {
            // TODO : 根据Buff类型移除状态
            NumericComponent NumericComponent = self.Unit.GetComponent<NumericComponent>();
            BuffConfig config = instance.Config;
            if ((config.BuffType & BuffType.Numeric) == BuffType.Numeric)
            {
                for (int i = 0; i < config.Numerics.Length; i++)
                {
                    int numeric = config.Numerics[i];
                    NumericComponent[numeric] -= instance.TotalEffects[i];
                }
            }
        }
    }
}