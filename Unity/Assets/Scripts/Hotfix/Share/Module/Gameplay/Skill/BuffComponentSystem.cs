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
            //Log.Info($"BuffComponentSystem  Update, BuffCount: {self.Buffs.Count} IScene {self.Scene()} {self.Fiber()} AppType {Options.Instance.AppType}");
            var now = TimeInfo.Instance.ServerFrameTime();
            var expiredKeys = new List<int>();
            foreach (var kvp in self.Buffs)
            {
                if (now >= kvp.Value.EndTime)
                {
                    expiredKeys.Add(kvp.Key);
                }
            }
            foreach (var key in expiredKeys)
            {
                self.RemoveBuff(key);
            }
        }
        
        public static void AddBuff(this BuffComponent self, int casterId, int buffId, int durationMs, int stack = 1)
        {
            var now = TimeInfo.Instance.ServerFrameTime();
            if (self.Buffs.TryGetValue(buffId, out var info))
            {
                info.Stacks += stack;
                info.EndTime = now + durationMs;
            }
            else
            {
                var config = BuffConfigCategory.Instance.Get(buffId);
                if (config == null) return;
                self.Buffs[buffId] = new BuffInstance()
                {
                    BuffId = buffId,
                    OwnerId = self.Unit.Id,
                    CasterId = casterId,
                    StartTime = now,
                    EndTime = now + durationMs,
                    Duration = config.Duration,
                };
            }
            // 触发Buff添加事件，可用于更新UI或行为树条件
            EventSystem.Instance.PublishAsync(self.Scene(), new OnBuffAddedEvent(){Unit = self.Unit, BuffConfig = self.Buffs[buffId]}).Coroutine();
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

        public static void RemoveBuff(this BuffComponent self,int buffId)
        {
            if (self.Buffs.ContainsKey(buffId))
            {
                var  info = self.Buffs[buffId];
                self.Buffs.Remove(buffId);
                EventSystem.Instance.PublishAsync(self.Scene(), new OnBuffRemovedEvent(){Unit = self.Unit, BuffConfig = self.Buffs[buffId]}).Coroutine();
            }
        }
    }

}

