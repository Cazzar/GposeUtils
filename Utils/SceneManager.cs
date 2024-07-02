using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.JavaScript;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace GposeUtils.Utils;

public class SceneManager
{
    public static SceneManager Instance { get; } = new();

    public Dictionary<string, Scene> Scenes { get; private set; } = new();
    private List<IntPtr> _actors = new();
    
    public void Init() 
    {
        ActorStateWatcher.OnGPoseChange += OnGPoseChange;
        unsafe
        {
            IPCUtils.OnActorSpawned += OnActorSpawned;
        }
    }

    public unsafe void AddScene(string name)
    {
        Scenes.Add(name, new ()
        {
            Actors = _actors.Select(
                s =>
                {
                    var character = (Character*)s;
                    var localCharacter = Services.ClientState.LocalPlayer!;
                    var charPos = character->GameObject.DrawObject->Object.Position;

                    var pos = (
                        x: localCharacter.Position.X - charPos.X,
                        y: localCharacter.Position.Y - charPos.Y,
                        z: localCharacter.Position.Z - charPos.Z
                    );
                    
                    Services.Log.Info("Adding {modelId} to scene at {x}, {y}, {z} with scale {Scale}", character->CharacterData.ModelCharaId, pos.x, pos.y, pos.z, character->GameObject.Scale);
                    return (character->CharacterData.ModelCharaId, pos, character->GameObject.Scale);
                }).ToList(),
            Name = name
        });
    }
    
    public Scene? GetScene(string name)
    {
        return Scenes.GetValueOrDefault(name);
    }

#region Event Handlers
    private void OnGPoseChange(bool isInGPose)
    {
        if (!isInGPose) return;
        
        _actors = new();
    }
    
    private unsafe void OnActorSpawned(Character* actor)
    {
        if (!Plugin.IsInGPose) return;
        
        _actors.Add((IntPtr) actor);
    }
#endregion
    
    public void Dispose()
    {
        ActorStateWatcher.OnGPoseChange -= OnGPoseChange;
        unsafe
        {
            IPCUtils.OnActorSpawned -= OnActorSpawned;
        }
    }

    public class Scene
    {
        public required string Name { get; init; }
        public required List<(int modelId, (float x, float y, float z) position, float scale)> Actors { get; init; }
        
        public void Load()
        {
            foreach (var (modelId, position, scale) in Actors)
            {
                Services.Log.Info("Spawning {ModelId} at {x}, {y}, {z} with scale {Scale}", modelId, position.x, position.y, position.z, scale);
                IPCUtils.SpawnWithModelId(modelId, new Vector3(position.x, position.y, position.z), scale);
            }
        }
    }
}
