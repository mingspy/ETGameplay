using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace ET
{
    public static class BattleHelper
    {
        /// <summary>
        ///     获取扇形范围内敌对单位（简化实现，项目中接物理/碰撞检测）
        /// </summary>
        public static List<Unit> GetUnitsInSector(float3 center, float3 forward, float range, float angleDeg, CampType selfCamp, CampType targetCamp)
        {
            // TODO: 项目中接入物理引擎OverlapSphere + 角度判定即可，此处省略实现
            throw new NotImplementedException();
        }
    }
}