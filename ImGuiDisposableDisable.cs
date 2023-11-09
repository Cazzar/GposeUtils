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
    }
    
    public void Dispose()
    {
        if (_disabled) ImGui.EndDisabled();
    }
}
