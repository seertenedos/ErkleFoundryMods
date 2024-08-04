using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace ZoomButton
{

    [HarmonyPatch]
    public static class Patch
    {
        private static bool _shouldZoom = false;
        private static bool _isZoomed = false;
        private static float _previousFOV = 90.0f;
        private static float _targetFOV = 90.0f;
        private static float _zoomRate = 0.0f;

        private static FieldInfo _renderCamera = typeof(GameCamera).GetField("renderCamera", BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyPatch(typeof(GameCamera), nameof(GameCamera.Update))]
        [HarmonyPrefix]
        public static void GameCamera_Update(GameCamera __instance)
        {
            var renderCamera = (Camera)_renderCamera.GetValue(__instance);
            if (renderCamera == null) return;

            _shouldZoom = Input.GetKey(Config.Input.zoomKey.value) && !GlobalStateManager.checkIfCursorIsRequired();

            if (_shouldZoom)
            {
                if (!_isZoomed)
                {
                    _isZoomed = true;

                    _previousFOV = renderCamera.fieldOfView;
                    _targetFOV = _previousFOV / Config.General.zoomFactor.value;
                    _zoomRate = 0.0f;
                }

                renderCamera.fieldOfView = Mathf.SmoothDamp(renderCamera.fieldOfView, _targetFOV, ref _zoomRate, 0.1f, 1500.0f, Time.deltaTime);
            }
            else if (_isZoomed)
            {
                _isZoomed = false;
                renderCamera.fieldOfView = _previousFOV;
            }
        }
    }

}
