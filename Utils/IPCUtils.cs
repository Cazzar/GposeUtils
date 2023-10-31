using System;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using SharpDX;
using Character = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

namespace GposeUtils.Utils;

public class IPCUtils
{
    private static ICallGateSubscriber<GameObject?> _spawnBrio;
    private static ICallGateSubscriber<(int, int)> _brioApiVersion;

    internal unsafe delegate void ActorSpawned(Character* actor);

    internal static ActorSpawned? OnActorSpawned;
    
    public static void Init(DalamudPluginInterface pi)
    { 
        _brioApiVersion = pi.GetIpcSubscriber<(int, int)>("Brio.ApiVersion");
        _spawnBrio = pi.GetIpcSubscriber<GameObject?>("Brio.SpawnActor");
    }
    
    internal static bool ShowBrioAvailable { get; private set; }
    
    internal static bool IsBrioAvailable()
    {
        try
        {
            var (major, minor) = _brioApiVersion.InvokeFunc(); 
            return ShowBrioAvailable = major == 1;
        }
        catch (Exception)
        {
            return false;
        }
    }
    
    private static GameObject? SpawnBrioActor()
    {
        if (!Plugin.IsInGPose) return null;
        if (!IsBrioAvailable()) return null;
        
        try
        {
            return _spawnBrio.InvokeFunc();
        }
        catch (Exception e)
        {
            Services.Log.Error(e, "Failed to spawn Brio actor.");
            //IGNORED
        }

        return null;
    }

    internal static GameObject? SpawnWithModelId(int modelId, Vector3? positionDelta = null, float? scale = null, float? opacity = null)
    {
        var brioObject = SpawnBrioActor();

        if (brioObject is null) return null;
        
        unsafe
        {
            var actor = (Character*)brioObject.Address;
            actor->CharacterData.ModelCharaId = modelId;
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
}
