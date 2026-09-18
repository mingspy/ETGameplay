using System;

namespace ET
{
    [FriendOf(typeof(BuffFactory))]
    public static class BuffFactoryExtensions
    {
        public static IBuffRunner GetRunner(this BuffFactory self, BuffType buffType)
        {
            foreach (var runner in self.allBuffRuners.Values)
            {
                if(runner.CanHandleBuff(buffType)) return runner;
            }
            return null;
        }

        public static BuffDataBase CreateBuff(this BuffFactory self, BuffType buffType)
        {
            BuffDataBase buffData = buffType switch
            {
                BuffType.Numeric => new NumericBuff { Type = buffType },
                // ControlBuffs
                BuffType.Frozen => new ControlBuff { State = State.Frozen, Type = buffType },
                BuffType.Stunned => new ControlBuff { State = State.Stunned, Type = buffType },
                BuffType.Dead => new ControlBuff { State = State.Dead, Type = buffType },
                _ => null
            };

            return buffData;
        }
    }
}