namespace ET
{
    [EntitySystemOf(typeof(StateComponent))]
    [FriendOf(typeof(StateComponent))]
    public static partial class StateComponentSystem
    {
        [EntitySystem]
        private static void Awake(this StateComponent self, int heroId)
        {
            self.HeroConfigId = heroId;
            self.State = State.Idle;
            self.PassiveBuffIds.Clear();
            self.IsInited = false;
            self.PassiveLevel = 1;
            self.PassiveData.Clear();
            self.InitHeroPassive(heroId);
        }

        public static void Freeze(this StateComponent self, float ms, long casterId)
        {
            self.FreezeMS((int)TimeHelper.ToMS(ms), casterId);
        }

        public static void FreezeMS(this StateComponent self, int ms, long casterId)
        {
            BuffComponent buffComponent = self.GetParent<Unit>().GetComponent<BuffComponent>();
            buffComponent.AddBuff(casterId, BuffType.Frozen, ms);
            EventSystem.Instance.Publish(self.Scene(), new FrozenEvent { Target = self.GetParent<Unit>(), DurationMS = ms });
        }

        public static void Unfreeze(this StateComponent self)
        {
            BuffComponent buffComponent = self.GetParent<Unit>().GetComponent<BuffComponent>();
            buffComponent.DispelByTag(nameof(BuffType.Frozen));
            self.RemoveState(State.Frozen);
        }

        public static void Stun(this StateComponent self, float ms, long casterId)
        {
            self.StunMS((int)TimeHelper.ToMS(ms), casterId);
        }

        public static void StunMS(this StateComponent self, int ms, long casterId)
        {
            BuffComponent buffComponent = self.GetParent<Unit>().GetComponent<BuffComponent>();
            buffComponent.AddBuff(casterId, BuffType.Stunned, ms);
            EventSystem.Instance.Publish(self.Scene(), new StunnedEvent { Target = self.GetParent<Unit>(), DurationMS = ms });
        }

        public static void Unstun(this StateComponent self)
        {
            BuffComponent buffComponent = self.GetParent<Unit>().GetComponent<BuffComponent>();
            buffComponent.DispelByTag(nameof(BuffType.Stunned));
            self.RemoveState(State.Stunned);
        }

        public static void AddState(this StateComponent self, State state)
        {
            self.State |= state;
        }

        public static void RemoveState(this StateComponent self, State state)
        {
            self.State &= ~state;
        }

        public static bool HasState(this StateComponent self, State state)
        {
            return (self.State & state) == state;
        }

        public static void ClearState(this StateComponent self)
        {
            self.State = State.Idle;
        }

        public static bool CanCast(this StateComponent self)
        {
            return (self.State & State.NotCastable) == 0;
        }

        #region Passive Management

        /// <summary>
        ///     初始化英雄被动
        ///     英雄创建时调用，根据英雄ID添加对应的被动Buff
        /// </summary>
        private static void InitHeroPassive(this StateComponent self, int heroConfigId)
        {
            Unit owner = self.GetParent<Unit>();
            self.HeroConfigId = heroConfigId;
            self.PassiveBuffIds.Clear();

            // 根据英雄ID配置对应的被动Buff
            switch (heroConfigId)
            {
                case HeroId.Lubanqihao:
                    self.PassiveBuffIds.Add(BuffConfigId.LubanPassive);
                    break;
                case HeroId.Machao:
                    self.PassiveBuffIds.Add(BuffConfigId.MachaoPassive);
                    break;
                case HeroId.Daji:
                    self.PassiveBuffIds.Add(BuffConfigId.DajiPassive);
                    break;
            }

            // 初始化被动自定义数据
            self.InitPassiveData(heroConfigId);

            // 添加被动Buff - 永久持续（duration=0）
            BuffComponent buffComp = owner.GetComponent<BuffComponent>();
            if (buffComp != null)
            {
                foreach (int buffId in self.PassiveBuffIds)
                {
                    buffComp.AddBuff(owner.Id, buffId, 0, BuffSourceType.HeroPassive);
                }
            }

            self.IsInited = true;
            Log.Info($"[Passive] {owner.Id} 被动技能已激活");
        }

        /// <summary>
        ///     初始化各英雄被动专属数据
        /// </summary>
        private static void InitPassiveData(this StateComponent self, int heroId)
        {
            self.PassiveData.Clear();

            switch (heroId)
            {
                case HeroId.Lubanqihao:
                    // 鲁班被动：连续普攻4次获得扫射强化
                    self.PassiveData["luban_normal_attack_count"] = 0;
                    self.PassiveData["luban_enhanced_ready"] = false;
                    self.PassiveData["luban_enhanced_buff_id"] = 0;
                    break;

                case HeroId.Machao:
                    // 马超被动：拾取冷晖枪后获得强化普攻和移速加成
                    self.PassiveData["machao_spear_count"] = 0; // 当前拾取的枪数
                    self.PassiveData["machao_movespeed_buff"] = 0;
                    self.PassiveData["machao_enhanced_ready"] = false;
                    break;

                case HeroId.Daji:
                    // 妲己被动：技能命中敌人减少法抗，最多3层
                    self.PassiveData["daji_current_stack"] = 0;
                    break;
            }
        }

        /// <summary>
        ///     获取被动数据值
        /// </summary>
        public static T GetPassiveData<T>(this StateComponent self, string key)
        {
            if (self.PassiveData.TryGetValue(key, out object value))
            {
                return (T)value;
            }

            return default;
        }

        /// <summary>
        ///     设置被动数据值
        /// </summary>
        public static void SetPassiveData(this StateComponent self, string key, object value)
        {
            self.PassiveData[key] = value;
        }

        #endregion
    }
}