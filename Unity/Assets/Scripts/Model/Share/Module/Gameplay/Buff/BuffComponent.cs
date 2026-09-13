using System.Collections.Generic;
using MemoryPack;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    /// <summary>
    ///     负责buff管理，包括应用，移除和更新。
    ///     TODO: 目前实现的Buff只能实现修改BUFF拥有者的属性，还不支持被动和AOE和继续传导，用被动天赋组件来实现，挂一个buff，只做标记，被动组件监控BUFF状态，实现被动效果。
    ///     TODO: 目前吸血被动，直接在攻击触发时实现，后续挪出来，解耦。
    ///     TODO: 按照BUFF类型管理管理BUFF。
    ///     TODO: 支持非Config类型BUFF，比如技能添加一个BUFF，直接设置BUFF效果和时间。
    ///     TODO: 添加BUFF工厂
    /// </summary>
    [EnableMethod]
    [ComponentOf(typeof(Unit))]
    public class BuffComponent : Entity, IAwake, IUpdate
    {
        public Dictionary<int, BuffDataBase> Buffs { get; set; } = new Dictionary<int, BuffDataBase>(capacity: 10);
        
        [BsonElement]
        [MemoryPackInclude]
        private int idGenerator = 10000000;

        /// <summary>
        /// 构造BuffId
        /// </summary>
        /// <returns></returns>
        public int GenId()
        {
            return ++idGenerator;
        }
    }

    public struct OnBuffAddedEvent
    {
        public Unit Unit;
        public BuffDataBase Buff;
    }

    public struct OnBuffRemovedEvent
    {
        public Unit Unit;
        public BuffDataBase Buff;
    }
    
    public struct OnBuffExpiredEvent
    {
        public Unit Unit;
        public BuffDataBase Buff;
    }
}