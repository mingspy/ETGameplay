using System;

namespace ET
{
    /// <summary>
    ///    元素类型。⚠️ ElementalType有区别， 是附着在角色身上的管理单元类型，为了区分是元素还是材质<br/>
    /// ⚠️ 【数值不可以改动】，已在配置文件中使用数值定义。
    /// </summary>
    public enum ElementType
    {
        None = 0, // 非元素，
        Physical=1, // 物理, 不属于元素，但是为了方便转换到Numeric，占位。(当然也可以理解为物理攻击是通过 物理虚元素传递的 :P)
        Magical=2, // 魔法，不属于元素，但是为了方便转换到Numeric，占位。
        Metal = 3, // 金 / 铁， 破甲、磁吸、反射；常与雷形成导电或磁化反应，没有金元素，对应物理攻击？
        Wood = 4, // 木 / 自然，Wood， 治疗、缠绕、生长；常与火形成燃烧反应，与水形成滋养
        Water = 5, // 水
        Fire = 6, // 火
        Earth = 7, // 土/地
        Wind = 8, // 风
        Lightning = 9, // 雷 / 电 
        Light = 10, // 光 / 圣
        Dark = 11, // 暗 / 邪
        Poison = 12, // 毒 (或 Toxin)
        Oil = 13, //  油，易燃，易爆 
        Ice = 14 // 冰
 
    }

    /// <summary>
    ///     物理材质类型枚举。摘自 塞尔达传说，调整了顺序。塞尔达主要用于物理反馈，视觉和状态上的，比如火点燃草丛，产生气流，角色可以借助气流飞起来。<br/>
    /// ⚠️ 【数值不可以改动】，已在配置文件中使用数值定义。
    /// </summary>
    public enum MaterialType
    {
        /// <summary>
        ///     未定义 / 默认
        /// </summary>
        None = 0,
        
        /// <summary>
        ///     金属：导电性强，不可燃，易被磁化，高防御
        /// </summary>
        Metal = 1,  
        
        /// <summary>
        ///     预留未使用字段 0
        /// </summary>
        Unused0 = 2,
        
        /// <summary>
        ///     木材：易燃，可被火烧尽，浮力中等
        /// </summary>
        Wood = 3,
        
        
        /// <summary>
        ///     水：导电介质，熄灭火焰，冻结成冰
        /// </summary>
        Water = 4,
        
        /// <summary>
        ///     泥土：可燃性低，易附着湿润，绝缘
        /// </summary>
        Soil = 5,

        /// <summary>
        ///     沙子：不可燃，可被风吹散，易附着湿润
        /// </summary>
        Sand = 6,
        
        /// <summary>
        ///     深沙：大幅减速，陷入效果，不可燃
        /// </summary>
        HeavySand = 7,
        
        /// <summary>
        ///     石头：不可燃，绝缘，高防御，可被爆炸破坏
        /// </summary>
        Stone = 8,
        
        /// <summary>
        ///     熔岩：高温伤害源，点燃接触物，不可冻结
        /// </summary>
        Lava = 9,
        
        /// <summary>
        ///     铁丝网：导电，可攀爬，轻量
        /// </summary>
        WireNet = 10,
        
        /// <summary>
        ///     草地：易燃，可被火烧尽，易附着湿润
        /// </summary>
        Grass = 11,
        
        /// <summary>
        ///     雪：可被火融化成水，可被风吹散
        /// </summary>
        Snow = 12,

        /// <summary>
        ///     厚雪：比雪更深的积雪，陷入效果更强
        /// </summary>
        HeavySnow = 13,
        
        /// <summary>
        ///     冰：滑溜表面，可被火融化成水，可被重击破碎
        /// </summary>
        Ice = 14,

        /// <summary>
        ///     沼泽：减速区域，易附着湿润，不可燃
        /// </summary>
        Bog= 15,

        /// <summary>
        ///     布料：易燃，轻量，易被风吹动
        /// </summary>
        Cloth= 16,

        /// <summary>
        ///     玻璃：易碎，透明，绝缘，不可燃
        /// </summary>
        Glass= 17,

        /// <summary>
        ///     骨头：不可燃，绝缘，可被粉碎
        /// </summary>
        Bone= 18,

        /// <summary>
        ///     绳索：可燃，可切割，轻量
        /// </summary>
        Rope= 19,

        /// <summary>
        ///     冲浪板/水面滑行状态
        /// </summary>
        Surfing= 20,
        

        /// <summary>
        ///     弹射板：提供向上或向前的推力
        /// </summary>
        LaunchPad= 21,

        /// <summary>
        ///     传送带：提供持续的水平推力
        /// </summary>
        Conveyor= 22,
        
        /// <summary>
        ///     铁轨：引导移动方向，通常用于矿车等
        /// </summary>
        Rail= 23,

        /// <summary>
        ///     怨念/瘴气：特殊负面区域，扣除上限或持续掉血
        /// </summary>
        Grudge= 24,
        
        /// <summary>
        ///     怨念减速：特殊的怨念区域，主要效果为大幅减速
        /// </summary>
        GrudgeSlow= 25,

        /// <summary>
        ///     肉类：可燃，可烹饪，吸引野兽
        /// </summary>
        Meat= 26,

        /// <summary>
        ///     蔬菜：可燃，可烹饪，通常较轻
        /// </summary>
        Vegetable= 27,

        /// <summary>
        ///     炸弹：易爆炸，引燃后延时爆炸
        /// </summary>
        Bomb= 28,

        /// <summary>
        ///     魔法球：特殊投射物，可能带有元素属性
        /// </summary>
        MagicBall= 29,

        /// <summary>
        ///     屏障：不可穿透，通常用于阻挡攻击或移动
        /// </summary>
        Barrier= 30,

        /// <summary>
        ///     空气墙：隐形碰撞体，用于限制地图边界
        /// </summary>
        AirWall= 31,
        
        //CharControl, // 角色控制体：特殊碰撞层，通常用于玩家/怪物根节点
        
        //Ragdoll, //  布娃娃物理体：死亡或受击后的物理模拟状态
        
        // GuardianFoot, //守护者（如神庙守卫）脚部：特殊判定，通常用于触发战斗或特殊互动
        
        /// <summary>
        ///     杂项：其他未分类材质
        /// </summary>
        Misc= 32,

    }
    
    /// <summary>
    ///     元素反应类型。<br/>⚠️ 【数值不可以改动】，已在配置文件中使用数值定义。
    /// </summary>
    [Flags]
    public enum ReactionType
    {
        None = 0,
        /// <summary>
        ///     蒸发 ，  火 攻击 水 蒸发，伤害系数1.0； 水克火，水 攻击 火，也产生蒸发，伤害系数1.4
        /// </summary>
        Vaporize = 1,          // 蒸发 (火+水)
        Melt = 2,              // 融化 (火+冰 / 火+冰材质)
        Overload = 3,          // 超载 (火+雷)
        SuperConduct = 4,      // 超导 (雷+冰)
        ElectroCharged = 5,    // 感电 (雷+水)
        Conductive = 6,        // 导电 (雷+导电材质)
        Frozen = 7,            // 冻结 (冰+水)
        Shatter = 8,           // 碎冰 (物理+冰)
        Swirl = 9,             // 扩散 (风+元素)
        Crystallize = 10,      // 结晶 (岩+元素)
        Burning = 11,          // 燃烧 (火+可燃元素/材质)
    
        // ===== 五行克制 =====
        MetalRestrainNature = 12,  // 金克木
        NatureRestrainEarth = 13,  // 木克土
        EarthRestrainWater = 14,   // 土克水
        WaterRestrainFire = 15,    // 水克火
        FireRestrainMetal = 16,    // 火克金
    
        // ===== 光暗对立 =====
        Purify = 17,           // 光净化暗
        Corrupt = 18,          // 暗腐蚀光
    
        // ===== 毒系 =====
        PoisonCorrode = 19,    // 毒腐蚀
    
        // ===== 爆炸类 =====
        Explosion = 20,        // 爆炸 (火+油/炸弹)
    
        // ===== 材质交互 =====
        Ignite = 21,           // 点燃材质 (火+木/草/布等)
        Douse = 22,            // 浇灭/浸湿 (水+燃烧态)
        Electrify = 23,        // 材质导电 (雷+金属/水材质)
        FreezeWater = 24,      // 冻水成冰 (冰+水材质)
        MeltIceMaterial = 25,  // 冰材质融化 (火+冰材质)
        Petrify = 26,          // 石化 (土+某些材质)
        Weathering = 27,       // 风化 (风+土/沙)
        Sinter = 28,           // 烧结 (火+土/沙→玻璃)
        Annihilation = 29,     // 攻击元素消失，比如光攻击结界
        Refraction = 30,       // 光通过玻璃，产生折射
    }

    

    public struct CombatHitEvent
    {
        public long AttackerId{ get; set; }
        public long TargetId{ get; set; }
        public float Damage{ get; set; }
        public string ReactionVfxName{ get; set; }
    }

    /// <summary>
    /// 附着在角色身上的类型，随时间衰减
    /// ⚠️ 【数值不可以改动】，已在配置文件中使用数值定义。 
    /// </summary>
    public enum ElementalType{
        Elemental = 0, // 元素
        Material,  // 材质
        Reaction,  // 反应结果
    }

    public enum ElementalPosition
    {
        Attach = 0, // 默认附着在表面
        Surface,
        Base
    }

    /// <summary>
    ///     元素附着信息
    /// </summary>
    [EnableClass]
    public class Elemental
    {
        /// <summary>
        /// Element or Material Type
        /// </summary>
        public int Type { get; set; } // 根据AffixType的类型决定Element的类型
        public ElementalType ElementalType { get; set; }
        public ElementalPosition  ElementalPosition { get; set; }
        public int Gauge{ get; set; } // 元素残留数量
        public int DecayPerSecond{ get; set; }// 每秒消耗
        public long EndTime{ get; set; } // 结束时间
        public long LastUpdateTime{ get; set; }
        public ReactionType Reaction{ get; set; }
        
        /// <summary>
        /// 元素与元素反应产生的类型是元素，元素与材质反应产生的类型是材质。<br/>
        /// 如果当前反应位置是base，那么贴到surface， 如果当前是Surface，如果与base匹配，放到base，如果不匹配，替换surface。<br/>
        /// 如果是元素，那么Attach到Unit上。
        /// </summary>
        public int NewElement{ get; set; } // 反应产生的新元素 or 材质
        
        public long SourceId{ get; set; } // 附着来源（技能/装备/单位）

        public bool IsExpired(long currentTime)
        {
            if (currentTime >= EndTime || Gauge <= 0)
            {
                return true;
            }
            return false ;
        }

        public void Clear()
        {
            this.Type = 0;
            this.EndTime = 0;
        }
    }
    
     
    [EnableClass]
    public class ReactionInfo
    {
        // set By Request
        /// <summary>
        /// 反应的来源元素
        /// </summary>
        public ElementType SourceElement { get; set; }
        public int SourceAmount { get; set; }

        /// <summary>
        /// 当前反应传导深度
        /// </summary>
        public int CurrentDepth { get; set; }
        
        /// <summary>
        /// 反应结果
        /// </summary>
        public ReactionType Result{ get; set; }
        public float DamageMultiplier{ get; set; } // 伤害倍率（增幅反应）
        public float ExtraDamage{ get; set; } // 额外固定伤害（超载等）
        public int SourceUsage{ get; set; } // 源元素消耗量
        public int ReactantUsage{ get; set; }
        
        /// <summary>
        /// 反应对象
        /// </summary>
        public Elemental Reactant{ get; set; } // 反应对象
        public ReactionConfig  ReactionConfig{ get; set; } // 使用的反应规则
    }
}