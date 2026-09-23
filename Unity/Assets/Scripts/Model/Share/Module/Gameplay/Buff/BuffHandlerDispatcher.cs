using System;
using System.Collections.Generic;

namespace ET
{
    [Code]
    [FriendOf(typeof(BuffNode))]
    public class BuffHandlerDispatcher : Singleton<BuffHandlerDispatcher>, ISingletonAwake
    {
        /// <summary>
        /// 按ConfigId索引的BuffHandler
        /// </summary>
        private readonly Dictionary<int, List<IBuffHandler>> handlers = new();
        
        /// <summary>
        /// 按类型索引的BuffHandler
        /// </summary>
        private readonly Dictionary<Type, List<IBuffHandler>> typeHandlers = new();
        
        public void Awake()
        {
            HashSet<Type> types = CodeTypes.Instance.GetTypes(typeof(BuffHandlerAttribute));
            foreach (Type type in types)
            {
                object[] attrs = type.GetCustomAttributes(typeof(BuffHandlerAttribute), false);
                foreach (object attr in attrs)
                {
                    IBuffHandler obj = (IBuffHandler)Activator.CreateInstance(type);
                    if (obj.ConfigId > 0)
                    {
                        if (!this.handlers.ContainsKey(obj.ConfigId))
                        {
                            this.handlers.Add(obj.ConfigId, new List<IBuffHandler>());
                        }
                        this.handlers[obj.ConfigId].Add(obj);
                        this.handlers[obj.ConfigId].Sort((a,b) => b.Priority.CompareTo(a.Priority));
                    }
                    
                    if (!this.typeHandlers.ContainsKey(obj.BuffType))
                    {
                        this.typeHandlers.Add(obj.BuffType, new List<IBuffHandler>());
                    }
                    
                    this.typeHandlers[obj.BuffType].Add(obj);
                    this.typeHandlers[obj.BuffType].Sort((a,b) => b.Priority.CompareTo(a.Priority));
                }
            }
        }
        
        public List<IBuffHandler> GetHandlers(BuffNode buffNode)
        {
            
            if (this.handlers.TryGetValue(buffNode.ConfigId, out var idHandlers))
            {
                return idHandlers ;
            }
            
            if (this.typeHandlers.TryGetValue(buffNode.GetType(), out var typesHandlers))
            {
                return typesHandlers ;
            }
            
            return null;
        }
        
        public async ETTask DispatchOnAdd(BuffComponent self, BuffNode buffNode)
        {
            var listHandlers = this.GetHandlers(buffNode);
            if (listHandlers == null) return;
            foreach (IBuffHandler handler in listHandlers)
            {
                try
                {
                    await handler.OnAdd(self, buffNode);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
            
        }

        public async ETTask DispatchOnRemove(BuffComponent self, BuffNode buffNode)
        {
            var listHandlers = this.GetHandlers(buffNode);
            if (listHandlers == null) return;
            foreach (IBuffHandler handler in listHandlers)
            {
                try
                {
                    await handler.OnRemove(self, buffNode);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public async ETTask DispatchOnIntervalTick(BuffComponent self, BuffNode buffNode)
        {
            
            var listHandlers = this.GetHandlers(buffNode);
            if (listHandlers == null) return;
            foreach (IBuffHandler handler in listHandlers)
            {
                try
                {
                    await handler.OnIntervalTick(self, buffNode);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public  async ETTask DispatchOnEvent<T>(BuffComponent self, BuffNode buffNode,  T eventData) where T : IBuffEvent
        {
            var listHandlers = this.GetHandlers(buffNode);
            if (listHandlers == null) return;
            foreach (IBuffHandler handler in listHandlers)
            {
                try
                {
                    await handler.OnEvent(self, buffNode, eventData);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}