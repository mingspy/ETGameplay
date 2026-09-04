using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 负责buff管理，包括应用，移除和更新
    /// </summary>
    [ComponentOf(typeof (Unit))]
    public class BuffComponent: Entity, IAwake, IUpdate
    {
        public Dictionary<int, BuffInstance> Buffs { get; set; }
        public Unit Unit { get; set; }
    }
    
    public struct OnBuffAddedEvent
    {
        public Unit Unit;
        public BuffInstance BuffConfig;
    }
    
    public struct OnBuffRemovedEvent
    {
        public Unit Unit;
        public BuffInstance BuffConfig;
    }

}

