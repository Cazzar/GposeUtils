using Dalamud.Configuration;
using Dalamud.Plugin;

namespace GposeUtils;

public class Configuration : IPluginConfiguration
{
    int IPluginConfiguration.Version { get; set; }

#region Saved configuration values
    public string CoolText { get; set; }
#endregion

    private readonly DalamudPluginInterface _pluginInterface;

    public Configuration(DalamudPluginInterface pi)
    {
        this._pluginInterface = pi;
    }

    public void Save()
    {
        this._pluginInterface.SavePluginConfig(this);
    }
}