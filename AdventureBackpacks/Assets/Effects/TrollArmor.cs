using AdventureBackpacks.Extensions;

namespace AdventureBackpacks.Assets.Effects;

public class TrollArmor : EffectsBase
{
    private string _effectName = "SetEffect_TrollArmor";
    public TrollArmor(string effectName, string effectDesc) : base(effectName, effectDesc)
    {
    }

    public override bool HasActiveStatusEffect(ItemDrop.ItemData item, out StatusEffect statusEffect)
    {
        statusEffect = GetStatusEffect(_effectName);
        return statusEffect != null && IsEffectActive(item);
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