using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.ComponentModel;

namespace ET
{
    [Config]
    public partial class EquipConfigCategory : Singleton<EquipConfigCategory>, IMerge
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private Dictionary<int, EquipConfig> dict = new();
		
        public void Merge(object o)
        {
            EquipConfigCategory s = o as EquipConfigCategory;
            foreach (var kv in s.dict)
            {
                this.dict.Add(kv.Key, kv.Value);
            }
        }
		
        public EquipConfig Get(int id)
        {
            this.dict.TryGetValue(id, out EquipConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (EquipConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, EquipConfig> GetAll()
        {
            return this.dict;
        }

        public EquipConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            
            var enumerator = this.dict.Values.GetEnumerator();
            enumerator.MoveNext();
            return enumerator.Current; 
        }
    }

	public partial class EquipConfig: ProtoObject, IConfig
	{
		/// <summary>装备ID</summary>
		public int Id { get; set; }
		/// <summary>名字</summary>
		public string Name { get; set; }
		/// <summary>描述</summary>
		public string Description { get; set; }
		/// <summary>类型</summary>
		public int EquiqType { get; set; }
		/// <summary>槽位</summary>
		public int Slot { get; set; }
		/// <summary>修改的属性</summary>
		public int[] Numerics { get; set; }
		/// <summary>效果数值, int 属性等于原来值，float 值 * 10000</summary>
		public int[] EffectValues { get; set; }
		/// <summary>装备的附加效果，如减速，元素</summary>
		public int[] BuffIds { get; set; }

	}
}
