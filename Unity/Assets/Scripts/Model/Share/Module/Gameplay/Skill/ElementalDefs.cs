using System;

namespace ET
{
    /*
       | 原始英文 | 推荐中文译名 | 代码枚举建议 (PascalCase) | 命名优化/别名建议 | 备注 |
       | --- | --- | --- | --- | --- |
       | Fire | 火 / 烈焰 | Fire | Flame, Pyro | "Pyro" 常用于技能前缀或职业分类 |
       | Water | 水 / 流水 | Water | Hydro, Aqua | "Hydro" 更具魔法感，常见于元素反应体系 |
       | Ice | 冰 / 寒冰 | Ice | Frost, Cryo | "Frost" 侧重寒冷状态，"Cryo" 侧重冰冻机制 |
       | Electricity | 雷 / 电 | Lightning | Thunder, Electro, Volt | 建议修改：Electricity 过于物理化。MOBA中常用 Lightning (闪电) 或 Thunder (雷霆)。"Electro" 常见于二次元 RPG。 |
       | Wind | 风 / 疾风 | Wind | Anemo, Gale, Air | "Anemo" 是特定游戏术语，通用推荐 Wind 或 Gale (狂风) |
       | Earth | 地 / 土 | Earth | Geo, Terra, Stone | "Geo" 侧重岩元素，"Terra" 更具古老魔法感 |
       | Light | 光 / 圣 | Light | Holy, Radiant, Lux | RPG中若涉及神职，常用 Holy (神圣)；MOBA中常用 Light |
       | Dark | 暗 / 邪 | Dark | Shadow, Void, Necro | RPG中若涉及亡灵/邪恶，常用 Shadow (暗影) 或 Necro (死灵) |
       | Poison | 毒 / 毒素 | Poison | Toxin, Venom, Bio | "Toxin" 侧重化学/自然毒，"Venom" 侧重生物毒液，"Bio" 侧重生化科幻 |
       
       | 新增元素英文 | 推荐中文译名 | 代码枚举建议 | 典型应用场景/机制 |
       | --- | --- | --- | --- |
       | Nature | 自然 / 木 | Nature | 治疗、缠绕、生长；常与火形成燃烧反应，与水形成滋养 |
       | Arcane | 奥术 / 秘法 | Arcane | 纯魔法伤害、穿透护盾、沉默；代表纯粹的能量 |
       | Physical | 物理 / 普攻 | Physical | 非元素伤害，用于区分魔法抗性 vs 物理防御 |
       | Chaos | 混沌 / 乱 | Chaos | 随机效果、混乱状态、真实伤害；常用于高阶 Boss 或特殊职业 |
       | Time | 时间 / 时 | Time | 减速、加速、回溯、停滞；高阶控制类元素 |
       | Space | 空间 / 空 | Space | 传送、位移、切割、维度打击；高机动性或爆发伤害 |
       | Blood | 血 / 鲜血 | Blood | 吸血、献祭、狂战士机制；以生命值换取力量 |
       | Sound | 音 / 声 | Sound | 眩晕、沉默、范围干扰；较少见但具有独特控制效果 |
       | Metal | 金 / 铁 | Metal | 破甲、磁吸、反射；常与雷形成导电或磁化反应 |
       | Mist | 雾 / 幻 | Mist | 隐身、闪避提升、致盲；辅助或刺客类元素 |
     */
    /// <summary>
    /// 元素类型
    /// </summary>
    public enum ElementalType
    {
        None = 0,
        // --- 基础自然元素 ---
        Fire ,   // 火
        Water ,   // 水
        Ice  ,   // 冰
        Lightning ,   // 雷 (替代 Electricity)
        Wind,   // 风
        Earth ,   // 地
        // --- 对立/概念元素 ---
        Light ,   // 光 (或 Holy)
        Dark ,   // 暗 (或 Shadow)
        Nature ,   // 自然 (木)
        Poison ,   // 毒 (或 Toxin)

        // --- 高阶/特殊元素 ---
        Chaos   ,  // 混沌
        Time   ,  // 时间
        Space ,  // 空间
    
        // --- 复合/衍生元素 (可选，也可通过逻辑计算得出) ---
        Magma, //       = Fire | Earth,     // 岩浆
        Storm, //       = Wind | Lightning, // 风暴
        Mist, //        = Water | Wind,     // 雾
        Blood, //       = Water | Dark,     // 鲜血 (示例)
    }


    /// <summary>
    /// 元素反应类型
    /// </summary>
    [Flags]
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
        Crystallize , // 结晶 (岩+其他)
        Burning ,    // 燃烧 (火+草/油)
        Conductive ,  // 传导 (雷+金属)
        ChangeMaterial = 1 << 8,  // 标记区域
        ChangeSurfaceMaterial = 1 << 9,
        ElementConsumption = 1 << 10,
        MASK = 0xFF // 前面8位用于表示反应结果，256种足够用了。后面位标记位。
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
        None = 0,

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
    /// 元素反应规则数据
    /// </summary>
    public class ReactionRule: ETObject
    {
        public ReactionType Result; // 反应结果，火烧木头为例，产生结果是持续燃烧，直到木头烧完。结果会挂在材质表面。
        public double DamageMultiplier;  // 伤害放大系数，只针对元素伤害放大, > 1 为放大， < 1 为减少伤害。
        //  反应强度，0不发生反应， 一般Intensity 设置1。
        // 攻击者不消耗元素量，反应结果是, 被攻击者元素销量 和产生的伤害都是 Min(攻击者元素量 * Intensity, 被攻击者元素销量)。
        public double Intensity; 
        public int DaceyPerSecond;    // 反应后剩余元素每秒消耗量, 0不消耗。比如火点燃了木头，后续持续消耗剩余的木头。
        public int Duration;         // 反应后附加元素的持续时间，单位毫秒。如果一直燃烧，直到结束，设置一个较大值。
        public int[] ApplyBuffs;  // 反应后附加的BUFF，比如持续伤害，持续周边AOE，持续蔓延。
        public string VfxName;       // 反应产生的视觉效果vfx名称
    }
    
    public struct CombatHitEvent
    {
        public long AttackerId;
        public long TargetId;
        public float Damage;
        public string ReactionVfxName;
    }
    
    /// 
    /// 元素反应结果数据
    /// </summary>
    public struct ReactionResult
    {
        public bool Triggered;
        public ReactionType ReactionType;
        public float DamageMultiplier;      // 伤害倍率（增幅反应）
        public float ResistanceReduction;   // 抗性削减（超导等）
        public float ControlDuration;       // 控制时长（冻结、感电麻痹等）
        public float ExtraDamage;           // 额外固定伤害（超载等）
        public ElementalType ConsumedElement; // 被消耗的已有元素
    }
    
    
    /// <summary>
    /// 元素附着信息
    /// </summary>
    public class ElementalAttachment: ETObject
    {
        public long SourceId;    // 附着来源（技能/装备/单位）
        public MaterialType Material;
        public ElementalType ElementalType;
        public ReactionType ReactionResult;
        public int Gauge;  // 元素残留数量
        public double Intensity;  // 反应强度
        public int DecayPerSecond; // 每秒消耗
        public long LastUpdateTime;
        public long EndTime;    // 结束时间
        public long ReactionEndTime; // 反应结束时间
        public ReactionRule ReactionRule; // 反应规则
    }
}
