using System;
using System.Collections.Generic;

namespace ET
{
    [Flags]
    public enum State
    {
        Idle = 0, // 空闲
        Moving = 1 << 0, // 移动
        Attacking = 1 << 1, // 普通攻击 Atk1-N
        Casting = 1 << 2, // 释放技能 Spell0-N
        Stunned = 1 << 3, // 被击中（硬直), 无法移动，无法释放魔法
        Dead = 1 << 4, // 死亡
        Invincible = 1 << 5, // 无敌
        Silence = 1 << 6, // 沉默：无法释放魔法
        Frozen = 1 << 7,
        SuperArmor = 1 << 8, // 霸体（免疫控制）
        Enhanced = 1 << 9, // 强化状态
        IsCasting = Attacking | Casting,
        Abnormal = Stunned | Dead,
        NotCastable = Dead | Stunned | Frozen | IsCasting
    }

    /// <summary>
    ///     管理用户状态，直接单独设置
    /// </summary>
    [ComponentOf(typeof(Unit))]
    [EnableMethod]
    public class StateComponent : Entity, IAwake<int>
    {
        public State State { get; set; }
        
        /// <summary>
        /// 英雄ID
        /// </summary>
        public int HeroConfigId { get; set; }

        /// <summary>
        /// 被动技能是否已初始化
        /// </summary>
        public bool IsInited{ get; set; }

        /// <summary>
        /// 被动技能等级
        /// </summary>
        public int PassiveLevel{ get; set; }

        /// <summary>
        /// 自定义被动数据 - 用于存储各英雄被动的专属状态
        /// 例如：鲁班的被动层数、马超的冷晖枪数量、妲己的层数等
        /// Key: 数据名, Value: 数据值
        /// </summary>
        public Dictionary<string, object> passiveData;

        public Dictionary<string, object> PassiveData
        {
            get
            {
                return passiveData??= ObjectPool.Instance.Fetch<Dictionary<string, object>>();
            }
        }
        
        /// <summary>
        /// 已激活的被动技能对应的BuffID列表
        /// 英雄出生时会自动添加这些Buff
        /// </summary>
        public List<long> passiveBuffIds;

        public List<long> PassiveBuffIds
        {
            get
            {
                return passiveBuffIds??= ObjectPool.Instance.Fetch<List<long>>();
            }
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            base.Dispose();

            if (this.passiveData != null)
            {
                this.passiveData.Clear();
                ObjectPool.Instance.Recycle(this.passiveData);
                this.passiveData = null;
            }
            
            if (this.passiveBuffIds != null)
            {
                this.passiveBuffIds.Clear();
                ObjectPool.Instance.Recycle(this.passiveBuffIds);
                this.passiveBuffIds = null;
            }
        }
    }
}