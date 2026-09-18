namespace ET
{
    [BuffRunner]
    public class ControlBuffRunner: ABuffRunner<ControlBuff>
    {
        protected override async ETTask DoApplyBuffEffect(BuffComponent buffComponent, BuffDataBase buff)
        {
            ControlBuff control  = buff as ControlBuff;
            buffComponent.GetParent<Unit>().GetComponent<StateComponent>().AddState(control.State);
            await ETTask.CompletedTask;
        }

        protected override async ETTask DoTickBuff(BuffComponent buffComponent, BuffDataBase buff, long currentTimeMs)
        {
            await ETTask.CompletedTask;
        }

        protected override async ETTask DoRemoveBuff(BuffComponent buffComponent, BuffDataBase buff)
        {
            ControlBuff control  = buff as ControlBuff;
            buffComponent.GetParent<Unit>().GetComponent<StateComponent>().RemoveState(control.State);
            await ETTask.CompletedTask;
        }

        protected override async ETTask DoExpiredBuff(BuffComponent buffComponent, BuffDataBase buff)
        {
            await DoRemoveBuff(buffComponent, buff);
        }
        
        public override bool CanHandleBuff(BuffType buffType)
        {
            return (buffType & BuffType.Control) == BuffType.Control;
        }
    }
}