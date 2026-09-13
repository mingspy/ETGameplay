using System;
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
            int cnt = BuffConfigCategory.Instance.GetAll().Count;
            Log.Info($"BuffComponentSystem  Awake, total BuffCount: {cnt} IScene {self.Scene()} {self.Fiber()} AppType {Options.Instance.AppType}");
        }

        [EntitySystem]
        private static void Update(this BuffComponent self)
        {
            if (self.Buffs.Count == 0) return;
            
            // 每帧检查过期Buff，TODO: 后续改成用 TimerComponent 定时器完成。
            long now = TimeHelper.Now();
            var expiredKeys = new List<int>();
            foreach (var kvp in self.Buffs)
            {
                BuffDataBase buff = kvp.Value;
                IBuffRunner runner = BuffRunnerFactory.Instance.Get(buff);
                runner.TickBuff(self, buff, now).Coroutine();
                // 检查周期性
                if (buff.Period > 0 && now >= buff.PeriodEndTime)
                {
                    BuffRunnerFactory.Instance.Get(buff).ApplyBuff(self, buff).Coroutine();
                    buff.PeriodEndTime = now + buff.Period;
                }

                // 检查过期
                if (buff.DurationType == BuffDurationType.HasDuration && now >= buff.EndTime)
                {
                    expiredKeys.Add(kvp.Key);
                }
            }

            foreach (int key in expiredKeys)
            {
                self.ExpireBuff(key);
            }
        }

        public static bool HasBuff(this BuffComponent self, int buffId)
        {
            if (self.Buffs.TryGetValue(buffId, out BuffDataBase info))
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
        

        public static int AddBuff(this BuffComponent self, BuffDataBase buff)
        {
            IBuffRunner runner = BuffRunnerFactory.Instance.Get(buff);
            if (runner == null)
            {
                Log.Error($"{buff.GetType()} has not implement IBuffRunner");
                return -1;
            }
            
            long now = TimeHelper.Now();
            
            if (buff.DurationType == BuffDurationType.Instant)
            {
                runner.ApplyBuff(self, buff).Coroutine();
            }
            else
            {
                buff.StartTime = now;
                buff.EndTime = now + buff.Duration;
                if (buff.BuffId > 0 && self.Buffs.TryGetValue(buff.BuffId, out BuffDataBase added))
                {
                    // 同类型可叠加则叠加层数，不可叠加则刷新时间
                    if (added.Stacks < added.MaxStacks)
                    {
                        added.Stacks += 1;
                        runner.ApplyBuff(self, buff).Coroutine();
                    }
                    else
                    {
                        added.EndTime = Math.Max(added.EndTime, buff.EndTime);
                    }
                }
                else
                {
                    if (buff.BuffId <= 0)
                    {
                        buff.BuffId = self.GenId();
                    }
                    self.Buffs[buff.BuffId] = buff;
                    runner.ApplyBuff(self, buff).Coroutine();
                }
            }
            
            // 触发Buff添加事件，可用于更新UI或行为树条件
            EventSystem.Instance.PublishAsync(self.Scene(), new OnBuffAddedEvent() { Unit = self.GetParent<Unit>(), Buff = buff }).Coroutine();
            return buff.BuffId;
        }

        public static void RemoveBuff(this BuffComponent self, int buffId)
        {
            if (!self.Buffs.Remove(buffId, out BuffDataBase buff))
            {
                return;
            }

            BuffRunnerFactory.Instance.Get(buff).RemoveBuff(self, buff).Coroutine();
 
            EventSystem.Instance.PublishAsync(self.Scene(), new OnBuffRemovedEvent() { Unit = self.GetParent<Unit>(), Buff = buff }).Coroutine();
        }
        
        private static void ExpireBuff(this BuffComponent self, int buffId)
        {
            if (!self.Buffs.Remove(buffId, out BuffDataBase buff))
            {
                return;
            }

            BuffRunnerFactory.Instance.Get(buff).ExpiredBuff(self, buff).Coroutine();
 
            EventSystem.Instance.PublishAsync(self.Scene(), new OnBuffExpiredEvent() { Unit = self.GetParent<Unit>(), Buff = buff }).Coroutine();
        }

        private static void ApplyBuffEffect(this BuffComponent self, BuffConfig config, int stack, BuffInstance instance)
        {
            NumericComponent NumericComponent = self.GetParent<Unit>().GetComponent<NumericComponent>();
            // TODO : 根据Buff类型设置状态
            if (config.BuffType == (uint)BuffType.Numeric)
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
            NumericComponent NumericComponent = self.GetParent<Unit>().GetComponent<NumericComponent>();
            BuffConfig config = instance.Config;
            if (config.BuffType == (uint)BuffType.Numeric)
            {
                for (int i = 0; i < config.Numerics.Length; i++)
                {
                    int numeric = config.Numerics[i];
                    NumericComponent[numeric] -= instance.TotalEffects[i];
                }
            }
        }

        public static void RemoveBuffs(this BuffComponent self, BuffType buffType)
        {
            var expiredKeys = new List<int>();
            foreach (var buff in self.Buffs.Values)
            {
                if (buff.Type == buffType)
                {
                    expiredKeys.Add(buff.BuffId);
                }
            }

            foreach (int key in expiredKeys)
            {
                self.RemoveBuff(key);
            }
        }
    }
}