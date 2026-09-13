using System.Collections.Generic;
using MemoryPack;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    [ComponentOf(typeof(Unit))]
    public class EquipComponent : Entity, IAwake
    {
        public Dictionary<int, EquipConfig> Equips = new(); // 装备槽位->装备配置
    }
}