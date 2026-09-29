using System;

namespace ET
{
    /// <summary>
    ///     Buff处理器接口
    /// </summary>
    public interface IBuffHandler
    {
        /// <summary>
        ///     如果大于0，那么只处理该指定的ConfigId(buffId);如果为0，处理一类BuffNode，由下面的BuffType指定。
        /// </summary>
        public int HandlerId { get; }

        /// <summary>
        ///     处理的buff class 类型。TODO: 需要支持反射,从BuffConfig中反射出实际的buff类型，同时还要改BuffNode不能继承自Entity，才能绕过ET的限制。
        /// </summary>
        public Type BuffClassType { get; }

        //public bool CanHandle(BuffNode buffNode);
        /// <summary>
        ///     优先级，值越大越先执行
        /// </summary>
        public int Priority { get; }

        ETTask OnAdd(BuffComponent self, BuffNode buffNode);
        ETTask OnRemove(BuffComponent self, BuffNode buffNode);
        ETTask OnIntervalTick(BuffComponent self, BuffNode buffNode);
        ETTask OnEvent<T>(BuffComponent self, BuffNode buffNode,  T eventData) where T : IDamageEvent;
    }

    /// <summary>
    ///     Buff处理器基类，提供默认空实现
    /// </summary>
    [EnableClass]
    public abstract class ABuffHandler<A> : IBuffHandler
    {
        protected ABuffHandler(int handlerId, int priority)
        {
            this.HandlerId = handlerId;
            this.Priority = priority;
        }

        public int HandlerId { get; }
        public Type BuffClassType => typeof(A);

        public int Priority { get; }

        //public virtual bool CanHandle(BuffNode buffNode) => this.Type == buffNode.GetType();

        public abstract ETTask OnAdd(BuffComponent self, BuffNode buffNode);
        public abstract ETTask OnRemove(BuffComponent self, BuffNode buffNode);
        public abstract ETTask OnIntervalTick(BuffComponent self, BuffNode buffNode);
        public abstract ETTask OnEvent<T>(BuffComponent self, BuffNode buffNode, T eventData) where T : IDamageEvent;
    }

    /// <summary>
    ///     BuffHandler注解，用于自动注册
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class BuffHandlerAttribute : BaseAttribute
    {
    }
}