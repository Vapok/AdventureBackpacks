using AdventureBackpacks.Extensions;
using BepInEx.Configuration;
using UnityEngine;
using Vapok.Common.Managers.Configuration;

namespace AdventureBackpacks.Assets.Effects;

public class Demister: EffectsBase
{
    private static Heightmap.Biome _previouseBiome = Heightmap.Biome.None;
    private static ConfigEntry<KeyboardShortcut> WisplightKeyToggle;
    private static ConfigEntry<bool> WisplightBiomeLogic;

    
    public Demister(string effectName, string effectDesc) : base(effectName, effectDesc)
    {
    }

    public override void ToggleEffect()
    {
        if (!Player.m_localPlayer || !ZNetScene.instance)
            return;

        var player = Player.m_localPlayer; 
        if ((player.m_currentBiome.Equals(Heightmap.Biome.Mistlands) && WisplightBiomeLogic.Value) || !WisplightBiomeLogic.Value)
        {
            if (Player.m_localPlayer.IsBackpackEquipped())
            {
                if (_previouseBiome != Heightmap.Biome.Mistlands && WisplightBiomeLogic.Value)
                {
                    SetEffectSwitch(true);
                    player.UpdateEquipmentStatusEffects();
                }
                if (ZInput.GetKeyDown(WisplightKeyToggle.Value.MainKey))
                {
                    if (IsEffectActive(Player.m_localPlayer))
                    {
                        ToggleEffectSwitch();
                        player.UpdateEquipmentStatusEffects();
                    }
                }
            }
        }
        else
        {
            if (CurrectSwitchSetting() && WisplightBiomeLogic.Value)
            {
                if (IsEffectActive(Player.m_localPlayer))
                {
                    SetEffectSwitch(false);
                    player.UpdateEquipmentStatusEffects();
                }
            }
        }
        
        _previouseBiome = player.GetCurrentBiome();
    }

    public override void LoadStatusEffect()
    {
        if (!CurrectSwitchSetting())
            return;
        
        SetStatusEffect("Demister");
    }

    public override bool HasActiveStatusEffect(Humanoid human, out StatusEffect statusEffect)
    {
        if (!CurrectSwitchSetting())
        {
            statusEffect = null;
            return false;   
        }
            
        SetStatusEffect("Demister");
        return base.HasActiveStatusEffect(human, out statusEffect);
    }

    public override void RegisterEffectConfiguration()
    {
        base.RegisterEffectConfiguration();
        
        ConfigSyncBase.UnsyncedConfig("Wisplight Client Settings", "Wisplight Effect Key Toggle", new KeyboardShortcut(KeyCode.L),
            new ConfigDescription("Hotkey to turn Wisplight on and off",
                null,
                new ConfigurationManagerAttributes { Order = 1 }),ref WisplightKeyToggle);
            
        ConfigSyncBase.UnsyncedConfig("Wisplight Client Settings", "Wisplight Biome Logic", true,
            new ConfigDescription("If enabled, the Wisplight will automatically turn on when entering Mistlands, and turn off when exiting.",
                null, new ConfigurationManagerAttributes { Order = 2 }), ref WisplightBiomeLogic);

    }
}