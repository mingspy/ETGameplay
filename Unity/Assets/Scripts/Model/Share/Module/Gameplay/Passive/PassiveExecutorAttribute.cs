namespace ET
{
    
    public class PassiveExecutorAttribute: BaseAttribute
    {
        public int PassiveId { get; }

        public PassiveExecutorAttribute(int passiveId)
        {
            this.PassiveId = passiveId;
        }
    }
}