using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using Dalamud.Plugin;
using DalamudPluginProjectTemplate.Attributes;
using System;
using Dalamud.Plugin.Services;

namespace DalamudPluginProjectTemplate
{
    public class Plugin : IDalamudPlugin
    {
        private readonly DalamudPluginInterface _pluginInterface;
        private readonly IChatGui _chat;
        private readonly IClientState _clientState;
        private readonly IPluginLog _log;

        private readonly PluginCommandManager<Plugin> _commandManager;
        private readonly Configuration _config;
        private readonly WindowSystem _windowSystem;

        public string Name => "Your Plugin's Display Name";

        public Plugin(
            DalamudPluginInterface pi,
            ICommandManager commands,
            IChatGui chat,
            IClientState clientState,
            IPluginLog log
        ) {
            this._pluginInterface = pi;
            this._chat = chat;
            this._clientState = clientState;
            _log = log;

            // Get or create a configuration object
            this._config = (Configuration)(this._pluginInterface.GetPluginConfig() ?? this._pluginInterface.Create<Configuration>())!;

            // Initialize the UI
            this._windowSystem = new(typeof(Plugin).AssemblyQualifiedName);

            var window = this._pluginInterface.Create<PluginWindow>();
            if (window is not null)
                this._windowSystem.AddWindow(window);

            this._pluginInterface.UiBuilder.Draw += this._windowSystem.Draw;

            // Load all of our commands
            this._commandManager = new(this, commands);
        }

        [Command("/example1")]
        [HelpMessage("Example help message.")]
        public void ExampleCommand1(string command, string args)
        {
            // You may want to assign these references to private variables for convenience.
            // Keep in mind that the local player does not exist until after logging in.
            var world = this._clientState.LocalPlayer?.CurrentWorld.GameData;
            _chat.Print($"Hello, {world?.Name}!");
            _log.Information("Message sent successfully.");
        }

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            this._commandManager.Dispose();

            this._pluginInterface.SavePluginConfig(this._config);

            this._pluginInterface.UiBuilder.Draw -= this._windowSystem.Draw;
            this._windowSystem.RemoveAllWindows();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
