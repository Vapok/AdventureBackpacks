using System.Linq;
using AdventureBackpacks.Assets.Factories;
using AdventureBackpacks.Extensions;
using HarmonyLib;
using JetBrains.Annotations;

namespace AdventureBackpacks.Assets.Effects;

//UpdateEquipmentStatusEffects
[HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UpdateEquipmentStatusEffects))]
public class Demister_Humanoid_UpdateEquipmentStatusEffects_Patch
{
    [UsedImplicitly]
    public static void Postfix(Humanoid __instance)
    {
        if (__instance is Player player)
        {
            var deMister = ObjectDB.instance.GetStatusEffect("Demister");
            if (deMister == null)
            {
                return;
            }
                
            EquipmentEffectCache.Reset(player);

            var equippedBackpack = Player.m_localPlayer.GetEquippedBackpack();

            if (equippedBackpack == null)
                return;
            
            var itemData = equippedBackpack.Item;
            
            itemData.TryGetBackpackItem(out var backpack);

            var shouldHaveFeatherFall = backpack.Biome == BackpackBiomes.Mistlands && itemData.m_quality == 4 ? true : false;  
            var hasFeatherFall = player.m_eqipmentStatusEffects.Contains(deMister);
            if (!hasFeatherFall && shouldHaveFeatherFall)
            {
                player.m_eqipmentStatusEffects.Add(deMister);
                player.m_seman.AddStatusEffect(deMister);
            }
        }
    }
}
