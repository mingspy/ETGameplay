using System;
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
            int cnt = BuffConfigCategory.Instance.GetAll().Count;
            Log.Info($"BuffComponentSystem  Awake, total BuffCount: {cnt} IScene {self.Scene()} {self.Fiber()} AppType {Options.Instance.AppType}");
        }

        [EntitySystem]
        private static void Update(this BuffComponent self)
        {
            if (self.Buffs.Count == 0)
            {
                return;
            }

            // 每帧检查过期Buff，TODO: 后续改成用 TimerComponent 定时器完成。
            long now = TimeHelper.Now();
            var expiredKeys = new List<int>();
            foreach (var kvp in self.Buffs)
            {
                BuffDataBase buff = kvp.Value;
                IBuffRunner runner = BuffFactory.Instance.GetRunner(buff.GetType());
                runner.TickBuff(self, buff, now).Coroutine();
                // 检查周期性
                if (buff.Period > 0 && now >= buff.PeriodEndTime)
                {
                    runner.ApplyBuff(self, buff).Coroutine();
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

        /// <summary>
        ///     从配置文件添加buff
        /// </summary>
        /// <param name="self"></param>
        /// <param name="casterId"></param>
        /// <param name="buffId"></param>
        /// <returns></returns>
        public static BuffDataBase AddBuff(this BuffComponent self, long casterId, int buffId)
        {
            BuffConfig config = BuffConfigCategory.Instance.Get(buffId);

            BuffDataBase buff = BuffFactory.Instance.CreateBuff((BuffType)config.BuffType);
            buff.CasterId = casterId;
            buff.BuffConfig = config;
            return self.AddBuff(buff);
        }

        /// <summary>
        ///     通过BuffType添加buff，如Control buff
        /// </summary>
        /// <param name="self"></param>
        /// <param name="casterId"></param>
        /// <param name="buffType"></param>
        /// <param name="durationMs"></param>
        /// <returns></returns>
        public static BuffDataBase AddBuff(this BuffComponent self, long casterId, BuffType buffType, long durationMs)
        {
            BuffDataBase buff = BuffFactory.Instance.CreateBuff(buffType);
            buff.Duration = TimeHelper.ToMS(durationMs);
            buff.CasterId = casterId;
            return self.AddBuff(buff);
        }

        /// <summary>
        ///     添加自定义buff
        /// </summary>
        /// <param name="self"></param>
        /// <param name="buff"></param>
        /// <returns></returns>
        public static BuffDataBase AddBuff(this BuffComponent self, BuffDataBase buff)
        {
            if (buff == null)
            {
                Log.Error($"input buff is null");
                return null;
            }

            IBuffRunner runner = BuffFactory.Instance.GetRunner(buff.GetType());
            if (runner == null)
            {
                Log.Error($"{buff.GetType()} has not implement IBuffRunner");
                return null;
            }

            // 立即性buff，直接应用buff效果。
            if (buff.DurationType == BuffDurationType.Instant)
            {
                runner.ApplyBuff(self, buff).Coroutine();
            }
            else
            {
                // 叠加buff
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
                else // 新增buff
                {
                    if (buff.BuffId <= 0)
                    {
                        buff.BuffId = self.GenId();
                    }

                    buff.Init(TimeHelper.Now());
                    self.Buffs[buff.BuffId] = buff;
                    runner.AddBuff(self, buff).Coroutine();
                }
            }

            // 触发Buff添加事件，可用于更新UI或行为树条件
            EventSystem.Instance.PublishAsync(self.Scene(), new OnBuffAddedEvent() { Unit = self.GetParent<Unit>(), Buff = buff }).Coroutine();
            return buff;
        }

        public static void RemoveBuff(this BuffComponent self, int buffId)
        {
            if (!self.Buffs.Remove(buffId, out BuffDataBase buff))
            {
                return;
            }

            BuffFactory.Instance.GetRunner(buff.GetType()).RemoveBuff(self, buff).Coroutine();

            EventSystem.Instance.PublishAsync(self.Scene(), new OnBuffRemovedEvent() { Unit = self.GetParent<Unit>(), Buff = buff }).Coroutine();
        }

        private static void ExpireBuff(this BuffComponent self, int buffId)
        {
            if (!self.Buffs.Remove(buffId, out BuffDataBase buff))
            {
                return;
            }

            BuffFactory.Instance.GetRunner(buff.GetType()).ExpiredBuff(self, buff).Coroutine();

            EventSystem.Instance.PublishAsync(self.Scene(), new OnBuffExpiredEvent() { Unit = self.GetParent<Unit>(), Buff = buff }).Coroutine();
        }

        /// <summary>
        ///     删除指定类型的所有buff
        /// </summary>
        /// <param name="self"></param>
        /// <param name="buffType"></param>
        public static void RemoveBuffs(this BuffComponent self, BuffType buffType)
        {
            var expiredKeys = new List<int>();
            foreach (BuffDataBase buff in self.Buffs.Values)
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