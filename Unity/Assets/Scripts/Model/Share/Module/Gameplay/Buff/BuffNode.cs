using System.Collections.Generic;
using Sirenix.Utilities;

namespace ET
{
    /// <summary>
    /// Buff节点 - 每个Buff的实例数据
    /// Model层只存放数据字段，不包含任何逻辑方法
    /// </summary>
    [EnableMethod]
    [ChildOf(typeof(BuffComponent))]
    public class BuffNode : Entity, IAwake
    {
        /// <summary>
        /// Buff配置ID
        /// </summary>
        public int ConfigId;

        /// <summary>
        /// Buff显示名称
        /// </summary>
        public string BuffName;

        /// <summary>
        /// Buff类型
        /// </summary>
        public BuffType BuffType;

        /// <summary>
        /// Buff来源类型
        /// </summary>
        public BuffSourceType SourceType;

        /// <summary>
        /// Buff来源实体ID（施放者、装备ID等）
        /// </summary>
        public long SourceId;

        /// <summary>
        /// 目标实体ID
        /// </summary>
        public long TargetId;

        /// <summary>
        /// 当前层数 - 可叠加Buff使用
        /// </summary>
        public int CurrentStack;

        /// <summary>
        /// 最大层数
        /// </summary>
        public int MaxStack = 1;

        /// <summary>
        /// 持续时间（毫秒）<br/>
        /// when BuffType == Duration, 0表示永久, <br/>
        /// when BuffType == Instant,  &lt;=0 表示立即buff
        /// </summary>
        public long Duration = -1;

        /// <summary>
        /// 已存在时间（毫秒）
        /// </summary>
        public long ElapsedTime;

        /// <summary>
        /// 间隔触发时间（毫秒），用于IntervalTick类型
        /// </summary>
        public long Interval;

        /// <summary>
        /// 上次触发间隔时间
        /// </summary>
        public long LastTickTime;

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool IsActive;

        /// <summary>
        /// 是否可以被驱散
        /// </summary>
        public bool IsDispellable;

        /// <summary>
        /// 内部冷却时间（毫秒）- 防止重复触发
        /// </summary>
        public long Cooldown;

        /// <summary>
        /// 冷却剩余时间
        /// </summary>
        public long CooldownRemaining;

        /// <summary>
        /// 属性修改器列表 - AttributeModifier类型使用
        /// Key: 属性类型, Value: 修改值（可以是绝对值或百分比）
        /// </summary>
        public Dictionary<int, int> NumericModifiers;

        /// <summary>
        /// 监听的事件类型列表 - EventTrigger类型使用
        /// </summary>
        public List<BuffEventType> ListenEvents;

        /// <summary>
        /// 自定义参数字典 - 用于特殊Buff的自定义数据
        /// 例如：鲁班扫射的层数、马超的枪数等
        /// </summary>
        public Dictionary<string, object> CustomData;

        /// <summary>
        /// 添加Buff时的时间戳
        /// </summary>
        public long CreateTime;

        /// <summary>
        /// Buff标签 - 用于分类和驱散（例如："控制"、"增益"、"减益"）
        /// </summary>
        public List<string> Tags;

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            base.Dispose();

            this.ConfigId = 0;
            this.BuffName = null;
            this.BuffType = 0;
            this.SourceType = 0;
            this.SourceId = 0;
            this.TargetId = 0;
            this.CurrentStack = 0;
            this.MaxStack = 0;
            this.Duration = 0;
            this.ElapsedTime = 0;
            this.Interval = 0;
            this.LastTickTime = 0;
            this.IsActive = false;
            this.IsDispellable = true;
            this.Cooldown = 0;
            this.CooldownRemaining = 0;
            this.CreateTime = 0;
            this.NumericModifiers?.Clear();
            this.NumericModifiers = null;
            this.ListenEvents?.Clear();
            this.ListenEvents =  null;
            this.CustomData?.Clear();
            this.CustomData = null;
            this.Tags?.Clear();
            this.Tags = null;
        }

        public bool HasType(BuffType type)
        {
            return (this.BuffType & type) == type;
        }
        
        /// <summary>
        /// 根据Buff配置ID初始化具体属性
        /// </summary>
        /// <param name="configId"></param>
        public void Init(int configId)
        {
            this.ConfigId = configId;
            //this.CustomData = new Dictionary<string, object>();
            this.MaxStack = 1;
            this.IsDispellable = true;
            this.Interval = 0;
            this.LastTickTime = 0;
            this.Cooldown = 0;
            this.ElapsedTime = 0;
            this.IsActive = true;
            this.CreateTime = TimeHelper.Now();
            this.CurrentStack = 1;
            this.CooldownRemaining = 0;
            if(configId <= 0)
            {
                return;
            }
            BuffConfig config = BuffConfigCategory.Instance.Get(configId);
            this.BuffName = config.Name;
            this.BuffType = (BuffType)config.BuffType;
            this.Duration = TimeHelper.ToMS(config.Duration);
            this.Interval = config.Interval;
            this.MaxStack = config.MaxStack;

            if (config.Tags.Length > 0)
            {
                this.Tags = new List<string>();
                this.Tags.AddRange(config.Tags);
            }
            
            if (config.Numerics.Length > 0)
            {
                this.NumericModifiers = new Dictionary<int, int>();
                for (int i = 0; i < config.Numerics.Length; i++)
                {
                    this.NumericModifiers.Add(config.Numerics[i], config.NumericValues[i]);
                    //this.NumericModifiers[config.Numerics[i]] = config.NumericValues[i];
                }
            }

            if (config.ListenEvents.Length > 0)
            {
                this.ListenEvents = new List<BuffEventType>();
                foreach (int eventType in config.ListenEvents)
                {
                    this.ListenEvents.Add((BuffEventType)eventType);
                }
            }
        }
    }
}