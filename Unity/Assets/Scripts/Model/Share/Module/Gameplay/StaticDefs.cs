namespace ET
{
    /// <summary>
    ///     Buff配置ID常量定义，【注意这里为Demo，Demo buff系统的实现逻辑】
    /// </summary>
    public static class BuffConfigId
    {
        // 英雄被动
        public const int LubanPassive = 10100; // 鲁班七号被动-火力压制
        public const int MachaoPassive = 10200; // 马超被动-魔影突袭
        public const int DajiPassive = 10300; // 妲己被动-失心

        // 装备被动
        public const int Equipment_Moshi_Passive = 20001; // 末世-破败
        public const int Equipment_Qixue_Passive = 20002; // 泣血之刃-吸血
        public const int Equipment_Zhicai_Passive = 20003; // 制裁之刃-重伤
        public const int Equipment_Pojun_Passive = 20004; // 破军-破军
    }

    /// <summary>
    ///     英雄ID常量
    /// </summary>
    public static class HeroId
    {
        public const int Lubanqihao = 101; // 鲁班七号
        public const int Machao = 102; // 马超
        public const int Daji = 103; // 妲己
    }

    public static class SkillDefs
    {
        /// <summary>
        ///     英雄联盟防御伤害减免常数
        /// </summary>
        public const float DefenseMitigation_C_LOL = 100; //英雄联盟防御伤害减免常数

        /// <summary>
        ///     王者荣耀防御伤害减免常数
        /// </summary>
        public const float DefenseMitigation_C_KG = 602; //王者荣耀防御伤害减免

#if MOBA_LIKE_LOL
        public const float DefenseMitigation_C = DefenseMitigation_C_LOL
#else
        public const float DefenseMitigation_C = DefenseMitigation_C_KG;
#endif
    }
}