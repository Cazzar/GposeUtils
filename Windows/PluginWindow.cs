using System;
using System.Linq;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Numerics;
using System.Reflection;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace GposeUtils.Windows;

public abstract class PluginWindow
{
    public bool _isOpen = false;
    public bool IsOpen
    {
        get => _isOpen;
        set => _isOpen = value;
    }
    
    public void Toggle() => IsOpen = !IsOpen;

    protected abstract string WindowName { get; }

    public void Draw()
    {
        if (!IsOpen) return;

        ImGui.SetNextWindowSize(new(-1, -1), ImGuiCond.FirstUseEver);
        if (!ImGui.Begin(WindowName, ref _isOpen))
        {
            ImGui.End();
            return;
        }
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10, 10));

        Contents();

        ImGui.PopStyleVar();
        ImGui.End();
    }

    protected abstract void Contents();
}