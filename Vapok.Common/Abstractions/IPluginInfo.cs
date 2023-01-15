using BepInEx;

namespace Vapok.Common.Abstractions;

public interface IPluginInfo
{
    string PluginId { get;  }
    string DisplayName { get; }
    string Version { get; }
    BaseUnityPlugin Instance { get; } 
}