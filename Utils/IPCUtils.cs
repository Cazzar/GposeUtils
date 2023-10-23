using System;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Character = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

namespace GposeUtils.Utils;

public class IPCUtils
{
    private static ICallGateSubscriber<GameObject?> _spawnBrio;

    public static void Init(DalamudPluginInterface pi)
    { 
        _spawnBrio = pi.GetIpcSubscriber<GameObject?>("Brio.SpawnActor");
    }

    internal static GameObject? SpawnBrioActor()
    {
        if (!Plugin.IsInGPose) return null;
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

    internal static GameObject? SpawnWithModelId(int modelId)
    {
        var brioObject = SpawnBrioActor();
        
        if (brioObject != null)
        {
            unsafe
            {
                var actor = (Character*)brioObject.Address;
                actor->CharacterData.ModelCharaId = modelId;
                actor->DrawData.HideWeapons(true);
                actor->DrawData.HideHeadgear(0, true);
                
                //Redraw
                actor->GameObject.DisableDraw();
                actor->GameObject.EnableDraw();
            }
        }

        return brioObject;
    }
}
