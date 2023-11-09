using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using GposeUtils.Utils;
using ImGuiNET;

namespace GposeUtils.Windows;

public class MainWindow : Window
{
    private const float SpawnScaleMin = 0.001f;
    private const float OpacityMin = 0f;
    
    private int? _selectedModelId = null;
    private bool _autoTarget = Plugin.Configuration.AutoTarget;
    private float _spawnScale = 1f;
    private float _opacity = 1f;
    
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

        using var _ = new ImGuiDisposableDisable(!Plugin.IsInGPose);
        
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

        if (_spawnScale <= SpawnScaleMin) _spawnScale = SpawnScaleMin;
        ImGui.SliderFloat("Scale", ref _spawnScale, SpawnScaleMin, 1f);
        GuiHelpers.Tooltip("The scale to spawn the actor as, this has to be set before spawning.");
        
        if (_opacity <= OpacityMin) _opacity = OpacityMin;
        ImGui.SliderFloat("Opacity", ref _opacity, OpacityMin, 1f);
        GuiHelpers.Tooltip("The opacity to spawn the actor as, this has to be set before spawning.");
                
        ImGui.Checkbox("Auto Target", ref _autoTarget);
        Plugin.Configuration.AutoTarget = _autoTarget;
        
        ImGui.SameLine(ImGui.GetContentRegionAvail().X - 7 - ImGui.CalcTextSize("Spawn").X);
        using (new ImGuiDisposableDisable(_selectedModelId is null))
        {
            if (ImGui.Button("Spawn!") && _selectedModelId != null)
            {
                if (_spawnScale <= SpawnScaleMin) _spawnScale = SpawnScaleMin;

                var gameObject = IPCUtils.SpawnWithModelId(_selectedModelId.Value, scale: _spawnScale, opacity: _opacity);
                if (_autoTarget && gameObject is not null && Plugin.IsInGPose)
                {
                    unsafe
                    {
                        Services.Targets->GPoseTarget = (GameObject*)gameObject.Address;
                    }
                }
            }
        }
        
#if ENABLE_SCENES
         if (ImGui.CollapsingHeader("Scenes"))
            DrawScenes();       
#endif

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

        using (new ImGuiDisposableDisable(_selectedName is null))
        {
            if (ImGui.Button("Spawn Scene"))
            {
                SceneManager.Instance.GetScene(_selectedName!)?.Load();
            }
        }
        
        
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
        if (!ImGui.BeginPopup("###actor_spawn_favourites_add")) return;
        
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