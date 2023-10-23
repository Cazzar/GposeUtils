using Dalamud.IoC;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using GposeUtils.Utils;
using Lumina.Excel.GeneratedSheets;

namespace GposeUtils.Windows;

public class WindowManager
{
    public static MainWindow MainWindow { get; private set; } = null!;

    private static void Draw()
    {
        MainWindow.Draw();
    }
    
    public static void Init(DalamudPluginInterface dalamud)
    {
        MainWindow = dalamud.Create<MainWindow>()!;

        Services.PluginInterface.UiBuilder.DisableGposeUiHide = true;
        Services.PluginInterface.UiBuilder.Draw += Draw;
        
        ActorStateWatcher.OnGPoseChange += OnGPoseChange;
    }

    private static void OnGPoseChange(bool isInGPose)
    {
        MainWindow.IsOpen = isInGPose;
    }

    internal static void Disposing()
    {
        Services.PluginInterface.UiBuilder.Draw -= Draw;
    }
}