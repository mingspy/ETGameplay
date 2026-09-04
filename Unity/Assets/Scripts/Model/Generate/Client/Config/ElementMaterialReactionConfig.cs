using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.ComponentModel;

namespace ET
{
    [Config]
    public partial class ElementMaterialReactionConfigCategory : Singleton<ElementMaterialReactionConfigCategory>, IMerge
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private Dictionary<int, ElementMaterialReactionConfig> dict = new();
		
        public void Merge(object o)
        {
            ElementMaterialReactionConfigCategory s = o as ElementMaterialReactionConfigCategory;
            foreach (var kv in s.dict)
            {
                this.dict.Add(kv.Key, kv.Value);
            }
        }
		
        public ElementMaterialReactionConfig Get(int id)
        {
            this.dict.TryGetValue(id, out ElementMaterialReactionConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (ElementMaterialReactionConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, ElementMaterialReactionConfig> GetAll()
        {
            return this.dict;
        }

        public ElementMaterialReactionConfig GetOne()
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

	public partial class ElementMaterialReactionConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		public int Id { get; set; }
		/// <summary>描述</summary>
		public string Description { get; set; }
		/// <summary>元素类型</summary>
		public int Element { get; set; }
		/// <summary>材质类型</summary>
		public int Material { get; set; }
		/// <summary>反应结果</summary>
		public int Reaction { get; set; }
		/// <summary>伤害加成</summary>
		public double DamageMultiplier { get; set; }
		/// <summary>反应后状态</summary>
		public int RemainElement { get; set; }
		/// <summary>元素计量消耗(%)</summary>
		public double GaugeConsumption { get; set; }
		/// <summary>VFX效果名称</summary>
		public string VfxName { get; set; }
		/// <summary>是否完全消耗元素</summary>
		public int IsConsumed { get; set; }
		/// <summary>目标施加BuffID</summary>
		public int[] ApplyBuffs { get; set; }

	}
}
