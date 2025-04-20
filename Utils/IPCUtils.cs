using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using Character = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

namespace GposeUtils.Utils;

public class IPCUtils
{
    private static ICallGateSubscriber<bool, bool, bool, Task<IGameObject?>> _spawnBrio;
    private static ICallGateSubscriber<(int, int)> _brioApiVersion;

    internal unsafe delegate void ActorSpawned(Character* actor);

    internal static ActorSpawned? OnActorSpawned;
    
    public static void Init(IDalamudPluginInterface pi)
    { 
        _brioApiVersion = pi.GetIpcSubscriber<(int, int)>("Brio.ApiVersion");
        _spawnBrio = pi.GetIpcSubscriber<bool, bool, bool, Task<IGameObject?>>("Brio.Actor.SpawnExAsync");
    }
    
    internal static bool ShowBrioAvailable { get; private set; }
    
    internal static bool IsBrioAvailable()
    {
        if (Services.PluginInterface.InstalledPlugins.All(x => x.Name != "Brio"))
            return false;
        
        try
        {
            var (major, minor) = _brioApiVersion.InvokeFunc(); 
            return ShowBrioAvailable = major == 2;
        }
        catch (Exception)
        {
            return false;
        }
    }
       
    private static async Task<IGameObject?> SpawnBrioActor()
    {
        if (!Plugin.IsInGPose) return null;
        if (!IsBrioAvailable()) return null;
        
        try
        {
            return await SpawnActorAsync(false, false, false);
        }
        catch (Exception e)
        {
            Services.Log.Error(e, "Failed to spawn Brio actor.");
            //IGNORED
        }

        return null;
    }

    internal static async Task<IGameObject?> SpawnWithModelId(int modelId, Vector3? positionDelta = null, float? scale = null, float? opacity = null)
    {
        var brioObject = await SpawnBrioActor();

        if (brioObject is null) return null;
        
        unsafe
        {
            var actor = (Character*)brioObject.Address;
            actor->ModelContainer.ModelCharaId = modelId;
            actor->DrawData.HideWeapons(true);
            actor->DrawData.HideHeadgear(0, true);

            var pos = actor->GameObject.Position;
            var delta = positionDelta.GetValueOrDefault(Vector3.Zero);

            pos.X -= delta.X;
            pos.Y -= delta.Y;
            pos.Z -= delta.Z;

            actor->GameObject.Scale = scale.GetValueOrDefault(1f);
            actor->Alpha = opacity.GetValueOrDefault(1f);

            //
            // actor->GameObject.Position = pos;
            // actor->GameObject.DefaultPosition = pos;
            // var cbase = (CharacterBase*) actor->GameObject.DrawObject;
            // cbase->Skeleton->PartialSkeletons->Skeleton->
                
            OnActorSpawned?.Invoke(actor);
                
            //Redraw
            actor->GameObject.DisableDraw();
            actor->GameObject.EnableDraw();
                
        }

        return brioObject;
    }

    /// <summary>
    /// Spawns an actor asynchronously.
    /// </summary>
    /// <param name="spawnWithCompanionSlot">Whether to spawn with a companion slot.</param>
    /// <param name="selectInHierarchy">Whether to select the actor in the hierarchy.</param>
    /// <param name="spawnFrozen">Whether to spawn the actor with animation speed of 0f.</param>
    /// <returns>The spawned actor, or null if the spawn failed.</returns>
    public static async Task<IGameObject?> SpawnActorAsync(bool spawnWithCompanionSlot = false, bool selectInHierarchy = false, bool spawnFrozen = false)
    {
        if (!IsBrioAvailable()) return null;

        return await _spawnBrio.InvokeFunc(spawnWithCompanionSlot, selectInHierarchy, spawnFrozen);
    }
}
