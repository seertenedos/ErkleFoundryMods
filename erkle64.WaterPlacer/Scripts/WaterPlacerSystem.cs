using C3;
using Unfoundry;
using UnityEngine;

namespace WaterPlacer
{

    [AddSystemToGameClient]
    public class WaterPlacerSystem : SystemManager.System
    {
        public static LogSource log = new LogSource("WaterPlacer");

        public WaterPlacerCHM _waterPlacerCHM = null;
        private int _waterPlacerModeIndex;

        public override void OnAddedToWorld()
        {
            _waterPlacerCHM = new WaterPlacerCHM();
            _waterPlacerModeIndex = CustomHandheldModeManager.RegisterMode(_waterPlacerCHM);
        }

        public override void OnRemovedFromWorld()
        {
            CustomHandheldModeManager.DeregisterMode(_waterPlacerCHM);
            _waterPlacerCHM = null;
        }

        [EventHandler]
        private void Update(OnUpdate evt)
        {
            var clientCharacter = GameRoot.getClientCharacter();
            if (clientCharacter == null) return;

            if (Input.GetKeyDown(Config.Input.openWaterPlacerKey.value) && InputHelpers.IsKeyboardInputAllowed)
            {
                CustomHandheldModeManager.ToggleMode(clientCharacter, _waterPlacerModeIndex);
            }
        }
    }

}
