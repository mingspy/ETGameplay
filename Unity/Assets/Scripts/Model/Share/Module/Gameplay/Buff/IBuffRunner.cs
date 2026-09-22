using System;

namespace ET
{
    
    /// <summary>
    ///     由工厂构造对应的BuffSystem，
    /// </summary>
    public interface IBuffRunner 
    {
        /// <summary>
        /// 处理的Buff类型
        /// </summary>
        public Type Type { get; }
        
        /// <summary>
        /// 是否可以处理指定buffType
        /// </summary>
        /// <param name="buffType"></param>
        /// <returns></returns>
        public bool CanHandleBuff(BuffType buffType);
        
        /// <summary>
        /// Buff刚被添加时调用。目前的设计一般直接调用<see cref="ApplyBuff"/>
        /// </summary>
        /// <param name="buffComponent"></param>
        /// <param name="buff"></param>
        /// <returns></returns>
        ETTask AddBuff(BuffComponent buffComponent, BuffDataBase buff);
        
        /// <summary>
        /// 应用buff效果，如Instant Buff，Period Buff。
        /// </summary>
        /// <param name="buffComponent"></param>
        /// <param name="buff"></param>
        /// <returns></returns>
        ETTask ApplyBuff(BuffComponent buffComponent, BuffDataBase buff);
        
        /// <summary>
        /// 周期性更新buff
        /// </summary>
        /// <param name="buffComponent"></param>
        /// <param name="buff"></param>
        /// <param name="currentTimeMs"></param>
        /// <returns></returns>
        ETTask TickBuff(BuffComponent buffComponent, BuffDataBase buff, long currentTimeMs);
        
        /// <summary>
        /// 删除buff
        /// </summary>
        /// <param name="buffComponent"></param>
        /// <param name="buff"></param>
        /// <returns></returns>
        ETTask RemoveBuff(BuffComponent buffComponent, BuffDataBase buff);
        
        /// <summary>
        /// Buff过期，一般调用删除
        /// </summary>
        /// <param name="buffComponent"></param>
        /// <param name="buff"></param>
        /// <returns></returns>
        ETTask ExpiredBuff(BuffComponent buffComponent, BuffDataBase buff);


    }

    [EnableClass]
    public abstract class ABuffRunner<A> : IBuffRunner where A : BuffDataBase
    {
        public Type Type => typeof(A);
        protected abstract ETTask OnAddBuff(BuffComponent buffComponent, BuffDataBase buff);
        protected abstract ETTask OnApplyBuffEffect(BuffComponent buffComponent, BuffDataBase buff);
        protected abstract ETTask OnTickBuff(BuffComponent buffComponent, BuffDataBase buff, long currentTimeMs);
        protected abstract ETTask OnRemoveBuff(BuffComponent buffComponent, BuffDataBase buff);
        protected abstract ETTask OnExpiredBuff(BuffComponent buffComponent, BuffDataBase buff);

        public abstract bool CanHandleBuff(BuffType buffType);
        
        
        public async ETTask AddBuff(BuffComponent buffComponent, BuffDataBase buff)
        {
            try
            {
                await this.OnApplyBuffEffect(buffComponent, buff);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public async ETTask ApplyBuff(BuffComponent buffComponent, BuffDataBase buff)
        {
            try
            {
                await this.OnApplyBuffEffect(buffComponent, buff);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public async ETTask TickBuff(BuffComponent buffComponent, BuffDataBase buff, long currentTimeMs)
        {
            try
            {
                await this.OnTickBuff(buffComponent, buff, currentTimeMs);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        // 主动 Remove，直接移除，不会再更新。
        public async ETTask RemoveBuff(BuffComponent buffComponent, BuffDataBase buff)
        {
            try
            {
                await this.OnRemoveBuff(buffComponent, buff);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public async ETTask ExpiredBuff(BuffComponent buffComponent, BuffDataBase buff)
        {
            try
            {
                await this.OnExpiredBuff(buffComponent, buff);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}