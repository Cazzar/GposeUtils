using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using GposeUtils.Utils;
using ImGuiNET;

namespace GposeUtils.Windows;

public class MainWindow : Window
{
    private int? _selectedModelId = null;
    private bool _autoTarget = Plugin.Configuration.AutoTarget;
    private float _spawnScale = 1f;
    
    public MainWindow() : base("GPose Utilities", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.AlwaysAutoResize)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MaximumSize = new (2000, 5000),
            MinimumSize = new (250, 200),
        };
    }

    public override void OnOpen()
    {
        IPCUtils.IsBrioAvailable();
    }

    public override void Draw()
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

        if (!Plugin.IsInGPose) ImGui.BeginDisabled();
        
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

        ImGui.SameLine(ImGui.GetContentRegionAvail().X - 3 - GuiHelpers.CalcIconSize(FontAwesomeIcon.Trash).X);
        if (GuiHelpers.IconButtonHoldCtrlConfirm(FontAwesomeIcon.Trash, "Remove (Ctrl to confirm)"))
        {
            if (_selectedModelId != null)
            {
                Plugin.Configuration.Favourites.Remove(_selectedModelId.Value);
                Plugin.Configuration.Save();
            }
        }

        ImGui.SliderFloat("Scale", ref _spawnScale, 0f, 1f);  
                
        ImGui.Checkbox("Auto Target", ref _autoTarget);
        Plugin.Configuration.AutoTarget = _autoTarget;
        
        ImGui.SameLine(ImGui.GetContentRegionAvail().X - 7 - ImGui.CalcTextSize("Spawn").X);
        if (_selectedModelId is null) ImGui.BeginDisabled();
        if (ImGui.Button("Spawn!") && _selectedModelId != null)
        {
            var gameObject = IPCUtils.SpawnWithModelId(_selectedModelId.Value, scale: _spawnScale);
            if (_autoTarget && gameObject is not null && Plugin.IsInGPose)
            {
                unsafe
                {
                    Services.Targets->GPoseTarget = (GameObject*)gameObject.Address;
                }
            }
        }
        if (_selectedModelId is null) ImGui.EndDisabled();
        
#if ENABLE_SCENES
         if (ImGui.CollapsingHeader("Scenes"))
            DrawScenes();       
#endif
        
        if (!Plugin.IsInGPose) ImGui.EndDisabled();

        DrawPopup();
        
    }
    
    private string? _selectedName = null;
    private string _newName = "";
    private void DrawScenes()
    {
        ImGui.BeginListBox("###actor_scene", new(-1, ImGui.GetTextLineHeight() * 9));

        var i = 0;
        foreach (var (name, scene) in SceneManager.Instance.Scenes)
        {
            if (ImGui.Selectable($"{name}###actor_scene{i++}", name == _selectedName))
            {
                _selectedName = name;
            }
        }
        
        ImGui.EndListBox();

        if (_selectedName is null) ImGui.BeginDisabled();
        if (ImGui.Button("Spawn Scene"))
        {
            SceneManager.Instance.GetScene(_selectedName!)?.Load();
        }
        if (_selectedName is null) ImGui.EndDisabled();
        
        
        ImGui.InputText("###actor_scene_name", ref _newName, 32);
        if (ImGui.Button("Add Scene"))
        {
            SceneManager.Instance.AddScene(_newName);
        }
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