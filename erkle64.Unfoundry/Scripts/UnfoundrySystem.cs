using C3;
using System.Collections.Generic;

namespace Unfoundry {

    [AddSystemToGameSimulation]
    public class UnfoundrySystem : SystemManager.System
    {
        static List<UnfoundryPlugin> plugins = new();

        internal static void RegisterPlugin(UnfoundryPlugin plugin)
        {
            plugins.Add(plugin);
        }

        public override void OnRemovedFromWorld()
        {
            foreach (var plugin in plugins) plugin.GameExit();
        }

        [EventHandler]
        void OnGameLoadCompleted(OnGameLoadCompleted evt)
        {
            foreach (var plugin in plugins) plugin.GameEnter();
        }
    }

}