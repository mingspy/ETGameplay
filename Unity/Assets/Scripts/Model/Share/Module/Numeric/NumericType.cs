namespace ET
{
    


// 这个可弄个配置表生成
public static class NumericTypeSuffix
{
    public const int Base = 1;
    public const int Add = 2;
    public const int Pct = 3;
    public const int FinalAdd = 4;
    public const int FinalPct = 5;
}

/// <summary>
///     相当GAS中的AttributeTag。数值的的规则是 xBase = x * 10 + 1, xAdd = x * 10 + 2，以此类推 xFinalPct = x * 10 + 5。
/// </summary>
public static class NumericType
{
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

    /// <summary>
    ///     为了减少计算，按照<see cref="DamageType" />顺序分配伤害、护甲、穿透字段。<br />
    ///     【伤害起始字段， 该字段不使用只占位，辅助计算】
    /// </summary>
    public const int DamageStart = 1100;

    // 真伤，使用
    public const int TrueDamage = 1100;

    //攻击力 
    public const int Attack = 1101;

    //法强
    public const int Magic = 1102;

    // 元素伤害
    public const int Fire = 1103; // 火
    public const int Water = 1104; // 水
    public const int Ice = 1105; // 冰
    public const int Electricity = 1106; // 雷 / 电
    public const int Wind = 1107; // 风
    public const int Earth = 1108; // 地 / 土
    public const int Light = 1109; //  光 / 圣
    public const int Dark = 1110; //  暗 / 邪
    // 元素伤害结束

    #endregion

    #region 护甲 (int 类型)

    /// <summary>
    ///     为了减少计算，按照<see cref="DamageType" />顺序分配伤害、护甲、穿透字段。<br />
    ///     【护甲起始字段， 该字段不使用只占位，辅助计算】
    /// </summary>
    public const int ResistStart = 1200;

    //护甲
    public const int Armor = 1201;

    //魔抗
    public const int MagicResist = 1202;

    // 元素抗性
    public const int FireResist = 1203; // 火
    public const int WaterResist = 1204; // 水
    public const int IceResist = 1205; // 冰
    public const int ElectricityResist = 1206; // 雷 / 电
    public const int WindResist = 1207; // 风
    public const int EarthResist = 1208; // 地 / 土
    public const int LightResist = 1209; //  光 / 圣

    public const int DarkResist = 1210; //  暗 / 邪
    // 元素抗性结束

    #endregion

    #region 固定穿透 (int 类型)

    /// <summary>
    ///     为了减少计算，按照<see cref="DamageType" />顺序分配伤害、护甲、穿透字段。<br />
    ///     【护甲穿透 起始字段， 该字段不使用只占位，辅助计算】
    /// </summary>
    public const int PenetrationFlatStart = 1300;

    //护甲穿透（int)固定穿透
    public const int ArmorPenetrationFlat = 1301;

    //法术穿透 （int)固定穿透
    public const int MagicPenetrationFlat = 1302;

    //元素穿透 (int)
    public const int FirePenetrationFlat = 1303;
    public const int WaterPenetrationFlat = 1304; // 水
    public const int IcePenetrationFlat = 1305; // 冰
    public const int ElectricityPenetrationFlat = 1306; // 雷 / 电
    public const int WindPenetrationFlat = 1307; // 风
    public const int EarthPenetrationFlat = 1308; // 地 / 土
    public const int LightPenetrationFlat = 1309; //  光 / 圣
    public const int DarkPenetrationFlat = 1310; //  暗 / 邪

    #endregion

    #region 护甲穿透比例(float 类型)

    /// <summary>
    ///     为了减少计算，按照<see cref="DamageType" />顺序分配伤害、护甲、穿透字段。<br />
    ///     【护甲穿透比例 起始字段， 该字段不使用只占位，辅助计算】
    /// </summary>
    public const int PenetrationPercentStart = 1400;

    //护甲穿透比例 (float)
    public const int ArmorPenetrationPercent = 1401;

    //法术穿透比例 (float)
    public const int MagicPenetrationPercent = 1402;

    //元素穿透比例 
    public const int FirePenetrationPercent = 1403;
    public const int WaterPenetrationPercent = 1404; // 水
    public const int IcePenetrationPercent = 1405; // 冰
    public const int ElectricityPenetrationPercent = 1406; // 雷 / 电
    public const int WindPenetrationPercent = 1407; // 风
    public const int EarthPenetrationPercent = 1408; // 地 / 土
    public const int LightPenetrationPercent = 1409; //  光 / 圣

    public const int DarkPenetrationPercent = 1410; //  暗 / 邪
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
    public const int LifestealRate = 1503;

    public const int MagicLifestealRate = 1504;

    /// <summary>
    ///     反伤(float)
    /// </summary>
    public const int ThornmailRate = 1505;

    //生命恢复
    public const int HPRec = 1801;

    //魔法恢复
    public const int MPRec = 1802;

    //等级
    public const int Level = 1999;

    // 2xxx 为对应最大值
    //最大生命值
    public const int MaxHp = 2001;

    //最大魔法值
    public const int MaxMp = 2002;

    //最大等级
    public const int MaxLevel = 2999;
}

}