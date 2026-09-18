using System.Collections.Generic;

namespace ET
{
    public class ControlBuff : BuffDataBase
    {
        public State  State { get; set; }
    }
    
    /// <summary>
    /// 属性修改器
    /// </summary>
    public class NumericBuff : BuffDataBase
    {
        /// <summary>
        ///  修改的属性
        /// </summary>
        public int[] Numerics { get; set; }
        /// <summary>
        ///  修改的属性数值，int 设置时为原始值，float设置时 要 * 10000， 比如要设置攻击加速比例为 1.5f，需要设置为 15000
        /// </summary>
        public int [] NumericValues { get; set; }

        public int[] TotalEffects { get; set; }

        protected override void PartialInit()
        {
            var config = this.BuffConfig;
            if (config != null)
            {
                this.Numerics =  config.Numerics;
                this.NumericValues = config.EffectValues;
            }
            if(this.Numerics != null) this.TotalEffects = new int[this.Numerics.Length];
        }
    }
}