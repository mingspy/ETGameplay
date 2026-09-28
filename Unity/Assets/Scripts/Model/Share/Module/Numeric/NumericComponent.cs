using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace ET
{
    [FriendOf(typeof (NumericComponent))]
    public static class NumericComponentSystem
    {
        private const int _Base = 1;
        private const int _Add = 2;
        private const int _Pct = 3;
        private const int _FinalAdd = 4;
        private const int _FinalPct = 5;
        

        /// <summary>
        ///     numeric 的基数索引，比如Hp的基数 = Hp * 10 + 1
        /// </summary>
        public static int AttributeBase(this NumericComponent self,int numeric)
        {
            return numeric * 10 + _Base;
        }


        public static int AttributeAdd(this NumericComponent self, int numeric)
        {
            return numeric * 10 + _Add;
        }

        public static int AttributePct(this NumericComponent self,int numeric)
        {
            return numeric * 10 + _Pct;
        }

        public static int AttributeFinalAdd(this NumericComponent self,int numeric)
        {
            return numeric * 10 + _FinalAdd;
        }

        public static int AttributeFinalPct(this NumericComponent self,int numeric)
        {
            return numeric * 10 + _FinalPct;
        }
        
        public static float GetAsFloat(this NumericComponent self, int numericType)
        {
            return NumericComponent.ValueAsFloat(self.GetByKey(numericType));
        }

        public static int GetAsInt(this NumericComponent self, int numericType)
        {
            return (int)self.GetByKey(numericType);
        }

        public static long GetAsLong(this NumericComponent self, int numericType)
        {
            return self.GetByKey(numericType);
        }

        public static void Set(this NumericComponent self, int nt, float value)
        {
            self[nt] = NumericComponent.ValueAsLong(value);
        }

        public static void Set(this NumericComponent self, int nt, int value)
        {
            self[nt] = value;
        }

        public static void Set(this NumericComponent self, int nt, long value)
        {
            self[nt] = value;
        }

        public static void SetNoEvent(this NumericComponent self, int numericType, long value)
        {
            self.Insert(numericType, value, false);
        }

        public static void Insert(this NumericComponent self, int numericType, long value, bool isPublicEvent = true)
        {
            long oldValue = self.GetByKey(numericType);
            if (oldValue == value)
            {
                return;
            }

            self.NumericDic[numericType] = value;

            if (numericType >= NumericType.Max)
            {
                self.Update(numericType, isPublicEvent);
                return;
            }

            if (isPublicEvent)
            {
                EventSystem.Instance.Publish(self.Scene(),
                    new NumbericChange() { Unit = self.GetParent<Unit>(), New = value, Old = oldValue, NumericType = numericType });
            }
        }

        public static long GetByKey(this NumericComponent self, int key)
        {
            long value = 0;
            self.NumericDic.TryGetValue(key, out value);
            return value;
        }

        public static void Update(this NumericComponent self, int numericType, bool isPublicEvent)
        {
            int final = (int)numericType / 10;
            int bas = final * 10 + 1;
            int add = final * 10 + 2;
            int pct = final * 10 + 3;
            int finalAdd = final * 10 + 4;
            int finalPct = final * 10 + 5;

            // 一个数值可能会多种情况影响，比如速度,加个buff可能增加速度绝对值100，也有些buff增加10%速度，所以一个值可以由5个值进行控制其最终结果
            // final = (((base + add) * (100 + pct) / 100) + finalAdd) * (100 + finalPct) / 100;
            long result = (long)(((self.GetByKey(bas) + self.GetByKey(add)) * (100 + self.GetAsFloat(pct)) / 100f + self.GetByKey(finalAdd)) *
                (100 + self.GetAsFloat(finalPct)) / 100f);
            self.Insert(final, result, isPublicEvent);
        }
    }
    
    public struct NumbericChange
    {
        public Unit Unit;
        public int NumericType;
        public long Old;
        public long New;
    }

    [ComponentOf(typeof (Unit))]
    [EnableMethod]
    public class NumericComponent: Entity, IAwake, ITransfer
    {
        /// <summary>
        ///     int 转float的乘数
        /// </summary>
        public const int FLOAT_INT_MULTIPLY = 10000;
        
        /// <summary>
        /// 存放数值属性的所有计算结果值，由GAS更新。这里实现的NumericComponent 相当于GAS中的AttributeValue集合。
        /// Key: NumericType，规则
        /// </summary>
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<int, long> NumericDic = new Dictionary<int, long>();

        public long this[int numericType]
        {
            get
            {
                return this.GetByKey(numericType);
            }
            set
            {
                this.Insert(numericType, value);
            }
        }
        
        public static long ValueAsLong(float value)
        {
            return (long)(value * FLOAT_INT_MULTIPLY);
        }

        public static float ValueAsFloat(long value)
        {
            return (float)value / FLOAT_INT_MULTIPLY;
        }
    }
}