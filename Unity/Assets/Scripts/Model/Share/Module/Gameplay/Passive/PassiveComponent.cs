using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    /// <summary>
    /// 被动组件，每个Unit上挂载一个。<br/>
    /// 实现上和BuffComponent很像，可以用buff的那套机制实现，但是为了逻辑清晰单独实现。
    /// </summary>
    [ComponentOf(typeof(Unit))]
    public class PassiveComponent:  Entity, IAwake, IDestroy
    {
        // 按触发类型分组存储所有被动实例（普通 C# 对象）
        [BsonIgnore]
        public readonly Dictionary<PassiveType, List<PassiveDataBase>> PassiveMap = new();
        
        // 全量字典，用于按 Id 查找移除
        public readonly Dictionary<int, PassiveDataBase> AllPassives = new();
    }
}