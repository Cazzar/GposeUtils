using System;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using GposeUtils.Utils;
using ImGuiNET;

namespace GposeUtils;

public class ImGuiDisposableDisable : IDisposable
{
    private readonly bool _disabled;
    
    public ImGuiDisposableDisable(bool disabled)
    {
        _disabled = disabled;
        if (_disabled) ImGui.BeginDisabled();
        Services.Log.Info("Added ImGuiDisposableDisable state {s}", _disabled);
    }
    
    public void Dispose()
    {
        if (_disabled) ImGui.EndDisabled();
        Services.Log.Info("Disposed ImGuiDisposableDisable");
    }
}
