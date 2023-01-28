using AdventureBackpacks.Assets.Factories;
using AdventureBackpacks.Extensions;
using HarmonyLib;

namespace AdventureBackpacks.Assets.Effects;

public static class Waterproof
{
    public static int AddingStatusFromEnv;

    //public void UpdateEnvStatusEffects(float dt)
    [HarmonyPatch(typeof(Player), nameof(Player.UpdateEnvStatusEffects))]
    public static class Waterproof_Player_UpdateEnvStatusEffects_Patch
    {
        public static bool Prefix()
        {
            AddingStatusFromEnv++;
            return true;
        }

        public static void Postfix(Player __instance)
        {
            AddingStatusFromEnv--;
        }
    }

    [HarmonyPatch(typeof(SEMan), nameof(SEMan.AddStatusEffect), typeof(string), typeof(bool), typeof(int), typeof(float))]
    public static class Waterproof_SEMan_AddStatusEffect_Patch
    {
        public static bool Prefix(SEMan __instance, string name)
        {
            if (AddingStatusFromEnv > 0 && __instance.m_character.IsPlayer() && name == "Wet")
            {
                var player = (Player) __instance.m_character;
                
                var equippedBackpack = player.GetEquippedBackpack();

                if (equippedBackpack == null)
                    return true;
            
                var itemData = equippedBackpack.Item;
            
                itemData.TryGetBackpackItem(out var backpack);

                var shouldBeWaterProof = backpack.Biome == BackpackBiomes.Swamp && itemData.m_quality > 2 ? true : false;  

                if (shouldBeWaterProof)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
