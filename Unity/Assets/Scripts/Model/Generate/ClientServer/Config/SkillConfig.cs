using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.ComponentModel;

namespace ET
{
    [Config]
    public partial class SkillConfigCategory : Singleton<SkillConfigCategory>, IMerge
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private Dictionary<int, SkillConfig> dict = new();
		
        public void Merge(object o)
        {
            SkillConfigCategory s = o as SkillConfigCategory;
            foreach (var kv in s.dict)
            {
                this.dict.Add(kv.Key, kv.Value);
            }
        }
		
        public SkillConfig Get(int id)
        {
            this.dict.TryGetValue(id, out SkillConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (SkillConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, SkillConfig> GetAll()
        {
            return this.dict;
        }

        public SkillConfig GetOne()
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

	public partial class SkillConfig: ProtoObject, IConfig
	{
		/// <summary>技能ID</summary>
		public int Id { get; set; }
		/// <summary>名字</summary>
		public string Name { get; set; }
		/// <summary>技能描述</summary>
		public string Description { get; set; }
		/// <summary>冷却</summary>
		public double CoolDown { get; set; }
		/// <summary>蓝耗</summary>
		public int ManaCost { get; set; }
		/// <summary>前摇时间</summary>
		public double WindupTime { get; set; }
		/// <summary>后摇时间</summary>
		public double RecoveryTime { get; set; }
		/// <summary>总时间</summary>
		public double Duration { get; set; }
		/// <summary>释放时自身增加的buff</summary>
		public int[] CastBuffs { get; set; }
		/// <summary>命中时增加的buff</summary>
		public int[] HitBuffs { get; set; }

	}
}
