using System;
using Dalamud.Bindings.ImGui;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using GposeUtils.Utils;

namespace GposeUtils;

public class ImGuiDisposableDisable : IDisposable
{
    private readonly bool _disabled;
    
    public ImGuiDisposableDisable(bool disabled)
    {
        _disabled = disabled;
        if (_disabled) ImGui.BeginDisabled();
    }
    
    public void Dispose()
    {
        if (_disabled) ImGui.EndDisabled();
    }
}
