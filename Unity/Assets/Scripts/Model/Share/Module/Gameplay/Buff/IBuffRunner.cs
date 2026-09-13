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
        ETTask ApplyBuff(BuffComponent buffComponent, BuffDataBase buff);
        ETTask TickBuff(BuffComponent buffComponent, BuffDataBase buff, long currentTimeMs);
        ETTask RemoveBuff(BuffComponent buffComponent, BuffDataBase buff);
        ETTask ExpiredBuff(BuffComponent buffComponent, BuffDataBase buff);
    }

    [EnableClass]
    public abstract class ABuffRunner<A> : IBuffRunner where A : BuffDataBase
    {
        public Type Type => typeof(A);
        protected abstract ETTask DoApplyBuffEffect(BuffComponent buffComponent, BuffDataBase buff);
        protected abstract ETTask DoTickBuff(BuffComponent buffComponent, BuffDataBase buff, long currentTimeMs);
        protected abstract ETTask DoRemoveBuff(BuffComponent buffComponent, BuffDataBase buff);
        protected abstract ETTask DoExpiredBuff(BuffComponent buffComponent, BuffDataBase buff);


        public async ETTask ApplyBuff(BuffComponent buffComponent, BuffDataBase buff)
        {
            try
            {
                await this.DoApplyBuffEffect(buffComponent, buff);
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
                await this.DoTickBuff(buffComponent, buff, currentTimeMs);
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
                await this.DoRemoveBuff(buffComponent, buff);
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
                await this.DoExpiredBuff(buffComponent, buff);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}