# ETGameplay
test gameplay on ET
- [ET readme](ET_README.md)
- [analyse about ET](Book/00_code_readme.md)


参考项目

| 参考仓库                                                                                              | 说明                      | 简称   |
|---------------------------------------------------------------------------------------------------|-------------------------|------|
| [sjai013/unity-gameplay-ability-system](https://github.com/sjai013/unity-gameplay-ability-system) | 模仿GAS实现的Unity版本，断更，功能不全 | UGAS |
| [NKGMobaBasedOnET](https://github.com/wqaetly/NKGMobaBasedOnET)                                   | 基于ET实现的Moba系统           | NKG  |
| [ET](https://github.com/egametang/ET)                                                             | 本仓库基础源码                 | ET   |

## Buff系统

Buff系统，也叫GameplayEffect系统，所有技能释放的伤害/增益效果，红蓝buff，装备加成等，都可以通过Buff系统实现。
### 设计方案
首先，从Buff的持续时间属性上划分成三大类，持续性buff可以周期性触发（Period > 0)，比如王者铭文每5秒涨血50；或者自定义每10秒减攻击力20等。
```csharp
    public enum BuffDurationType
    {
        Instant, // 立即执行
        Infinite,  // 永久性的
        HasDuration  // 有持续时间
    }
    
    public enum BuffModifyType
    {
        Const, // ADD
        Pct  // percent
    }
    
    [Flags]
    public enum BuffType
    {
        None = 0,
        Gain = 1 << 1,   // 增益
        Debuff = 1 << 2, // 减益
        Control = 1 << 3, // 控制
        PersistAfterExpire = 1<< 4 // 过期后释放保留，默认为0，不保留。
    }
    
    public struct BuffData 
    {
        public BuffType buffType;
        public BuffDurationType durationType;
        public float duration;
        public float period; // Period 大于0 为周期性触发
        public int attribute; // 待修改的属性ID,或者做成enum。
        public BuffModifyType modifyType;
        public int value;
    }
```
#### Buff用法
- 一般间接Buff都是用在 攻击力，攻速，魔法等属性的间接属性上。
- 像Hp、Mana等属性，一般直接把Buff设置到这些直接属性上，当然，这类Buff一般过期时也不回退效果。


#### 数值系统
沿用ET的设计，只使用一个NumbericDict保存所有属性。直接属性(如Hp, Speed)和间接属性(如 HpAdd, HpPct等)都是当前值。

#### Buff管理
- Instant类型Buff直接修改属性值，不用把该Buff加入BuffManager
- 持续性Buff （Infinite, HasDuration)
  - Buff添加时，直接操作属性值（如果是间接属性会触发直接属性更新）。
    - 比如一个buff对 HpAdd 增加50， 那么直接进行 NumericDict[HpAdd] += 50, NumericDict的机制会触发Hp的重算, 最终 Hp 也会+ 50 
  - Buff删除时，同理直接去掉buff的增益，如上面的Buff，NumericDict[HpAdd] -= 50
  - 周期性Buff，记录所有的增益，在Buff过期后，去掉总增益。当然需要一个标记(类型)设置是否删除总增益，默认删除，但是相加血buff，不需要。
- 过期管理，采用时间触发回调方式更新，由于ET已经实现了Timer类，那么可以复用，Buff添加时，注册过期回调，到期时自动清掉。

### 参考项目做法   【TL;DR】

那么其他人是怎么实现的呢。
####  UGAS
采用属性AttributeValue + Modifier设计, 每个属性 都有一个 AttributeValue，然后把所有针对该属性的Buff(AttributeModifier)都保存起来，统一做时间管理。  

```csharp
    [Serializable]
    public struct AttributeValue
    {
        public AttributeScriptableObject Attribute; // 属性，如Hp, HpMax, Mana ..
        public float BaseValue; // 基础值
        public float CurrentValue; // 当前值
        public AttributeModifier Modifier;
    }

    [Serializable]
    public struct AttributeModifier
    {
        public float Add;
        public float Multiply;
        public float Override;

        public AttributeModifier Combine(AttributeModifier other)
        {
            other.Add += Add;
            other.Multiply += Multiply;
            other.Override = Override;
            return other;
        }
    }
    // 属性计算公式 
    attributeValue.CurrentValue = (attributeValue.BaseValue + attributeValue.Modifier.Add) * (attributeValue.Modifier.Multiply + 1);
```
- 对于Instant类型buff，立即更改BaseValue的属性。
- 对于周期性Buff，每帧都进行更新，对于所有属性做如下操作
  - Update
    - 先清空当前属性的Modifer，即，attributeValue.Modifier = default 
    - 获取当前属性的所有Modifers，累加属性值: foreach other in modifiers:  attributeValue.Modifier.combine(other)
    - 更新Buff剩余时间
    - 更新周期性buff逻辑，如果是周期性的且到达执行时间，立即应用当前Buff为一个Instant buff
    - 清理过期Buff
  - LaterUpdate
    - 按照属性公式计算当前属性值

优缺点
- 优点: 模块化设计，网络通信友好，概念清晰。
- 缺点: 使用起来稍显麻烦，很多设计过于臃肿，且很多功能都没有实现。


#### ET
ET只实现了Numeric数值系统的设计,这里的每个NumbericType相当于UGAS中的一个Attribute。
使用Dictionary<NumbericType, int> 来存储角色的所有属性。 
```csharp
    public enum NumericType
    {
		Max = 10000,
        // ...
	    Hp = 1001,
	    HpBase = Hp * 10 + 1,

	    MaxHp = 1002,
	    MaxHpBase = MaxHp * 10 + 1,
	    MaxHpAdd = MaxHp * 10 + 2,
	    MaxHpPct = MaxHp * 10 + 3,
	    MaxHpFinalAdd = MaxHp * 10 + 4,
		MaxHpFinalPct = MaxHp * 10 + 5,
        // ...
	}
    
    // 数据值计算公式    
    // 一个数值可能会多种情况影响，比如速度,加个buff可能增加速度绝对值100，也有些buff增加10%速度，所以一个值可以由5个值进行控制其最终结果
    final = (((base + add) * (100 + pct) / 100) + finalAdd) * (100 + finalPct) / 100;
```
这里的Hp我们称为直接属性，HpBase, HpAdd，HpPct, FinalAdd, FinalPct我们称为间接属性。

#### NKGMobaBasedOnET
数值系统沿用ET做法，不过使用两个Dict保存数值，分别保存 当前值Numeric和原始值OriginNumeric。

buff管理类似UGAS,每个属性有Modifier，不过只有常量和百分百两种方式
- 增加Buff时，使用OriginNumeric重算当前属性的所有buff累加结果，通过NumbericComponent更新Numeric，如果该属性是间接属性，则会触发直接属性按照公式的重算。
- 删除Buff时，机制类似增加Buff，重算属性值，通过组件触发可能的二次更新。

采用Buff Pool减少Buff构建，目前看是采用了按帧轮训机制，更新处理buff。


优缺点
- 优点: 实现的比较完整，包括可视化配置。
- 缺点: 一切皆Buff的设计并不符合个人要求，比如一些动画播放等等事件也做成buff，另外代码读起来比较难溯源，需要ET和自定义机制背景。


## 元素反应系统
elemental reaction system
参考赛达传说，增加游戏可玩性。

## 技能系统
参考NKGMobaBasedOnET 使用双端行为树管理技能，并且使用Timeline(slate)组件管理技能动画时间。  
整合传统 + 元素反应。

被动技能实现，通过事件机制，监听角色的数值变化来完成。比如鲁班的连续普攻5次，触发一次强化普攻。