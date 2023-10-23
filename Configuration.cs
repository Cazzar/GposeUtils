using System.Collections.Generic;
using Dalamud.Configuration;
using Dalamud.Plugin;
using GposeUtils.Utils;

namespace GposeUtils;

public class Configuration : IPluginConfiguration
{
    int IPluginConfiguration.Version { get; set; }

#region Saved configuration values
    public Dictionary<int, string> Favourites { get; set; } = new()
    {
        {585, "Wind-up Sun"},
        {1855, "Wind-up Moon"},
        {586, "Plush Cushion"},
        {1332, "Wind-up Leviathan"},
    };
    
    public bool AutoTarget { get; set; } = true;
#endregion
    
    public void Save()
    {
        Services.PluginInterface.SavePluginConfig(this);
    }
}