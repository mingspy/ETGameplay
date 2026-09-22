namespace ET.Passive
{
    [PassiveExecutor(1009)]
    public class CountPassiveExecutor: APassiveExecutor<NormalAttackHitEvent>
    {
        public override async ETTask<bool> Execute(Unit owner, PassiveDataBase passiveData, NormalAttackHitEvent eventData)
        {
            CountPassiveData countPassiveData = passiveData as CountPassiveData;
            countPassiveData.CurrentCount++;
            if (countPassiveData.CurrentCount < countPassiveData.Threshold) return false;
            if(countPassiveData.ResetOnTrigger) countPassiveData.CurrentCount = 0;
            BuffComponent buffComponent = owner.GetComponent<BuffComponent>();
            buffComponent.AddBuff(owner.Id, countPassiveData.TargetBuffId);
            await ETTask.CompletedTask;
            return true;
        }
    }
}