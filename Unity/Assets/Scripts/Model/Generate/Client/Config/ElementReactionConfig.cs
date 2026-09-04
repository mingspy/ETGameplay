using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.ComponentModel;

namespace ET
{
    [Config]
    public partial class ElementReactionConfigCategory : Singleton<ElementReactionConfigCategory>, IMerge
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private Dictionary<int, ElementReactionConfig> dict = new();
		
        public void Merge(object o)
        {
            ElementReactionConfigCategory s = o as ElementReactionConfigCategory;
            foreach (var kv in s.dict)
            {
                this.dict.Add(kv.Key, kv.Value);
            }
        }
		
        public ElementReactionConfig Get(int id)
        {
            this.dict.TryGetValue(id, out ElementReactionConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (ElementReactionConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, ElementReactionConfig> GetAll()
        {
            return this.dict;
        }

        public ElementReactionConfig GetOne()
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

	public partial class ElementReactionConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		public int Id { get; set; }
		/// <summary>描述</summary>
		public string Description { get; set; }
		/// <summary>源元素</summary>
		public int FromElement { get; set; }
		/// <summary>目标元素</summary>
		public int ToElement { get; set; }
		/// <summary>反应结果</summary>
		public int Reaction { get; set; }
		/// <summary>伤害加成</summary>
		public double DamageMultiplier { get; set; }
		/// <summary>反应后残留的元素，0表示清除</summary>
		public int RemainElement { get; set; }
		/// <summary>消耗目标元素量(%)</summary>
		public double GaugeConsumption { get; set; }
		/// <summary>VFX效果名称</summary>
		public string VfxName { get; set; }
		/// <summary>是否完全消耗目标</summary>
		public int IsConsumed { get; set; }
		/// <summary>目标施加BuffID列表</summary>
		public int[] ApplyBuffs { get; set; }

	}
}
