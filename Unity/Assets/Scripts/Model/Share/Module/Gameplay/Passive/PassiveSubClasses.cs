namespace ET
{
    // 计数型被动：只带计数相关字段
    public class CountPassiveData : PassiveDataBase
    {
        public int CurrentCount { get; set; }
        public int Threshold { get; set; }
        public bool ResetOnTrigger { get; set; }
        public int TargetBuffId { get; set; }
    }

    // 冷却型被动：只带冷却相关字段
    public class CooldownPassiveData : PassiveDataBase
    {
        public float Cooldown { get; set; }
        public long LastTriggerTime { get; set; }
        public int TargetBuffId { get; set; }
    }

    // 概率型被动：只带概率字段
    public class ProbabilityPassiveData : PassiveDataBase
    {
        public float Probability { get; set; }
        public int TargetBuffId { get; set; }
    }

    // 血量阈值型被动：只带阈值字段
    public class HpThresholdPassiveData : PassiveDataBase
    {
        public float ThresholdPercent { get; set; } // 30% 填 0.3
        public bool HasTriggered { get; set; } // 单局只触发一次
        public int TargetBuffId { get; set; }
    }

    public struct NormalAttackHitEvent : IPassiveEvent
    {
        public PassiveType TriggerType => PassiveType.OnNormalAttackHit;
    }
}