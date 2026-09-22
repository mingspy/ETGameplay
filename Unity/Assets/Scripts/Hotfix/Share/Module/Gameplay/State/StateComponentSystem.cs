namespace ET
{
    [EntitySystemOf(typeof(StateComponent))]
    [FriendOf(typeof(StateComponent))]
    public static partial class StateComponentSystem
    {
        [EntitySystem]
        private static void Awake(this StateComponent self)
        {
            self.State = State.Idle;
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
            buffComponent.RemoveBuffs(BuffType.Frozen);
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
            buffComponent.RemoveBuffs(BuffType.Stunned);
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
    }
}