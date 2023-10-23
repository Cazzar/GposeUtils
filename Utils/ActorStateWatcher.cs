using Dalamud.Plugin.Services;

namespace GposeUtils.Utils;

public class ActorStateWatcher
{
    public delegate void GPoseChange(bool isInGpose);

    public static GPoseChange? OnGPoseChange = null;

    private static bool _wasInGPose;
    
    public static void Dispose() {
        Services.Framework.Update -= Monitor;
        if (Plugin.IsInGPose)
            OnGPoseChange?.Invoke(false);
    }

    public static void Init() {
        Services.Framework.Update += Monitor;
    }

    private static void Monitor(IFramework framework)
    {
        if (_wasInGPose == Plugin.IsInGPose) return;
        
        _wasInGPose = Plugin.IsInGPose;
        OnGPoseChange?.Invoke(Plugin.IsInGPose);
    }
}
