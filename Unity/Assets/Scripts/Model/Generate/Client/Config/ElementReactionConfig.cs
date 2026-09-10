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
		/// <summary>反应结果，火烧木头为例，产生结果是持续燃烧，直到木头烧完。结果会挂在材质表面。</summary>
		public int Reaction { get; set; }
		/// <summary>伤害放大系数，只针对元素伤害放大, > 1 为放大， < 1 为减少伤害。</summary>
		public double DamageMultiplier { get; set; }
		/// <summary>反应强度，0不发生反应， 一般Intensity 设置1。 攻击者不消耗元素量，反应结果是, 被攻击者元素销量 和产生的伤害都是 Min(攻击者元素量 * Intensity, 被攻击者元素销量)</summary>
		public double Intensity { get; set; }
		/// <summary>反应后剩余元素每秒消耗量, 0不消耗。比如火点燃了木头，后续持续消耗剩余的木头。</summary>
		public int DaceyPerSecond { get; set; }
		/// <summary>反应后附加元素的持续时间，单位毫秒。如果一直燃烧，直到结束，设置一个较大值。</summary>
		public int Duration { get; set; }
		/// <summary>反应后附加的BUFF，比如持续伤害，持续周边AOE，持续蔓延。</summary>
		public int[] ApplyBuffs { get; set; }
		/// <summary>VFX效果名称</summary>
		public string VfxName { get; set; }

	}
}
