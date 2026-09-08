using System;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 存储所有Type(一般是组件)的system实现，保存在 <see cref="typeSystemsMap"/>
    /// </summary>
    public class TypeSystems
    {
        /// <summary>
        /// 用于存储一个OneType的所有子接口实现,这里的OneType一般为组件。<br />
        /// 成员<see cref="Map"/>中存的是OneType所有systems的实现，比如 key 是 IUpdateSystem, value 为 List&lt;SystemObject&gt;.
        /// </summary>
        public class OneTypeSystems
        {
            public OneTypeSystems(int count)
            {
                this.QueueFlag = new bool[count];
            }
            
            public readonly UnOrderMultiMap<Type, SystemObject> Map = new();
            // 这里不用hash，数量比较少，直接for循环速度更快
            public readonly bool[] QueueFlag;
        }

        private readonly int count;

        public TypeSystems(int count)
        {
            this.count = count;
        }
        
        private readonly Dictionary<Type, OneTypeSystems> typeSystemsMap = new();

        public OneTypeSystems GetOrCreateOneTypeSystems(Type type)
        {
            OneTypeSystems systems = null;
            this.typeSystemsMap.TryGetValue(type, out systems);
            if (systems != null)
            {
                return systems;
            }

            systems = new OneTypeSystems(this.count);
            this.typeSystemsMap.Add(type, systems);
            return systems;
        }

        public OneTypeSystems GetOneTypeSystems(Type type)
        {
            OneTypeSystems systems = null;
            this.typeSystemsMap.TryGetValue(type, out systems);
            return systems;
        }

        public List<SystemObject> GetSystems(Type type, Type systemType)
        {
            OneTypeSystems oneTypeSystems = null;
            if (!this.typeSystemsMap.TryGetValue(type, out oneTypeSystems))
            {
                return null;
            }

            if (!oneTypeSystems.Map.TryGetValue(systemType, out List<SystemObject> systems))
            {
                return null;
            }

            return systems;
        }
    }
}