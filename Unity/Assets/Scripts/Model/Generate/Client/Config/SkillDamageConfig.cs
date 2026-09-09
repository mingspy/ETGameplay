using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.ComponentModel;

namespace ET
{
    [Config]
    public partial class SkillDamageConfigCategory : Singleton<SkillDamageConfigCategory>, IMerge
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private Dictionary<int, SkillDamageConfig> dict = new();
		
        public void Merge(object o)
        {
            SkillDamageConfigCategory s = o as SkillDamageConfigCategory;
            foreach (var kv in s.dict)
            {
                this.dict.Add(kv.Key, kv.Value);
            }
        }
		
        public SkillDamageConfig Get(int id)
        {
            this.dict.TryGetValue(id, out SkillDamageConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (SkillDamageConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, SkillDamageConfig> GetAll()
        {
            return this.dict;
        }

        public SkillDamageConfig GetOne()
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

	public partial class SkillDamageConfig: ProtoObject, IConfig
	{
		/// <summary>ID</summary>
		public int Id { get; set; }
		/// <summary>描述</summary>
		public string Description { get; set; }
		/// <summary>伤害类型</summary>
		public int DamageType { get; set; }
		/// <summary>Numeric伤害加成比例</summary>
		public double NumericRatio { get; set; }
		/// <summary>基础伤害</summary>
		public double FlatBaseValue { get; set; }
		/// <summary>是否可暴击,1可以</summary>
		public int CanCrit { get; set; }
		/// <summary>吸血比例</summary>
		public double LifestealRate { get; set; }

	}
}
