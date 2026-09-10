using System.Collections.Generic;
using MemoryPack;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    /// <summary>
    /// 负责buff管理，包括应用，移除和更新
    /// </summary>
    [ComponentOf(typeof (Unit))]
    public class BuffComponent: Entity, IAwake, IUpdate
    {
        [MemoryPackIgnore]
        [BsonIgnore]
        public Unit Unit { get; set; }
        
        public Dictionary<int, BuffInstance> Buffs { get; set; }

    }
    
    public struct OnBuffAddedEvent
    {
        public Unit Unit;
        public BuffInstance Buff;
    }
    
    public struct OnBuffRemovedEvent
    {
        public Unit Unit;
        public BuffInstance Buff;
    }

}

