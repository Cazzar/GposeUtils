using System.Numerics;
using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using GposeUtils.Utils;
using ImGuiNET;

namespace GposeUtils.Windows;

public class MainWindow : PluginWindow
{
    private int? _selectedModelId = null;
    private bool _autoTarget = Plugin.Configuration.AutoTarget;
    
    protected override string WindowName => "GPose Utilities";

    public override void Toggle()
    {
        IPCUtils.IsBrioAvailable();
        base.Toggle();
    }

    protected override void Contents()
    {
        if (!IPCUtils.ShowBrioAvailable)
        {
            ImGui.TextColored(new (255, 0, 0, 255), "Brio IPC is not enabled.");
            ImGui.Text("Please enable and install Brio");
            ImGui.Text("As well as enabling IPC in");
            ImGui.Text("Settings -> Integrations");
            if (GuiHelpers.IconButton(FontAwesomeIcon.Sync))
                IPCUtils.IsBrioAvailable();

            return;
        }

        ImGui.BeginListBox("###actor_spawn_favourites", new(-1, ImGui.GetTextLineHeight() * 9));

        foreach (var (modelId, name) in Plugin.Configuration.Favourites)
        {
            if (ImGui.Selectable($"{name}###actor_model_{modelId}", modelId == _selectedModelId))
            {
                _selectedModelId = modelId;
            }
        }
        
        ImGui.EndListBox();
        
        if (GuiHelpers.IconButton(FontAwesomeIcon.Plus))
        {
            ImGui.OpenPopup("###actor_spawn_favourites_add");
        }

        ImGui.SameLine(ImGui.GetContentRegionAvail().X - GuiHelpers.CalcIconSize(FontAwesomeIcon.Trash).X);
        if (GuiHelpers.IconButtonHoldCtrlConfirm(FontAwesomeIcon.Trash, "Remove (Ctrl to confirm)"))
        {
            if (_selectedModelId != null)
            {
                Plugin.Configuration.Favourites.Remove(_selectedModelId.Value);
                Plugin.Configuration.Save();
            }
        }
        
                
        ImGui.Checkbox("Auto Target", ref _autoTarget);
        Plugin.Configuration.AutoTarget = _autoTarget;
        
        ImGui.SameLine(ImGui.GetContentRegionAvail().X  - ImGui.CalcTextSize("Spawn").X);
        if (_selectedModelId is null) ImGui.BeginDisabled();
        if (ImGui.Button("Spawn!") && _selectedModelId != null)
        {
            var gameObject = IPCUtils.SpawnWithModelId(_selectedModelId.Value);
            if (_autoTarget && gameObject is not null && Plugin.IsInGPose)
            {
                unsafe
                {
                    Services.Targets->GPoseTarget = (GameObject*)gameObject.Address;
                }
            }
        }
        ImGui.EndDisabled();
        
        
        DrawPopup();
    }

    int addInputModelId = 0;
    string addInputFavouriteName = "";
    
    private void DrawPopup()
    {
        if (ImGui.BeginPopup("###actor_spawn_favourites_add"))
        {
            ImGui.Text("Model ID");
            ImGui.SameLine();
            ImGui.InputInt("###actor_spawn_favourites_add_model_id", ref addInputModelId);
            
            ImGui.Text("Name");
            ImGui.SameLine();
            ImGui.InputText("###actor_spawn_favourites_add_name", ref addInputFavouriteName, 32);

            if (Plugin.Configuration.Favourites.ContainsKey(addInputModelId)) ImGui.BeginDisabled();
            if (ImGui.Button("Add"))
            {
                Plugin.Configuration.Favourites.Add(addInputModelId, addInputFavouriteName);
                Plugin.Configuration.Save();
                addInputModelId = 0;
                addInputFavouriteName = "";
                ImGui.CloseCurrentPopup();
            }
            
            ImGui.EndPopup();
        }
    }
}
