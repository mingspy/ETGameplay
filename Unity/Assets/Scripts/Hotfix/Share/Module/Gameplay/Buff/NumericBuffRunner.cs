namespace ET
{
    [BuffRunner]
    public class NumericBuffRunner : ABuffRunner<NumericBuff>
    {
        protected override async ETTask OnAddBuff(BuffComponent buffComponent, BuffDataBase buff)
        {
            await this.OnApplyBuffEffect(buffComponent, buff);
        }

        protected override async ETTask OnApplyBuffEffect(BuffComponent buffComponent, BuffDataBase buff)
        {
            NumericBuff numericBuff = buff as NumericBuff;
            NumericComponent NumericComponent = buffComponent.GetParent<Unit>().GetComponent<NumericComponent>();
            for (int i = 0; i < numericBuff.Numerics.Length; i++)
            {
                NumericComponent[numericBuff.Numerics[i]] += numericBuff.NumericValues[i];
                numericBuff.TotalEffects[i] += numericBuff.NumericValues[i];
            }

            await ETTask.CompletedTask;
        }

        protected override async ETTask OnTickBuff(BuffComponent buffComponent, BuffDataBase buff, long currentTimeMs)
        {
            await ETTask.CompletedTask;
        }

        protected override async ETTask OnRemoveBuff(BuffComponent buffComponent, BuffDataBase buff)
        {
            NumericBuff numericBuff = buff as NumericBuff;
            NumericComponent NumericComponent = buffComponent.GetParent<Unit>().GetComponent<NumericComponent>();
            for (int i = 0; i < numericBuff.Numerics.Length; i++)
            {
                NumericComponent[numericBuff.Numerics[i]] -= numericBuff.TotalEffects[i];
            }

            await ETTask.CompletedTask;
        }

        protected override async ETTask OnExpiredBuff(BuffComponent buffComponent, BuffDataBase buff)
        {
            await this.OnRemoveBuff(buffComponent, buff);
        }

        public override bool CanHandleBuff(BuffType buffType)
        {
            return buffType == BuffType.Numeric;
        }
    }
}