using System.Collections.Generic;
using MemoryPack;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    /// <summary>
    /// 负责技能的释放和管理
    /// </summary>
    [ComponentOf(typeof(Unit))]
    public class SkillComponent : Entity, IAwake
    {
        [MemoryPackIgnore]
        [BsonIgnore]
        public Unit Unit { get; set; }

        public Dictionary<int, long> Cooldowns { get; set; }
#if DEF_NPBehave
        public Dictionary<int, Root> ActiveTrees { get; set; } // 正在运行的行为树
#endif
    }
}