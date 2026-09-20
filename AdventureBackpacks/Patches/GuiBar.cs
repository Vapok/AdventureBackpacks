using BepInEx.Bootstrap;
using HarmonyLib;
using UnityEngine;

namespace AdventureBackpacks.Patches;

public class GuiBarPatches
{
    [HarmonyPatch(typeof(GuiBar), "Awake")]
    public static class GuiBarAwakePatch
    {
        public const string eaqsGUID = "randyknapp.mods.equipmentandquickslots";
        
        private static bool Prefix(GuiBar __instance)
        {
            if (!Chainloader.PluginInfos.ContainsKey(eaqsGUID) && __instance.name == "durability" && __instance.m_bar.sizeDelta.x != 54)
            {
                __instance.m_bar.sizeDelta = new Vector2(54, 0);
            }
            return true;
        }
    }
}