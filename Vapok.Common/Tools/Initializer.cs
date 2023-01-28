using ItemManager;
using Vapok.Common.Managers.LocalizationManager;
using Vapok.Common.Managers.PieceManager;
using Vapok.Common.Managers.Skill;
using Vapok.Common.Managers.StatusEffects;

namespace Vapok.Common.Tools;

public static class Initializer
{
    public static void LoadManagers()
    {
        Managers.Creature.PrefabManager.Init();
        Item.Init();
        Managers.Location.Location.Init();
        MaterialReplacer.Init();
        PiecePrefabManager.Init();
        Skill.Init();
        EffectManager.Init();
        Localizer.Init();
    }
}