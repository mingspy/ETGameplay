namespace ET
{
    public interface IPassiveExecutor
    {
        //ETTask<bool> Execute<T>(Unit owner, PassiveDataBase passiveData, T args) where T: IPassiveEvent;
    }


    [EnableClass]
    public abstract class APassiveExecutor<A> : IPassiveExecutor where A : IPassiveEvent
    {
        public abstract ETTask<bool> Execute(Unit owner, PassiveDataBase passiveData, A eventData);
    }
}