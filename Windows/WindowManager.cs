using System.Linq;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using GposeUtils.Utils;

namespace GposeUtils.Windows;

public static class WindowManager
{
    public static MainWindow MainWindow { get; private set; } = null!;

    private static WindowSystem WindowSystem { get; set; } = new ("GPose Utilities");
    
    public static void Init(IDalamudPluginInterface dalamud)
    {
        MainWindow = dalamud.Create<MainWindow>()!;
        WindowSystem.AddWindow(MainWindow);

        Services.PluginInterface.UiBuilder.DisableGposeUiHide = true;
        Services.PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        Services.PluginInterface.UiBuilder.OpenConfigUi += OpenConfig;
        
        ActorStateWatcher.OnGPoseChange += OnGPoseChange;
    }
    private static void OpenConfig()
    {
        MainWindow.IsOpen = true;
    }

    private static void OnGPoseChange(bool isInGPose)
    {
        MainWindow.IsOpen = isInGPose;
    }

    internal static void Disposing()
    {
        Services.PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
    }
}