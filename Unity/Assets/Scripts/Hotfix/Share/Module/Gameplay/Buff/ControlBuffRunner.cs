namespace ET
{
    [BuffRunner]
    public class ControlBuffRunner : ABuffRunner<ControlBuff>
    {
        protected override async ETTask OnAddBuff(BuffComponent buffComponent, BuffDataBase buff)
        {
            await this.OnApplyBuffEffect(buffComponent, buff);
        }

        protected override async ETTask OnApplyBuffEffect(BuffComponent buffComponent, BuffDataBase buff)
        {
            ControlBuff controlBuff = buff as ControlBuff;
            buffComponent.GetParent<Unit>().GetComponent<StateComponent>().AddState(controlBuff.State);
            await ETTask.CompletedTask;
        }

        protected override async ETTask OnTickBuff(BuffComponent buffComponent, BuffDataBase buff, long currentTimeMs)
        {
            await ETTask.CompletedTask;
        }

        protected override async ETTask OnRemoveBuff(BuffComponent buffComponent, BuffDataBase buff)
        {
            ControlBuff controlBuff = buff as ControlBuff;
            buffComponent.GetParent<Unit>().GetComponent<StateComponent>().RemoveState(controlBuff.State);
            await ETTask.CompletedTask;
        }

        protected override async ETTask OnExpiredBuff(BuffComponent buffComponent, BuffDataBase buff)
        {
            await this.OnRemoveBuff(buffComponent, buff);
        }

        public override bool CanHandleBuff(BuffType buffType)
        {
            return (buffType & BuffType.Control) == BuffType.Control;
        }
    }
}