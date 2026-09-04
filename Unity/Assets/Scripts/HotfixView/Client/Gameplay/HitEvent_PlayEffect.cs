using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 客户端播放技能打击效果。
    /// </summary>
    [Event(SceneType.Main)]
    public class HitEvent_PlayEffect: AEvent<Scene, CombatHitEvent>
    {
        protected override async ETTask Run(Scene scene, CombatHitEvent args)
        {
            // 1. 查找目标实体对应的 GameObject
            var targetUnit = scene.GetComponent<UnitComponent>().Get(args.TargetId);
            if (targetUnit == null) return;

            var go = targetUnit.GetComponent<GameObjectComponent>()?.GameObject;
            if (go == null) return;

            // 2. 根据反应类型播放特效
            if (!string.IsNullOrEmpty(args.ReactionVfxName))
            {
                PlayVfx(go.transform.position, args.ReactionVfxName);
            }
            
            // 3. 播放命中特效
            PlayVfx(go.transform.position, "Vfx_Hit_Default");
            
            // 4. 飘字
            ShowDamageText(go.transform.position, args.Damage);
        }

        private void PlayVfx(Vector3 pos, string vfxName)
        {
            // 实际项目中使用对象池实例化 Prefab
            // var vfx = ObjectPool.Instantiate(vfxName);
            // vfx.transform.position = pos;
            Debug.Log($"Playing VFX: {vfxName} at {pos}");
        }

        private void ShowDamageText(Vector3 pos, float damage)
        {
            // UI 系统显示伤害数字
            Debug.Log($"Damage Text: {damage}");
        }
    }
    
}

