using System;
using System.Collections.Generic;

namespace ET
{
    [EntitySystemOf(typeof(BuffComponent))]
    [FriendOf(typeof(BuffComponent))]
    [FriendOf(typeof(BuffNode))]
    public static partial class BuffComponentSystem
    {
        #region Buff EntitySystem

        [EntitySystem]
        private static void Awake(this BuffComponent self)
        {
            self.BuffsByConfigId.Clear();
            self.BuffsByTag.Clear();
            self.UpdateBuffs.Clear();
            self.EventBuffs.Clear();
        }

        /// <summary>
        ///     每帧更新 - 轮询驱动入口
        ///     只遍历需要Update的Buff（UpdateBuffs列表），不需要遍历所有Buff
        /// </summary>
        [EntitySystem]
        public static void Update(this BuffComponent self)
        {
            long deltaTime = TimeHelper.DeltaTime;
            if (self.UpdateBuffs.Count == 0)
            {
                return;
            }

            // 冷却时间更新 - 遍历所有Buff更新冷却？不，只更新UpdateBuffs里的
            // 实际上冷却也可以用事件驱动，这里简化处理：冷却在Update中更新
            var toRemove = new List<long>();

            foreach (long buffId in self.UpdateBuffs)
            {
                BuffNode buffNode = self.GetChild<BuffNode>(buffId);
                //if (!self.Buffs.TryGetValue(buffId, out BuffNode buffNode))
                if (buffNode == null)
                {
                    toRemove.Add(buffId);
                    continue;
                }

                if (!buffNode.IsActive)
                {
                    continue;
                }

                // 更新已存在时间
                buffNode.ElapsedTime += deltaTime;

                // 更新冷却
                if (buffNode.CooldownRemaining > 0)
                {
                    buffNode.CooldownRemaining = Math.Max(0, buffNode.CooldownRemaining - deltaTime);
                }

                // 检查持续时间是否结束
                if (buffNode.Duration > 0 && buffNode.ElapsedTime >= buffNode.Duration)
                {
                    toRemove.Add(buffId);
                    continue;
                }

                // 间隔触发检查
                if (buffNode.HasType(BuffType.IntervalTick) && buffNode.Interval > 0)
                {
                    buffNode.LastTickTime += deltaTime;
                    if (buffNode.LastTickTime >= buffNode.Interval)
                    {
                        buffNode.LastTickTime = 0;
                        BuffHandlerDispatcher.Instance.DispatchBuffIntervalTick(self, buffNode).Coroutine();
                    }
                }
            }

            // 移除到期的Buff
            foreach (long buffId in toRemove)
            {
                self.RemoveBuff(buffId);
            }
        }

        #endregion

        #region BuffNode Manage 

        /// <summary>
        ///     添加Buff - 核心入口方法
        /// </summary>
        /// <param name="self">BuffComponent</param>
        /// <param name="configId">Buff配置ID</param>
        /// <param name="sourceId">来源实体ID</param>
        /// <param name="duration">持续时间，0表示永久</param>
        /// <param name="sourceType">来源类型</param>
        /// <returns>添加的BuffNode实例</returns>
        public static BuffNode AddBuff(this BuffComponent self, long sourceId, int configId, long duration, BuffSourceType sourceType)
        {
            Unit owner = self.GetParent<Unit>();

            // 创建BuffNode实例
            BuffNode buffNode = self.AddChild<BuffNode>();
            buffNode.Init(configId);
            buffNode.SourceId = sourceId;
            buffNode.TargetId = owner.Id;
            buffNode.Duration = duration;
            buffNode.SourceType = sourceType;

            return self.AddBuff(buffNode);
        }

        /// <summary>
        ///     完全通过Buff ConfigId添加
        /// </summary>
        /// <param name="self"></param>
        /// <param name="sourceId"></param>
        /// <param name="configId"></param>
        /// <returns></returns>
        public static BuffNode AddBuff(this BuffComponent self, long sourceId, int configId)
        {
            Unit owner = self.GetParent<Unit>();

            // 创建BuffNode实例
            BuffNode buffNode = self.AddChild<BuffNode>();
            buffNode.Init(configId);
            buffNode.SourceId = sourceId;
            buffNode.TargetId = owner.Id;

            return self.AddBuff(buffNode);
        }

        /// <summary>
        ///     临时测试用，后续改用前面几个方法。要求BuffType和ConfigId对应，并且在BuffConfig配置中存在。 如BuffId = Type.Stunned,Frozen,Invincible
        /// </summary>
        /// <param name="self"></param>
        /// <param name="sourceId"></param>
        /// <param name="buffType"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public static BuffNode AddBuff(this BuffComponent self, long sourceId, BuffType buffType, long duration)
        {
            Unit owner = self.GetParent<Unit>();

            // 创建BuffNode实例
            BuffNode buffNode = self.AddChild<BuffNode>();
            buffNode.Init((int)buffType);
            buffNode.Type = buffType;
            buffNode.SourceId = sourceId;
            buffNode.TargetId = owner.Id;
            buffNode.Duration = duration;

            return self.AddBuff(buffNode);
        }

        private static BuffNode AddBuff(this BuffComponent self, BuffNode buffNode)
        {
            // 立即buff，直接添加，并返回
            if (buffNode.HasType(BuffType.Instant) && buffNode.Duration <= 0)
            {
                self.ApplyBuff(buffNode);
                buffNode.Dispose();
                return buffNode;
            }

            // 加入字典 暂时不需要，直接从children中获取
            //self.Buffs.Add(buffNode.Id, buffNode);
            int configId = buffNode.HandlerId;

            // 按ConfigId索引
            if (!self.BuffsByConfigId.ContainsKey(configId))
            {
                self.BuffsByConfigId[configId] = new List<long>();
            }

            self.BuffsByConfigId[configId].Add(buffNode.Id);

            /*
            if (self.BuffsByConfigId[configId].Count < buffNode.MaxStack )
            {
                self.BuffsByConfigId[configId].Add(buffNode.Id);
            }
            else
            {
                // 更新buff的时间
                foreach (long bid in self.BuffsByConfigId[configId])
                {
                    BuffNode added = self.GetChild<BuffNode>(bid);
                    if (added == null)
                    {
                        continue;
                    }

                    if (buffNode.Duration > added.Duration - added.ElapsedTime)
                    {
                        added.Duration = buffNode.Duration;
                        added.ElapsedTime = 0;
                        added.CooldownRemaining = 0;
                    }

                    buffNode.Dispose();
                    return null;
                }
            }
            */

            // 按标签索引
            if (buffNode.Tags != null)
            {
                foreach (string tag in buffNode.Tags)
                {
                    if (!self.BuffsByTag.ContainsKey(tag))
                    {
                        self.BuffsByTag[tag] = new List<long>();
                    }

                    self.BuffsByTag[tag].Add(buffNode.Id);
                }
            }

            // 判断是否需要加入Update轮询列表
            if (buffNode.HasType(BuffType.Duration) || buffNode.Duration >= 0)
            {
                self.UpdateBuffs.Add(buffNode.Id);
            }

            // 注册事件监听
            if (buffNode.ListenEvents != null)
            {
                foreach (DamageStage triggerEvent in buffNode.ListenEvents)
                {
                    if (!self.EventBuffs.ContainsKey(triggerEvent))
                    {
                        self.EventBuffs[triggerEvent] = new List<long>();
                    }

                    self.EventBuffs[triggerEvent].Add(buffNode.Id);
                }
            }

            self.ApplyBuff(buffNode);

            return buffNode;
        }

        /// <summary>
        ///     应用buff效果
        /// </summary>
        /// <param name="self"></param>
        /// <param name="buffNode"></param>
        public static void ApplyBuff(this BuffComponent self, BuffNode buffNode)
        {
            // 如果是属性修改型Buff，添加时立即重算属性
            //if (buffNode.HasType(Type.AttributeModifer)  || buffNode.NumericModifiers != null)
            if (buffNode.AttributeModifiers != null)
            {
                self.ApplyNumericModifiers(buffNode);
            }

            // 触发OnAdd事件 - Buff添加时的立即效果
            BuffHandlerDispatcher.Instance.DispatchBuffAdd(self, buffNode).Coroutine();

            Log.Info($"[BuffSystem] 添加Buff: {buffNode.Name}, 来源: {buffNode.SourceId}, 目标: {buffNode.TargetId}");
        }

        /// <summary>
        ///     移除Buff
        /// </summary>
        public static void RemoveBuff(this BuffComponent self, long buffInstanceId)
        {
            BuffNode buffNode = self.GetChild<BuffNode>(buffInstanceId);
            if (buffNode == null)
            {
                return;
            }

            // 触发OnRemove回调
            BuffHandlerDispatcher.Instance.DispatchBuffRemove(self, buffNode).Coroutine();

            // 如果是属性修改型，移除时回滚属性并重算
            if (buffNode.AttributeModifiers != null)
            {
                self.RemoveNumericModifiers(buffNode);
            }

            // 从各索引中移除
            if (self.BuffsByConfigId.TryGetValue(buffNode.HandlerId, out var configList))
            {
                configList.Remove(buffInstanceId);
                if (configList.Count == 0)
                {
                    self.BuffsByConfigId.Remove(buffNode.HandlerId);
                }
            }

            if (buffNode.Tags != null)
            {
                foreach (string tag in buffNode.Tags)
                {
                    if (!self.BuffsByTag.TryGetValue(tag, out var tagList))
                    {
                        continue;
                    }

                    tagList.Remove(buffInstanceId);
                    if (tagList.Count == 0)
                    {
                        self.BuffsByTag.Remove(tag);
                    }
                }
            }

            self.UpdateBuffs.Remove(buffInstanceId);

            if (buffNode.ListenEvents != null)
            {
                foreach (DamageStage triggerEvent in buffNode.ListenEvents)
                {
                    if (!self.EventBuffs.TryGetValue(triggerEvent, out var eventList))
                    {
                        continue;
                    }

                    eventList.Remove(buffInstanceId);
                    if (eventList.Count == 0)
                    {
                        self.EventBuffs.Remove(triggerEvent);
                    }
                }
            }

            // 从主字典移除并销毁
            //self.Buffs.Remove(buffInstanceId);
            self.RemoveChild(buffInstanceId);
            //buffNode.Dispose();

            Log.Info($"[BuffSystem] 移除Buff, BuffId: {buffInstanceId}");
        }

        /// <summary>
        ///     移除指定ConfigId的所有Buff
        /// </summary>
        public static void RemoveBuffByConfigId(this BuffComponent self, int configId)
        {
            if (!self.BuffsByConfigId.TryGetValue(configId, out var buffIds))
            {
                return;
            }

            // 复制一份列表，因为RemoveBuff会修改原列表
            var toRemove = new List<long>(buffIds);
            foreach (long buffId in toRemove)
            {
                self.RemoveBuff(buffId);
            }
        }

        /// <summary>
        ///     驱散指定标签的Buff
        /// </summary>
        public static int DispelByTag(this BuffComponent self, string tag, int count = -1)
        {
            if (!self.BuffsByTag.TryGetValue(tag, out var buffIds))
            {
                return 0;
            }

            var toRemove = new List<long>(buffIds);
            int removed = 0;
            foreach (long buffId in toRemove)
            {
                if (count >= 0 && removed >= count)
                {
                    break;
                }

                BuffNode buff = self.GetChild<BuffNode>(buffId);

                if (buff != null && buff.IsDispellable)
                {
                    self.RemoveBuff(buffId);
                    removed++;
                }
            }

            return removed;
        }

        public static bool HasTag(this BuffComponent self, string tag)
        {
            if (!self.BuffsByTag.TryGetValue(tag, out var buffIds))
            {
                return false;
            }

            return buffIds.Count > 0;
        }

        public static bool HasAny(this BuffComponent self, IEnumerable<string> tags)
        {
            foreach (string tag in tags)
            {
                if (self.HasTag(tag))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool HasAll(this BuffComponent self, IEnumerable<string> tags)
        {
            foreach (string tag in tags)
            {
                if (!self.HasTag(tag))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     发布战斗事件 - 事件驱动入口
        ///     当战斗事件发生时调用此方法，立即触发所有监听该事件的Buff
        /// </summary>
        public static void PublishEvent<T>(this BuffComponent self, T eventData) where T : IDamageEvent
        {
            if (!self.EventBuffs.TryGetValue(eventData.Stage, out var buffIds))
            {
                return;
            }

            // 遍历监听该事件的Buff，执行触发逻辑
            // 注意：这里遍历副本，防止触发过程中Buff列表变化
            var buffList = new List<long>(buffIds);
            foreach (long buffId in buffList)
            {
                //if (!self.Buffs.TryGetValue(buffId, out BuffNode buffNode))
                BuffNode buffNode = self.GetChild<BuffNode>(buffId);
                if (buffNode != null)
                {
                    continue;
                }

                if (!buffNode.IsActive)
                {
                    continue;
                }

                // 检查冷却
                if (buffNode.CooldownRemaining > 0)
                {
                    continue;
                }

                buffNode.CooldownRemaining = buffNode.Cooldown;

                BuffHandlerDispatcher.Instance.DispatchBuffEvent(self, buffNode, eventData).Coroutine();
            }
        }

        /// <summary>
        ///     叠加Buff层数
        /// </summary>
        public static int AddStack(this BuffComponent self, long buffInstanceId, int addStack = 1)
        {
            //if (!self.Buffs.TryGetValue(buffInstanceId, out BuffNode buffNode))
            BuffNode buffNode = self.GetChild<BuffNode>(buffInstanceId);
            if (buffNode == null)
            {
                return 0;
            }

            int oldStack = buffNode.CurrentStack;
            int newStack = Math.Min(buffNode.MaxStack, buffNode.CurrentStack + addStack);

            if (buffNode.CurrentStack != newStack)
            {
                // 层数变化，重算属性
                if (buffNode.AttributeModifiers != null)
                {
                    self.RemoveNumericModifiers(buffNode);
                    buffNode.CurrentStack = newStack;
                    self.ApplyNumericModifiers(buffNode);
                }

                buffNode.CurrentStack = newStack;

                // 刷新持续时间（王者荣耀大部分Buff叠层会刷新时间）
                buffNode.ElapsedTime = 0;

                Log.Info($"[BuffSystem] Buff层数变化: {buffNode.Name}, {oldStack} -> {buffNode.CurrentStack}");
            }

            return buffNode.CurrentStack;
        }

        /// <summary>
        ///     应用属性修改器到数值组件
        /// </summary>
        private static void ApplyNumericModifiers(this BuffComponent self, BuffNode buffNode)
        {
            Unit owner = self.GetParent<Unit>();
            NumericComponent numeric = owner.GetComponent<NumericComponent>();
            if (numeric == null || buffNode.AttributeModifiers == null)
            {
                return;
            }

            int stack = buffNode.CurrentStack;
            foreach (var kv in buffNode.AttributeModifiers)
            {
                long value = kv.Value * stack;
                numeric[kv.Key] += value;
            }
        }

        /// <summary>
        ///     移除属性修改器
        /// </summary>
        private static void RemoveNumericModifiers(this BuffComponent self, BuffNode buffNode)
        {
            Unit owner = self.GetParent<Unit>();
            NumericComponent numeric = owner.GetComponent<NumericComponent>();
            if (numeric == null || buffNode.AttributeModifiers == null)
            {
                return;
            }

            int stack = buffNode.CurrentStack;
            foreach (var kv in buffNode.AttributeModifiers)
            {
                long value = kv.Value * stack;
                numeric[kv.Key] -= value;
            }
        }

        #endregion
    }
}