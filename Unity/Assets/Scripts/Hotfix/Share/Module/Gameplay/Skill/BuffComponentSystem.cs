using System.Collections.Generic;
using System.Runtime.CompilerServices;

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

            var cnt = BuffConfigCategory.Instance.GetAll().Count;
            Log.Info($"BuffComponentSystem  Awake, total BuffCount: {cnt} IScene {self.Scene()} {self.Fiber()} AppType {Options.Instance.AppType}");
        }
        
        
        [EntitySystem]
        private static void Update(this BuffComponent self)
        {
            // 每帧检查过期Buff，TODO: 后续改成用 TimerComponent 定时器完成。
            var now = TimeInfo.Instance.ServerFrameTime();
            var expiredKeys = new List<int>();
            foreach (var kvp in self.Buffs)
            {
                var config = kvp.Value.Config;
                // 检查周期性
                if (config.Period > 0 && now >= kvp.Value.PeriodEndTime)
                {
                    self.ApplyBuffEffect(config, 1, kvp.Value);
                    kvp.Value.PeriodEndTime = now + (float)config.Period;
                }
                
                // 检查过期
                if (config.DurationType == BuffDurationType.HasDuration && now >= kvp.Value.EndTime)
                {
                    expiredKeys.Add(kvp.Key);
                }
            }
            
            foreach (var key in expiredKeys)
            {
                self.RemoveBuff(key);
            }
        }
        
        public static bool HasBuff(this BuffComponent self, int buffId)
        {
            if (self.Buffs.TryGetValue(buffId, out var info))
            {
                if (TimeInfo.Instance.ServerFrameTime() < info.EndTime) return true;
                // 过期移除
                self.Buffs.Remove(buffId);
            }
            return false;
        }
        
        public static void AddBuff(this BuffComponent self, BuffConfig config, int stack = 1, int casterId = -1 )
        {
            //var tp = (BuffType)config.BuffType;
            if (config.DurationType == BuffDurationType.Instant)
            {
                self.ApplyBuffEffect(config, stack, null);
                return;
            }
            
            var now = TimeInfo.Instance.ServerFrameTime();
            int buffId = config.Id;
            if (self.Buffs.TryGetValue(buffId, out var info))
            {
                // 同类型可叠加则叠加层数，不可叠加则刷新时间
                if (info.Stacks < info.Config.MaxStacks)
                {
                    info.Stacks += stack;
                    self.ApplyBuffEffect(config, stack, info);
                }
                else
                {
                    info.EndTime = now + (float)config.Duration; 
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
                    EndTime = now + (float)config.Duration,
                    PeriodEndTime = now + (float)config.Duration,
                    TotalEffects = new int [config.Numerics.Length],
                    Config = config
                };
                self.ApplyBuffEffect(config, stack, self.Buffs[buffId]);
            }
            // 触发Buff添加事件，可用于更新UI或行为树条件
            EventSystem.Instance.PublishAsync(self.Scene(), new OnBuffAddedEvent(){Unit = self.Unit, Buff = self.Buffs[buffId]}).Coroutine();
        }
        

        public static void RemoveBuff(this BuffComponent self, int buffId)
        {
            if (self.Buffs.ContainsKey(buffId))
            {
                var  info = self.Buffs[buffId];
                self.Buffs.Remove(buffId);
                self.RemoveBuffEffect(info);
                EventSystem.Instance.PublishAsync(self.Scene(), new OnBuffRemovedEvent(){Unit = self.Unit, Buff = self.Buffs[buffId]}).Coroutine();
            }
        }

        private static void ApplyBuffEffect(this BuffComponent self, BuffConfig config, int stack, BuffInstance instance)
        {
            var NumericComponent = self.Unit.GetComponent<NumericComponent>();
            // TODO : 根据Buff类型设置状态
            var buffType = (BuffType)config.BuffType;
            if (buffType == BuffType.Numeric)
            {
                for (int i = 0; i < config.Numerics.Length; i++)
                {
                    //var modifyType = config.ModifyTypes[i]; 
                    var numeric = config.Numerics[i];
                    var effect = config.EffectValues[i] * stack;
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
            var NumericComponent = self.Unit.GetComponent<NumericComponent>();
            var config = instance.Config;
            var buffType = (BuffType)instance.Config.BuffType;
            if (buffType == BuffType.Numeric)
            {
                for (int i = 0; i < config.Numerics.Length; i++)
                {
                    var numeric = config.Numerics[i];
                    NumericComponent[numeric] -= instance.TotalEffects[i];
                }
            }
        }
    }

}

