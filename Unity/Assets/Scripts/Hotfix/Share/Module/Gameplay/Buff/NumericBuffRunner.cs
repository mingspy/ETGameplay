namespace ET
{
    [BuffRunner]
    public class NumericBuffRunner: ABuffRunner<NumericBuff>
    {
        protected override async ETTask DoApplyBuffEffect(BuffComponent buffComponent, BuffDataBase buff)
        {
            NumericComponent NumericComponent = buffComponent.GetParent<Unit>().GetComponent<NumericComponent>();
            NumericBuff _buff = buff as NumericBuff;
            for (int i = 0; i < _buff.Numerics.Length; i++)
            {
                NumericComponent[_buff.Numerics[i]] += _buff.NumericValues[i];
                _buff.TotalEffects[i] += _buff.NumericValues[i];
            }
            
            await ETTask.CompletedTask;
        }

        protected override async ETTask DoTickBuff(BuffComponent buffComponent, BuffDataBase buff, long currentTimeMs)
        {
            await ETTask.CompletedTask;
        }

        protected override async ETTask DoRemoveBuff(BuffComponent buffComponent, BuffDataBase buff)
        {
            NumericComponent NumericComponent = buffComponent.GetParent<Unit>().GetComponent<NumericComponent>();
            NumericBuff _buff = buff as NumericBuff;
            for (int i = 0; i < _buff.Numerics.Length; i++)
            {
                NumericComponent[_buff.Numerics[i]] -= _buff.TotalEffects[i];
            }
            await ETTask.CompletedTask;
        }

        protected override async ETTask DoExpiredBuff(BuffComponent buffComponent, BuffDataBase buff)
        {
            await DoRemoveBuff(buffComponent, buff);
        }
        
        public override bool CanHandleBuff(BuffType buffType)
        {
            return buffType == BuffType.Numeric;
        }
    }
}