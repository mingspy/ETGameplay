using System.Collections.Generic;

namespace ET.Passive
{
    [EntitySystemOf(typeof(PassiveComponent))]
    [FriendOf(typeof(PassiveComponent))]
    public static partial class PassiveComponentSystem
    {
        [EntitySystem]
        private static void Awake(this PassiveComponent self)
        {

        }
        [EntitySystem]
        private static void Destroy(this PassiveComponent self)
        {

        }
        
        // 添加被动
        public static PassiveDataBase AddPassive(this PassiveComponent self, PassiveDataBase passive)
        {
            // TODO: 支持从配置表读取被动配置
            // var passiveConfig = PassiveConfigCategory.Instance.Get(passiveId);
            if (!self.AllPassives.TryAdd(passive.PassiveId, passive))
            {
                return null;
            }

            // 按触发类型分组存入字典
            if (!self.PassiveMap.ContainsKey(passive.TriggerType))
            {
                self.PassiveMap.Add(passive.TriggerType, new List<PassiveDataBase>());
            }
            self.PassiveMap[passive.TriggerType].Add(passive);
            // 按优先级排序，高优先级先执行
            self.PassiveMap[passive.TriggerType].Sort((a,b) => b.Priority.CompareTo(a.Priority));
            return passive;
        }
    
        // 移除被动
        public static bool RemovePassive(this PassiveComponent self, int passiveId)
        {
            if (!self.AllPassives.TryGetValue(passiveId, out PassiveDataBase passive))
            {
                return false;
            }

            self.PassiveMap[passive.TriggerType].Remove(passive);
            self.AllPassives.Remove(passiveId);
            return true;
        }
    
        // 触发指定类型的所有被动
        public static async ETTask TriggerPassives<T>(this PassiveComponent self,  T eventData) where T : IPassiveEvent
        {
            if (!self.PassiveMap.TryGetValue(eventData.TriggerType, out var passives)) return;
            // 倒序遍历避免移除元素时的索引问题
            for (int i = passives.Count - 1; i >= 0; i--)
            {
                var passive = passives[i];
                if (!passive.IsEnabled) continue;
                // 冷却判断
                if (TimeHelper.Now() - passive.LastTriggerTime < passive.Cooldown * 1000) continue;
                // 执行被动效果
                bool isIntercept = await PassiveDispatcher.Instance.Execute(self.GetParent<Unit>(), passive, eventData);
                
                passive.LastTriggerTime = TimeHelper.ServerNow();
                // 如果被动返回拦截，终止后续被动执行
                if (isIntercept) break;
            }
        }
    }
}