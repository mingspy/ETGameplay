namespace ET
{
    public static class DamageHelper
    {
        /// <summary>
        /// 计算最终伤害 TODO: 细化伤害
        /// </summary>
        public static float CalculateDamage(Unit attacker, Unit target, float baseDamage)
        {
            var attackerNumeric = attacker.GetComponent<NumericComponent>();
            var targetNumeric = target.GetComponent<NumericComponent>();
            
            // 1. 基础攻击力加成
            float attack = attackerNumeric.GetAsInt(NumericType.Attack);
            
            // 2. 伤害百分比加成  
            float damageBonus = attackerNumeric.GetAsFloat(NumericType.ArmorPenetration);
            float finalDamage = (baseDamage + attack) * (1 + damageBonus);
            
            // 3. 目标防御减伤
            float defense = targetNumeric.GetAsFloat(NumericType.Armor);
            float damageReduction = defense / (defense + 100); // 经典MOBA防御公式
            finalDamage *= (1 - damageReduction);

            return finalDamage < 1 ? 1 : finalDamage;
        }
    }
}

