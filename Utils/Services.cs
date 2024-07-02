using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using GposeUtils.Windows;

namespace GposeUtils.Utils;

public class Services
{
    [PluginService]
    internal static ICommandManager CommandManager { get; private set; } = null!;

    [PluginService]
    internal static IChatGui ChatGui { get; private set; } = null!;

    [PluginService]
    internal static IClientState ClientState { get; private set; } = null!;
    
    [PluginService]
    internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    
    [PluginService]
    internal static IPluginLog Log { get; private set; } = null!;
    
    [PluginService]
    internal static IFramework Framework { get; private set; } = null!;

    internal static readonly unsafe TargetSystem* Targets = TargetSystem.Instance();
    
    internal static void Init(IDalamudPluginInterface pi)
    {
        pi.Create<Services>();
    }
}