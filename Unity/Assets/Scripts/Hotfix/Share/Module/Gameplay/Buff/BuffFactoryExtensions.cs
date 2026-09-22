namespace ET
{
    [FriendOf(typeof(BuffFactory))]
    public static class BuffFactoryExtensions
    {
        public static IBuffRunner GetRunner(this BuffFactory self, BuffType buffType)
        {
            foreach (IBuffRunner runner in self.allBuffRuners.Values)
            {
                if (runner.CanHandleBuff(buffType))
                {
                    return runner;
                }
            }

            return null;
        }

        /// <summary>
        ///     创建Buff，目前直接构造，后续改成内容池。
        /// </summary>
        /// <param name="self"></param>
        /// <param name="buffType"></param>
        /// <returns></returns>
        public static BuffDataBase CreateBuff(this BuffFactory self, BuffType buffType)
        {
            BuffDataBase buffData = buffType switch
            {
                BuffType.Numeric => new NumericBuff { Type = buffType },
                // ControlBuffs
                BuffType.Frozen => new ControlBuff { State = State.Frozen, Type = buffType },
                BuffType.Stunned => new ControlBuff { State = State.Stunned, Type = buffType },
                BuffType.Dead => new ControlBuff { State = State.Dead, Type = buffType },
                BuffType.Invincible => new ControlBuff { State = State.Invincible, Type = buffType },
                BuffType.Silence => new ControlBuff { State = State.Silence, Type = buffType },
                BuffType.SuperArmor => new ControlBuff { State = State.SuperArmor, Type = buffType },
                BuffType.Enhanced => new ControlBuff { State = State.Enhanced, Type = buffType },
                _ => null
            };

            return buffData;
        }
    }
}