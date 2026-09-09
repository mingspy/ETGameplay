namespace ET
{
    /// <summary>
    /// 单位身上的元素附着状态
    /// 通常作为 Buff 的子组件存在，或者直接挂在 Unit 上作为临时状态
    /// 这里设计为独立组件，方便快速查询。
    /// TODO: 1.改成Buff。2.实现复杂的元素反应和残留逻辑及效果。 （目前很简单，先行测试阶段）
    /// </summary>
    [ComponentOf(typeof (Unit))]
    public class ElementalComponent : Entity, IAwake
    {
        public ElementalType ElementalType { get; set; }
        
        /// <summary>
        /// 元素计量值 (0-100)
        /// </summary>
        public float Gauge { get; set; }

        /// <summary>
        /// 剩余持续时间 (秒)
        /// </summary>
        public float Duration { get; set; }
        
    }
}

