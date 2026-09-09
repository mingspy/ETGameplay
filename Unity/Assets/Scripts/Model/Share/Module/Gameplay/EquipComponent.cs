using System.Collections.Generic;

namespace ET
{
    [ComponentOf(typeof (Unit))]
    public class EquipComponent: Entity, IAwake
    {
        public Unit Unit{ get; set; }
        public Dictionary<int, EquipConfig> Equips = new(); // 装备槽位->装备配置
        
    }
}

