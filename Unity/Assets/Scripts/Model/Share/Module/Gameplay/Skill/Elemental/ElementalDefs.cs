namespace ET
{
    /// <summary>
    /// 元素类型
    /// </summary>
    public enum ElementalType
    {
        None = 0,
        Fire,
        Water,
        Ice,
        Electricity, // 雷 / 电
        Wind,
        Earth,  // 地 / 土
        Light,  //  光 / 圣
        Dark  //  暗 / 邪
    }


    /// <summary>
    /// 元素反应类型
    /// </summary>
    public enum ReactionType
    {
        None = 0,
        Vaporize,   // 蒸发 (火+水)
        Melt,       // 融化 (火+冰)
        Overload,   // 超载 (火+雷)
        Superconduct, // 超导 (冰+雷)
        ElectroCharged, // 感电 (水+雷)
        Frozen,     // 冻结 (水+冰)
        Shatter,    // 碎冰 (冻结+物理/重击)
        Swirl,      // 扩散 (风+其他)
        Crystallize, // 结晶 (岩+其他)
        Burning,    // 燃烧 (火+草/油)
        Conductive  // 传导 (雷+金属)
    }
    

        /// <summary>
    /// 物理材质类型枚举
    /// 用于定义物体表面的物理属性，决定其与元素、武器、环境的交互反应
    /// </summary>
    public enum MaterialType
    {
        /// <summary>
        /// 未定义 / 默认
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// 泥土：可燃性低，易附着湿润，绝缘
        /// </summary>
        Soil,

        /// <summary>
        /// 石头：不可燃，绝缘，高防御，可被爆炸破坏
        /// </summary>
        Stone,

        /// <summary>
        /// 沙子：不可燃，可被风吹散，易附着湿润
        /// </summary>
        Sand,

        /// <summary>
        /// 金属：导电性强，不可燃，易被磁化，高防御
        /// </summary>
        Metal,

        /// <summary>
        /// 铁丝网：导电，可攀爬，轻量
        /// </summary>
        WireNet,

        /// <summary>
        /// 草地：易燃，可被火烧尽，易附着湿润
        /// </summary>
        Grass,

        /// <summary>
        /// 木材：易燃，可被火烧尽，浮力中等
        /// </summary>
        Wood,

        /// <summary>
        /// 水：导电介质，熄灭火焰，冻结成冰
        /// </summary>
        Water,

        /// <summary>
        /// 雪：可被火融化成水，可被风吹散
        /// </summary>
        Snow,

        /// <summary>
        /// 冰：滑溜表面，可被火融化成水，可被重击破碎
        /// </summary>
        Ice,

        /// <summary>
        /// 熔岩：高温伤害源，点燃接触物，不可冻结
        /// </summary>
        Lava,

        /// <summary>
        /// 沼泽：减速区域，易附着湿润，不可燃
        /// </summary>
        Bog,

        /// <summary>
        /// 深沙：大幅减速，陷入效果，不可燃
        /// </summary>
        HeavySand,

        /// <summary>
        /// 布料：易燃，轻量，易被风吹动
        /// </summary>
        Cloth,

        /// <summary>
        /// 玻璃：易碎，透明，绝缘，不可燃
        /// </summary>
        Glass,

        /// <summary>
        /// 骨头：不可燃，绝缘，可被粉碎
        /// </summary>
        Bone,

        /// <summary>
        /// 绳索：可燃，可切割，轻量
        /// </summary>
        Rope,

        /// <summary>
        /// 角色控制体：特殊碰撞层，通常用于玩家/怪物根节点
        /// </summary>
        CharControl,

        /// <summary>
        /// 布娃娃物理体：死亡或受击后的物理模拟状态
        /// </summary>
        Ragdoll,

        /// <summary>
        /// 冲浪板/水面滑行状态
        /// </summary>
        Surfing,

        /// <summary>
        /// 守护者（如神庙守卫）脚部：特殊判定，通常用于触发战斗或特殊互动
        /// </summary>
        GuardianFoot,

        /// <summary>
        /// 厚雪：比雪更深的积雪，陷入效果更强
        /// </summary>
        HeavySnow,

        /// <summary>
        /// 预留未使用字段 0
        /// </summary>
        Unused0,

        /// <summary>
        /// 弹射板：提供向上或向前的推力
        /// </summary>
        LaunchPad,

        /// <summary>
        /// 传送带：提供持续的水平推力
        /// </summary>
        Conveyer,

        /// <summary>
        /// 铁轨：引导移动方向，通常用于矿车等
        /// </summary>
        Rail,

        /// <summary>
        /// 怨念/瘴气：特殊负面区域，扣除上限或持续掉血
        /// </summary>
        Grudge,

        /// <summary>
        /// 肉类：可燃，可烹饪，吸引野兽
        /// </summary>
        Meat,

        /// <summary>
        /// 蔬菜：可燃，可烹饪，通常较轻
        /// </summary>
        Vegetable,

        /// <summary>
        /// 炸弹：易爆炸，引燃后延时爆炸
        /// </summary>
        Bomb,

        /// <summary>
        /// 魔法球：特殊投射物，可能带有元素属性
        /// </summary>
        MagicBall,

        /// <summary>
        /// 屏障：不可穿透，通常用于阻挡攻击或移动
        /// </summary>
        Barrier,

        /// <summary>
        /// 空气墙：隐形碰撞体，用于限制地图边界
        /// </summary>
        AirWall,

        /// <summary>
        /// 杂项：其他未分类材质
        /// </summary>
        Misc,

        /// <summary>
        /// 怨念减速：特殊的怨念区域，主要效果为大幅减速
        /// </summary>
        GrudgeSlow
    }
    
    /// <summary>
    /// 元素反应数据
    /// </summary>
    public class ReactionRule : ETObject
    {
        public ReactionType Result;
        public float DamageMultiplier;
        public ElementalType Remain; // 反应后残留的元素，None表示清除
        public float GaugeConsumption;      // 消耗的目标元素计量值 (0-1)
        public string VfxName;
        public bool IsConsumed;             // 是否完全消耗目标元素
        public int[] ApplyBuffs;
    }
    
    public struct CombatHitEvent
    {
        public long AttackerId;
        public long TargetId;
        public float Damage;
        public string ReactionVfxName;
    }
}
