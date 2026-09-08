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
	/// 相当GAS中的AttributeTag。数值的的规则是 xBase = x * 10 + 1, xAdd = x * 10 + 2，以此类推 xFinalPct = x * 10 + 5。
	/// </summary>
    public static class  NumericType
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

	    //攻击力
	    public const int Attack = 1004;
	    //攻击速度
	    public const int AttackSpeed = 1005;
	    //攻速收益
	    public const int AttackSpeedIncome = 1006;
	    
	    //攻击距离
	    public const int AttackRange = 1007;
	    
	    //暴击率
	    public const int CriticalStrikeProbability = 1008;
	    
	    //暴击伤害
	    public const int CriticalStrikeHarm = 1009;
	    
	    //法强
	    public const int MagicStrength = 1010;
	    
	    //真伤
	    public const int TrueDamage = 1011;

	    //护甲
	    public const int Armor = 1020;

	    //魔抗
	    public const int MagicResistance = 1021;

	    //护甲穿透
	    public const int ArmorPenetration = 1022;

	    //法术穿透
	    public const int MagicPenetration = 1023;



	    //生命恢复
	    public const int HPRec = 1601;

	    //魔法恢复
	    public const int MPRec = 1602;




	    //等级
	    public const int Level = 1999;


	    
	    //最大生命值
	    public const int MaxHp = 2001;
	    //最大魔法值
	    public const int MaxMp = 2002;
	    //最大等级
	    public const int MaxLevel = 2999;
    }
}
