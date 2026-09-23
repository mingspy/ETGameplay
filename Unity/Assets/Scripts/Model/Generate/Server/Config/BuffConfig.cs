using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.ComponentModel;

namespace ET
{
    [Config]
    public partial class BuffConfigCategory : Singleton<BuffConfigCategory>, IMerge
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private Dictionary<int, BuffConfig> dict = new();
		
        public void Merge(object o)
        {
            BuffConfigCategory s = o as BuffConfigCategory;
            foreach (var kv in s.dict)
            {
                this.dict.Add(kv.Key, kv.Value);
            }
        }
		
        public BuffConfig Get(int id)
        {
            this.dict.TryGetValue(id, out BuffConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (BuffConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, BuffConfig> GetAll()
        {
            return this.dict;
        }

        public BuffConfig GetOne()
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

	public partial class BuffConfig: ProtoObject, IConfig
	{
		/// <summary>BuffID</summary>
		public int Id { get; set; }
		/// <summary>名字</summary>
		public string Name { get; set; }
		/// <summary>描述</summary>
		public string Description { get; set; }
		/// <summary>BuffType</summary>
		public int BuffType { get; set; }
		/// <summary>Tags</summary>
		public string[] Tags { get; set; }
		/// <summary>最多叠加层数</summary>
		public int MaxStack { get; set; }
		/// <summary>持续时间 单位毫秒</summary>
		public int Duration { get; set; }
		/// <summary>周期性, 0为没有，单位毫秒</summary>
		public int Interval { get; set; }
		/// <summary>修改的属性</summary>
		public int[] Numerics { get; set; }
		/// <summary>修改的值, int 属性设置成原来值；float 值需要 * 10000，比如0.5，这里设置成5000</summary>
		public int[] NumericValues { get; set; }
		/// <summary>监听的事件id</summary>
		public int[] ListenEvents { get; set; }

	}
}
