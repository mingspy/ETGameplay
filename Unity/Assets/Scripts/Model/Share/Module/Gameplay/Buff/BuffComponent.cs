using System.Collections.Generic;

namespace ET
{
    /// <summary>
    ///     负责buff管理，包括应用，移除和更新。
    ///     TODO: 目前实现的Buff只能实现修改BUFF拥有者的属性，还不支持被动和AOE和继续传导，用被动天赋组件来实现，挂一个buff，只做标记，被动组件监控BUFF状态，实现被动效果。
    ///     TODO: 目前吸血被动，直接在攻击触发时实现，后续挪出来，解耦。
    ///     TODO: 按照BUFF类型管理管理BUFF。
    ///     TODO: 支持非Config类型BUFF，比如技能添加一个BUFF，直接设置BUFF效果和时间。
    ///     TODO: 添加BUFF工厂
    /// </summary>
    [EnableMethod]
    [ComponentOf(typeof(Unit))]
    public class BuffComponent : Entity, IAwake, IUpdate
    {
        #region BuffNode 管理

        // 所有Buff节点字典，Key: Buff实例ID，不需要，直接操作 this.Children
        //public Dictionary<long, EntityRef<BuffNode>> Buffs = new Dictionary<long, EntityRef<BuffNode>>();

        private SortedDictionary<int, List<long>> buffsByConfigId;

        /// <summary>
        ///     按配置ID索引的Buff列表，用于快速查找同类型Buff
        ///     Key: BuffConfigId, Value: BuffNode列表
        /// </summary>
        public SortedDictionary<int, List<long>> BuffsByConfigId
        {
            get
            {
                return this.buffsByConfigId ??= ObjectPool.Instance.Fetch<SortedDictionary<int, List<long>>>();
            }
        }

        private SortedDictionary<string, List<long>> buffsByTag;

        /// <summary>
        ///     按标签索引的Buff列表，用于驱散
        ///     Key: Tag名称, Value: BuffNode列表
        /// </summary>
        public SortedDictionary<string, List<long>> BuffsByTag
        {
            get
            {
                return this.buffsByTag ??= ObjectPool.Instance.Fetch<SortedDictionary<string, List<long>>>();
            }
        }

        private List<long> updateBuffs;

        /// <summary>
        ///     需要每帧更新的Buff列表（Duration、IntervalTick类型）
        ///     轮询驱动只遍历这个列表，性能更好
        /// </summary>
        public List<long> UpdateBuffs
        {
            get
            {
                return this.updateBuffs ??= ObjectPool.Instance.Fetch<List<long>>();
            }
        }

        private SortedDictionary<DamagePipelineEvent, List<long>> eventBuffs;

        /// <summary>
        ///     监听事件的Buff字典
        ///     Key: 事件类型, Value: 监听该事件的Buff列表
        ///     事件驱动时直接查找这个字典，不需要遍历所有Buff
        /// </summary>
        public SortedDictionary<DamagePipelineEvent, List<long>> EventBuffs
        {
            get
            {
                return this.eventBuffs ??= ObjectPool.Instance.Fetch<SortedDictionary<DamagePipelineEvent, List<long>>>();
            }
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            base.Dispose();

            if (this.buffsByConfigId != null)
            {
                this.buffsByConfigId.Clear();
                ObjectPool.Instance.Recycle(this.buffsByConfigId);
                this.buffsByConfigId = null;
            }

            if (this.buffsByTag != null)
            {
                this.buffsByTag.Clear();
                ObjectPool.Instance.Recycle(this.buffsByTag);
                this.buffsByTag = null;
            }

            if (this.updateBuffs != null)
            {
                this.updateBuffs.Clear();
                ObjectPool.Instance.Recycle(this.updateBuffs);
                this.updateBuffs = null;
            }

            if (this.eventBuffs != null)
            {
                this.eventBuffs.Clear();
                ObjectPool.Instance.Recycle(this.eventBuffs);
                this.eventBuffs = null;
            }
        }

        #endregion
    }
}