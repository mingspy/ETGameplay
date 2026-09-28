using System;
using System.Collections.Generic;

namespace ET
{
    [Code]
    [FriendOf(typeof(BuffNode))]
    public class BuffHandlerDispatcher : Singleton<BuffHandlerDispatcher>, ISingletonAwake
    {
        public void Awake()
        {
            var types = CodeTypes.Instance.GetTypes(typeof(BuffHandlerAttribute));
            foreach (Type type in types)
            {
                object[] attrs = type.GetCustomAttributes(typeof(BuffHandlerAttribute), false);
                foreach (object attr in attrs)
                {
                    BuffHandlerAttribute numericWatcherAttribute = (BuffHandlerAttribute)attr;
                    IBuffHandler handler = (IBuffHandler)Activator.CreateInstance(type);
                    this.BuffHandlersById.TryAdd(handler.HandlerId, handler);
                    if (!this.BuffHandlersByType.TryGetValue(handler.BuffClassType, out var handlers))
                    {
                        handlers = ObjectPool.Instance.Fetch<List<IBuffHandler>>();
                        this.BuffHandlersByType[handler.BuffClassType] = handlers;
                    }

                    if (!handlers.Exists(a => a.HandlerId == handler.HandlerId))
                    {
                        handlers.Add(handler);
                        handlers.Sort((a, b) => b.Priority.CompareTo(a.Priority));
                    }
                }
            }
        }

        protected override void Destroy()
        {
            if (this.buffHandlersById != null)
            {
                this.buffHandlersById.Clear();
                ObjectPool.Instance.Recycle(this.buffHandlersById);
                this.buffHandlersById = null;
            }

            if (this.buffHandlersByType != null)
            {
                foreach (var kvp in this.buffHandlersByType)
                {
                    ObjectPool.Instance.Recycle(kvp.Value);
                }

                this.buffHandlersByType.Clear();
                ObjectPool.Instance.Recycle(this.buffHandlersByType);
                this.buffHandlersByType = null;
            }
        }

        #region BuffHandler 管理

        /// <summary>
        ///     按handlerId索引的BuffHandler，唯一索引，优先获得执行机会。
        /// </summary>
        private SortedDictionary<int, IBuffHandler> buffHandlersById;

        public SortedDictionary<int, IBuffHandler> BuffHandlersById
        {
            get
            {
                return this.buffHandlersById ??= ObjectPool.Instance.Fetch<SortedDictionary<int, IBuffHandler>>();
            }
        }

        /// <summary>
        ///     按类型索引的BuffHandler，按照Priority顺序执行。
        /// </summary>
        private SortedDictionary<Type, List<IBuffHandler>> buffHandlersByType;

        public SortedDictionary<Type, List<IBuffHandler>> BuffHandlersByType
        {
            get
            {
                return this.buffHandlersByType ??= ObjectPool.Instance.Fetch<SortedDictionary<Type, List<IBuffHandler>>>();
            }
        }

        #endregion

        #region BuffHandler Methods

        public async ETTask DispatchBuffAdd(BuffComponent buffComponent, BuffNode buffNode)
        {
            if (this.BuffHandlersById.TryGetValue(buffNode.HandlerId, out IBuffHandler aHandler))
            {
                await aHandler.OnAdd(buffComponent, buffNode);
                return;
            }

            if (!this.BuffHandlersByType.TryGetValue(buffNode.GetType(), out var handlers))
            {
                return;
            }

            foreach (IBuffHandler handler in handlers)
            {
                try
                {
                    await handler.OnAdd(buffComponent, buffNode);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public async ETTask DispatchBuffRemove(BuffComponent buffComponent, BuffNode buffNode)
        {
            if (this.BuffHandlersById.TryGetValue(buffNode.HandlerId, out IBuffHandler aHandler))
            {
                await aHandler.OnRemove(buffComponent, buffNode);
                return;
            }

            if (!this.BuffHandlersByType.TryGetValue(buffNode.GetType(), out var handlers))
            {
                return;
            }

            foreach (IBuffHandler handler in handlers)
            {
                try
                {
                    await handler.OnRemove(buffComponent, buffNode);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public async ETTask DispatchBuffIntervalTick(BuffComponent buffComponent, BuffNode buffNode)
        {
            if (this.BuffHandlersById.TryGetValue(buffNode.HandlerId, out IBuffHandler aHandler))
            {
                await aHandler.OnIntervalTick(buffComponent, buffNode);
                return;
            }

            if (!this.BuffHandlersByType.TryGetValue(buffNode.GetType(), out var handlers))
            {
                return;
            }

            foreach (IBuffHandler handler in handlers)
            {
                try
                {
                    await handler.OnIntervalTick(buffComponent, buffNode);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public async ETTask DispatchBuffEvent<T>(BuffComponent buffComponent, BuffNode buffNode, T eventData) where T : IDamageEvent
        {
            if (this.BuffHandlersById.TryGetValue(buffNode.HandlerId, out IBuffHandler aHandler))
            {
                await aHandler.OnEvent(buffComponent, buffNode, eventData);
                eventData.Context.IsHandled = eventData.IsHandled;
                return;
            }

            if (!this.BuffHandlersByType.TryGetValue(buffNode.GetType(), out var handlers))
            {
                return;
            }

            foreach (IBuffHandler handler in handlers)
            {
                try
                {
                    await handler.OnEvent(buffComponent, buffNode, eventData);
                    if (eventData.IsHandled)
                    {
                        eventData.Context.IsHandled = true;
                        return;
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        #endregion
    }
}