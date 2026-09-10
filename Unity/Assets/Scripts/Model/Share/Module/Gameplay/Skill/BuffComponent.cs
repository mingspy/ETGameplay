using System.Collections.Generic;
using MemoryPack;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    /// <summary>
    /// 负责buff管理，包括应用，移除和更新。
    /// TODO: 目前实现的Buff只能实现修改BUFF拥有者的属性，还不支持被动和AOE和继续传导，用被动天赋组件来实现，挂一个buff，只做标记，被动组件监控BUFF状态，实现被动效果。
    /// TODO: 目前吸血被动，直接在攻击触发时实现，后续挪出来，解耦。
    /// </summary>
    [ComponentOf(typeof(Unit))]
    public class BuffComponent : Entity, IAwake, IUpdate
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