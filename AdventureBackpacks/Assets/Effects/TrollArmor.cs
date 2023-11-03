using AdventureBackpacks.Extensions;

namespace AdventureBackpacks.Assets.Effects;

public class TrollArmor : EffectsBase
{
    private string _effectName = "SetEffect_TrollArmor";
    public TrollArmor(string effectName, string effectDesc) : base(effectName, effectDesc, true)
    {
    }

    public override void LoadStatusEffect()
    {
        SetStatusEffect(_effectName);
    }

    public override bool HasActiveStatusEffect(ItemDrop.ItemData item, out StatusEffect statusEffect)
    {
        SetStatusEffect(_effectName);
        return base.HasActiveStatusEffect(item, out statusEffect);
    }

    public override bool IsEffectActive(Humanoid human)
    {
        if (human is Player player)
        {
            var equippedBackpack = player.GetEquippedBackpack();
            
            if (equippedBackpack == null || !EnabledEffect.Value)
                return false;
            
            var itemData = equippedBackpack.Item;
            return IsEffectActive(itemData);
        }
        return false;
    }
}