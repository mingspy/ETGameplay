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
		/// <summary>伤害系数。基础伤害  = 攻击力 * 伤害系数 + 固定伤害值 = 攻击力 * Coefficient + BaseValue</summary>
		public double Coefficient { get; set; }
		/// <summary>技能固定伤害值</summary>
		public int BaseDamage { get; set; }
		/// <summary>是否可暴击,1可以</summary>
		public int CanCrit { get; set; }
		/// <summary>吸血比例</summary>
		public double LifeStealRate { get; set; }
		/// <summary>0:立即触发，1：延迟，2：持续伤害</summary>
		public int DamageTriggerType { get; set; }
		/// <summary>延迟伤害执行时间，单位毫秒</summary>
		public int DelayTime { get; set; }
		/// <summary>命中时自身添加的BuffID</summary>
		public int[] HitBuffIdsForSelf { get; set; }
		/// <summary>命中时队友添加的BuffID</summary>
		public int[] HitBuffIdsForTeam { get; set; }
		/// <summary>命中时目标添加的BuffID</summary>
		public int[] HitBuffIdsForTarget { get; set; }

	}
}
