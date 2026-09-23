using System;

namespace ET
{
    /// <summary>
    /// Buff处理器接口
    /// </summary>
    public interface IBuffHandler
    {
        /// <summary>
        /// 处理的Buff ConfigId, 如果大于0，那么处理指定的配置文件中的buff。
        /// </summary>
        public int ConfigId { get; }
        
        /// <summary>
        /// 处理的buff class 类型
        /// </summary>
        public Type BuffType { get; }
        //public bool CanHandle(BuffNode buffNode);
        /// <summary>
        /// 优先级，值越大越先执行
        /// </summary>
        public int Priority { get; set; }
        ETTask OnAdd(BuffComponent self, BuffNode buffNode);
        ETTask OnRemove(BuffComponent self, BuffNode buffNode);
        ETTask OnIntervalTick(BuffComponent self, BuffNode buffNode);
        ETTask OnEvent<T>(BuffComponent self, BuffNode buffNode,  T eventData) where T : IBuffEvent;
    }
    
    /// <summary>
    /// Buff处理器基类，提供默认空实现
    /// </summary>
    [EnableClass]
    public abstract class ABuffHandler<A> : IBuffHandler
    {
        public int ConfigId { get; }
        public Type BuffType => typeof(A);
        
        public int Priority { get; set; }

        //public virtual bool CanHandle(BuffNode buffNode) => this.BuffType == buffNode.GetType();
        
        public abstract ETTask OnAdd(BuffComponent self, BuffNode buffNode);
        public abstract ETTask OnRemove(BuffComponent self, BuffNode buffNode);
        public abstract ETTask OnIntervalTick(BuffComponent self, BuffNode buffNode);
        public abstract ETTask OnEvent<T>(BuffComponent self, BuffNode buffNode, T eventData) where T : IBuffEvent;
    }
    
}