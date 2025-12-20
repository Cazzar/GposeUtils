using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Brio.API;
using Brio.API.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using Character = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

namespace GposeUtils.Utils;

public class IPCUtils
{
    private static SpawnActor _spawnBrio;
    private static ApiVersion _brioApiVersion;

    private static ICallGateSubscriber<(int, int)> _ktisisApiVersion;
    private static ICallGateSubscriber<bool> _ktisisRefreshActors;

    internal unsafe delegate void ActorSpawned(Character* actor);

    internal static ActorSpawned? OnActorSpawned;
    
    public static void Init(IDalamudPluginInterface pi)
    {
        _brioApiVersion = new ApiVersion(pi);
        _spawnBrio = new SpawnActor(pi);  //pi.GetIpcSubscriber<bool, bool, bool, Task<IGameObject?>>("Brio.Actor.SpawnExAsync");
        _ktisisApiVersion = pi.GetIpcSubscriber<(int, int)>("Ktisis.ApiVersion");
        _ktisisRefreshActors = pi.GetIpcSubscriber<bool>("Ktisis.RefreshActors");
    }
    
    internal static bool ShowBrioAvailable { get; private set; }
    
    internal static bool IsBrioAvailable()
    {
        if (Services.PluginInterface.InstalledPlugins.All(x => x.Name != "Brio"))
            return false;
        
        try
        {
            var (major, minor) = _brioApiVersion.Invoke(); 
            return ShowBrioAvailable = major == 3;
        }
        catch (Exception)
        {
            return false;
        }
    }
    
    internal static bool IsKtisisAvailable()
    {
        if (Services.PluginInterface.InstalledPlugins.All(x => x.Name != "Ktisis"))
            return false;
        
        try
        {
            var (major, _) = _ktisisApiVersion.InvokeFunc(); 
            return major == 1;
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

        IGameObject? result = null;
        
        try
        { 
            result = await SpawnActorAsync(false, false, false);
        }
        catch (Exception e)
        {
            Services.Log.Error(e, "Failed to spawn Brio actor.");
            return null;
            //IGNORED
        }

        try
        {
            if (IsKtisisAvailable() && !_ktisisRefreshActors.InvokeFunc())
            {
                throw new Exception();
            }
        }
        catch (Exception e)
        {
            Services.Log.Error(e, "Failed to refresh Ktisis actors.");
            //IGNORED
        }

        return result;
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

        var flags = SpawnFlags.Default;
        
        if (spawnWithCompanionSlot)
            flags |= SpawnFlags.ReserveCompanionSlot;
        
        
        return _spawnBrio.Invoke(flags/*, selectInHierarchy*/, spawnFrozen);
    }
}
