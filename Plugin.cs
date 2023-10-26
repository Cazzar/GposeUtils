using Dalamud.Plugin;
using System;
using GposeUtils.Utils;
using GposeUtils.Windows;

namespace GposeUtils;

public class Plugin : IDalamudPlugin
{
    public static Configuration Configuration { get; private set; } = null!;
    private readonly DalamudPluginInterface _pluginInterface;

    public static bool IsInGPose => Services.ClientState.IsGPosing;
    
    public string Name => "GPose Utilities";
    public string CommandName => "/gposeutils";

    public Plugin(
        DalamudPluginInterface pi
    ) {
        Services.Init(pi);
        _pluginInterface = pi;
        IPCUtils.Init(pi);

        Configuration = pi.GetPluginConfig() as Configuration ?? pi.Create<Configuration>()!;

        InitCommands();
        ActorStateWatcher.Init();
#if ENABLE_SCENES
        SceneManager.Instance.Init();
#endif
        WindowManager.Init(pi);
    }

    private void InitCommands()
    {
        Services.CommandManager.AddHandler(CommandName, new(HandleCommand) { HelpMessage = "Open the GPose Utilities window." });
    }

    private void HandleCommand(string command, string arguments)
    {
        WindowManager.MainWindow.Toggle();
    }
    
#region IDisposable Support
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing) return;

        _pluginInterface.SavePluginConfig(Configuration);
        ActorStateWatcher.Dispose();
#if ENABLE_SCENES
        SceneManager.Instance.Dispose();
#endif
        WindowManager.Disposing();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
#endregion
}