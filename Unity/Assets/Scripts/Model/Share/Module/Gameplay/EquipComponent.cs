using System.Collections.Generic;
using MemoryPack;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    [ComponentOf(typeof (Unit))]
    public class EquipComponent: Entity, IAwake
    {
        [MemoryPackIgnore]
        [BsonIgnore]
        public Unit Unit{ get; set; }
        public Dictionary<int, EquipConfig> Equips = new(); // 装备槽位->装备配置
        
    }
}

