using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Common.Math;

namespace GposeUtils.Windows;

internal class GuiHelpers
{
    //Some of these are taken from Ktisis: https://github.com/ktisis-tools/Ktisis/blob/main/Ktisis/Util/GuiHelpers.cs
    
    public static bool IconButtonHoldConfirm(FontAwesomeIcon icon, string tooltip, bool isHoldingKey, Vector2 size = default, string hiddenLabel = "") {
        if (!isHoldingKey) ImGui.PushStyleVar(ImGuiStyleVar.Alpha, ImGui.GetStyle().DisabledAlpha);
        bool accepting = IconButton(icon, size, hiddenLabel);
        if (!isHoldingKey) ImGui.PopStyleVar();

        Tooltip(tooltip);

        return accepting && isHoldingKey;
    }
		
    public static bool IconButtonHoldConfirm(FontAwesomeIcon icon, string tooltip, Vector2 size = default, string hiddenLabel = "") =>
        IconButtonHoldConfirm(icon, tooltip, ImGui.GetIO().KeyCtrl && ImGui.GetIO().KeyShift, size, hiddenLabel);
		
    public static bool IconButtonHoldCtrlConfirm(FontAwesomeIcon icon, string tooltip, Vector2 size = default, string hiddenLabel = "") =>
        IconButtonHoldConfirm(icon, tooltip, ImGui.GetIO().KeyCtrl, size, hiddenLabel);

    public static bool IconButtonTooltip(FontAwesomeIcon icon, string tooltip, Vector2 size = default, string hiddenLabel = "") {
        bool accepting = IconButton(icon, size, hiddenLabel);
        Tooltip(tooltip);
        return accepting;
    }
    public static bool IconButton(FontAwesomeIcon icon, Vector2 size = default, string hiddenLabel = "") {
        ImGui.PushFont(UiBuilder.IconFont);
        bool accepting = ImGui.Button((icon.ToIconString() ?? "")+"##"+ hiddenLabel, size);
        ImGui.PopFont();
        return accepting;
    }
    
    public static void Tooltip(string text)
    {
        if (!ImGui.IsItemHovered()) return;
        ImGui.BeginTooltip();
        ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
        ImGui.TextUnformatted(text);
        ImGui.PopTextWrapPos();
        ImGui.EndTooltip();
    }
    
    public static Vector2 CalcIconSize(FontAwesomeIcon icon) {
        ImGui.PushFont(UiBuilder.IconFont);
        var size = ImGui.CalcTextSize(icon.ToIconString());
        ImGui.PopFont();
        return size;
    }
}
