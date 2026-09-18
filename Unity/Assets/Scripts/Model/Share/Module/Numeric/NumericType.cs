namespace ET
{
    /// <summary>
    ///     NumericType 相当GAS中的AttributeTag。 属性值计算公式：
    ///     final = ((Base + Add) * (1 + Pct) + FinalAdd)* ( 1 + FinalPct);<br />
    ///     保存这些基数的规则是，对于NumericType x,  xBase = x * 10 + 1, xAdd = x * 10 + 2，以此类推 xFinalPct = x * 10 + 5。<br />
    /// </summary>
    public static class NumericType
    {
        private const int _Base = 1;
        private const int _Add = 2;
        private const int _Pct = 3;
        private const int _FinalAdd = 4;
        private const int _FinalPct = 5;

        /// <summary>
        ///     int 转float的乘数
        /// </summary>
        public const int FLOAT_INT_MULTIPLY = 10000;

        /// <summary>
        ///     numeric 的基数索引，比如Hp的基数 = Hp * 10 + 1
        /// </summary>
        /// <param name="numeric"></param>
        /// <returns></returns>
        public static int Base(int numeric)
        {
            return numeric * 10 + _Base;
        }

        /// <summary>
        /// </summary>
        /// <param name="numeric"></param>
        /// <returns></returns>
        public static int Add(int numeric)
        {
            return numeric * 10 + _Add;
        }

        public static int Pct(int numeric)
        {
            return numeric * 10 + _Pct;
        }

        public static int FinalAdd(int numeric)
        {
            return numeric * 10 + _FinalAdd;
        }

        public static int FinalPct(int numeric)
        {
            return numeric * 10 + _FinalPct;
        }

        public static int AsInt(float value)
        {
            return (int)(value * FLOAT_INT_MULTIPLY);
        }

        public static float AsFloat(int value)
        {
            return (float)value / FLOAT_INT_MULTIPLY;
        }

        //小于此值的都被认为是原始属性
        public const int Max = 10000;
        public const int AOI = 9999;
        public const int State = 9998;

        //生命值
        public const int Hp = 1001;

        //魔法值
        public const int Mp = 1002;

        //移动速度
        public const int Speed = 1003;

        //攻击速度
        public const int AttackSpeed = 1004;
        //攻速收益
        //public const int AttackSpeedIncome = 1006;

        //攻击距离
        public const int AttackRange = 1005;

        #region 伤害 (int 类型)

        // 真伤，使用
        public const int TrueDamage = 1100;

        /// <summary>
        ///     为了减少计算，按照<see cref="DamageType" />顺序分配伤害、护甲、穿透字段。<br />
        ///     【伤害起始字段， 该字段不使用只占位，辅助计算】
        /// </summary>
        public const int Placeholder_DamageStart = 1100;

        //攻击力, 物理攻击
        public const int Attack = 1101;

        //法强
        public const int Magic = 1102;

        // 元素伤害
        public const int Metal = 1103;
        public const int Wood = 1104;
        public const int Water = 1105; // 水
        public const int Fire = 1106; // 火
        public const int Earth = 1107; // 地 / 土
        public const int Wind = 1108; // 风
        public const int Lightning = 1109; // 雷 / 电
        public const int Light = 1110; //  光 / 圣
        public const int Dark = 1111; //  暗 / 邪
        public const int Poison = 1112; // 毒 (或 Toxin)
        public const int Oil = 1113; //  油，易燃，易爆 
        public const int Ice = 1114; // 冰

        #endregion

        #region 护甲 (int 类型)

        /// <summary>
        ///     为了减少计算，按照<see cref="DamageType" />顺序分配伤害、护甲、穿透字段。<br />
        ///     【护甲起始字段， 该字段不使用只占位，辅助计算】
        /// </summary>
        public const int Placeholder_ResistStart = 1200;

        //护甲
        public const int Armor = 1201;

        //魔抗
        public const int MagicResist = 1202;

        // 元素抗性
        public const int MetalResist = 1203;
        public const int WoodResist = 1204;
        public const int WaterResist = 1205; // 水
        public const int FireResist = 1206; // 火
        public const int EarthResist = 1207; // 地 / 土
        public const int WindResist = 1208; // 风
        public const int LightningResist = 1209; // 雷 / 电
        public const int LightResist = 1210; //  光 / 圣
        public const int DarkResist = 1211; //  暗 / 邪
        public const int PoisonResist = 1212; // 毒 (或 Toxin)
        public const int OilResist = 1213; //  油，易燃，易爆 
        public const int IceResist = 1214; // 冰

        // 元素抗性结束

        #endregion

        #region 固定穿透 (int 类型)

        /// <summary>
        ///     Placeholder_PenetrationFlatStart
        ///     为了减少计算，按照<see cref="DamageType" />顺序分配伤害、护甲、穿透字段。<br />
        ///     【护甲穿透 起始字段， 该字段不使用只占位，辅助计算】
        /// </summary>
        public const int Placeholder_FlatPenStart = 1300;

        //护甲穿透（int)固定穿透
        public const int ArmorFlatPen = 1301;

        //法术穿透 （int)固定穿透
        public const int MagicFlatPen = 1302;

        //元素穿透 (int)
        public const int MetalFlatPen = 1303;
        public const int WoodFlatPen = 1304;
        public const int WaterFlatPen = 1305; // 水
        public const int FireFlatPen = 1306; // 火
        public const int EarthFlatPen = 1307; // 地 / 土
        public const int WindFlatPen = 1308; // 风
        public const int LightningFlatPen = 1309; // 雷 / 电
        public const int LightFlatPen = 1310; //  光 / 圣
        public const int DarkFlatPen = 1311; //  暗 / 邪
        public const int PoisonFlatPen = 1312; // 毒 (或 Toxin)
        public const int OilFlatPen = 1313; //  油，易燃，易爆 
        public const int IceFlatPen = 1314; // 冰

        #endregion

        #region 护甲穿透比例(float 类型)

        /// <summary>
        ///     Placeholder_PenetrationPercentStart
        ///     为了减少计算，按照<see cref="DamageType" />顺序分配伤害、护甲、穿透字段。<br />
        ///     【护甲穿透比例 起始字段， 该字段不使用只占位，辅助计算】
        /// </summary>
        public const int Placeholder_PctPenStart = 1400;

        //护甲穿透比例 (float)
        public const int ArmorPctPen = 1401;

        //法术穿透比例 (float)
        public const int MagicPctPen = 1402;

        //元素穿透比例 
        public const int MetalPctPen = 1403;
        public const int WoodPctPen = 1404;
        public const int WaterPctPen = 1405; // 水
        public const int FirePctPen = 1406; // 火
        public const int EarthPctPen = 1407; // 地 / 土
        public const int WindPctPen = 1408; // 风
        public const int LightningPctPen = 1409; // 雷 / 电
        public const int LightPctPen = 1410; //  光 / 圣
        public const int DarkPctPen = 1411; //  暗 / 邪
        public const int PoisonPctPen = 1412; // 毒 (或 Toxin)
        public const int OilPctPen = 1413; //  油，易燃，易爆 
        public const int IcePctPen = 1414; // 冰
        //元素穿透比例结束

        #endregion

        /// <summary>
        ///     冷却缩减比例(float)
        /// </summary>
        public const int CooldownReduction = 1500;

        /// <summary>
        ///     暴击率(float) <br />
        ///     Critical Strike Rate: 决定了普攻/技能触发暴击的‌概率‌，作用是提升伤害触发的覆盖率。低暴击率时暴击是随机惊喜，高暴击率时暴击变成常态输出，满足喜欢稳定输出的玩家偏好
        /// </summary>
        public const int CritChance = 1501;

        /// <summary>
        ///     暴击伤害加成比例(float类型) finalDamage  = Damage * ( 1 + CritDamagePct) <br />
        ///     Critical Strike Damage:决定了触发暴击后，最终伤害在基础值上的‌倍率加成‌，作用是拉高单次攻击的爆发力。暴伤越高，暴击跳字的爽感越强，满足喜欢赌高爆发、追求秒人快感的玩家偏好。
        /// </summary>
        public const int CritDamagePct = 1502;

        /// <summary>
        ///     吸血(float)
        /// </summary>
        public const int LifeStealRate = 1503;

        public const int MagicLifeStealRate = 1504;

        /// <summary>
        ///     反伤(float)
        /// </summary>
        public const int ThornMailRate = 1505;

        //等级
        public const int Level = 1999;

        // 2xxx 为对应最大值
        //最大生命值
        public const int MaxHp = 2001;

        //最大魔法值
        public const int MaxMp = 2002;

        //最大等级
        public const int MaxLevel = 2999;

        // 3xxx对应恢复速度
        //生命恢复
        public const int HPRec = 3001;

        //魔法恢复
        public const int MPRec = 3002;
    }
}