using System.Linq;

namespace ET;

[EntitySystemOf(typeof(ElementalComponent))]
[FriendOf(typeof(ElementalComponent))]
public static partial class ElementalComponentSystem
{
    [EntitySystem]
    private static void Awake(this ElementalComponent self, MaterialType materialType)
    {
        self.Unit = self.GetParent<Unit>();

        // 默认血肉材质
        self.BaseMaterial.Material = materialType; //MaterialType.Meat;
    }

    [EntitySystem]
    private static void Destroy(this ElementalComponent self)
    {
    }

    [EntitySystem]
    private static void Update(this ElementalComponent self)
    {
        long now = TimeHelper.Now();

        if (self.SurfaceMaterial != null)
        {
            self.TickElementalReaction(self.SurfaceMaterial, now);
            if (UpdateElemental(self.SurfaceMaterial, now))
            {
                self.SurfaceMaterial = null;
            }
        }

        if (self.BaseMaterial != null)
        {
            self.TickElementalReaction(self.BaseMaterial, now);
            if (UpdateElemental(self.BaseMaterial, now))
            {
                self.BaseMaterial = null;
            }
        }

        // 更新元素持续时间和剩余量
        for (int i = self.ActiveElements.Count - 1; i >= 0; i--)
        {
            ElementalAttachment elem = self.ActiveElements[i];
            self.TickElementalReaction(elem, now);
            if (UpdateElemental(self.BaseMaterial, now))
            {
                self.ActiveElements.RemoveAt(i);
            }
        }
    }

    /// <summary>
    ///     应用元素反应结果更新，继续扩散。
    /// </summary>
    /// <param name="self"></param>
    /// <param name="elem"></param>
    /// <param name="now"></param>
    private static void TickElementalReaction(this ElementalComponent self, ElementalAttachment elem, long now)
    {
        if (elem == null || elem.ReactionResult == ReactionType.None)
        {
            return;
        }

        //
        switch (elem.ReactionResult)
        {
        }
    }

    private static bool UpdateElemental(ElementalAttachment elem, long now)
    {
        if (now > elem.ReactionEndTime)
        {
            elem.ReactionResult = ReactionType.None;
        }

        // 这里只做元素的消耗和处理，伤害由Buff自动处理。
        if (elem.DecayPerSecond > 0 && now > elem.LastUpdateTime + TimeHelper.OneSecond)
        {
            elem.Gauge -= elem.DecayPerSecond;
            elem.LastUpdateTime = now;
        }

        if ((now > elem.EndTime || elem.Gauge <= 0) && elem.ReactionResult == ReactionType.None)
        {
            return true;
        }

        return false;
    }

    public static bool TryGetElementReaction(ElementalType attacker, ElementalType target, out ReactionRule rule)
    {
        rule = null;
        int from = (int)attacker;
        int to = (int)target;
        ElementReactionConfig config = ElementReactionConfigCategory.Instance.GetAll().Values
                .FirstOrDefault(kv => kv.FromElement == from && kv.ToElement == to);
        if (config == null)
        {
            return false;
        }

        rule = new ReactionRule
        {
            Result = (ReactionType)config.Reaction,
            DamageMultiplier = config.DamageMultiplier,
            Intensity =  config.Intensity,
            DaceyPerSecond = config.DaceyPerSecond,
            Duration = config.Duration,
            ApplyBuffs = config.ApplyBuffs,
            VfxName = config.VfxName
        };
        return true;
    }

    /// <summary>
    ///     获取材质引发的特殊反应
    /// </summary>
    public static bool TryGetMaterialReaction(ElementalType attacker, MaterialType targetMaterial, out ReactionRule rule)
    {
        rule = null;
        int from = (int)attacker;
        int to = (int)targetMaterial;
        ElementMaterialReactionConfig config = ElementMaterialReactionConfigCategory.Instance.GetAll().Values
                .FirstOrDefault(kv => kv.FromElement == from && kv.ToMaterial == to);
        if (config == null)
        {
            return false;
        }

        rule = new ReactionRule
        {
            Result = (ReactionType)config.Reaction,
            DamageMultiplier = config.DamageMultiplier,
            Intensity =  config.Intensity,
            DaceyPerSecond = config.DaceyPerSecond,
            Duration = config.Duration,
            ApplyBuffs = config.ApplyBuffs,
            VfxName = config.VfxName
        };
        return true;
    }
}