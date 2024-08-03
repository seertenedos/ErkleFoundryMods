using C3;
using Unfoundry;
using UnityEngine;

namespace WaterPlacer
{

    public class WaterPlacerCHM : CustomHandheldMode
    {
        private LiquidTemplate _waterTemplate = null;
        private byte _waterIndex = 0;

        private bool _freePlaceMode = false;
        private FillMode _fillMode = FillMode.SINGLE;
        private BoxStep _boxStep = BoxStep.IDLE;
        private Vector3Int _boxFrom = Vector3Int.zero;
        private Vector3Int _boxTo = Vector3Int.zero;

        private CustomRadialMenuStateControl menuStateControl;
        private Sprite _iconFlood;
        private Sprite _iconSingle;
        private Sprite _iconBox;

        private enum FillMode
        {
            SINGLE,
            FLOOD,
            BOX
        }

        private enum BoxStep
        {
            IDLE,
            HORIZONTAL,
            VERTICAL
        }

        public WaterPlacerCHM()
        {
            _iconFlood = AssetManager.Database.LoadAssetAtPath<Sprite>("Assets/erkle64.WaterPlacer/Sprites/waterplacer-flood-fill.png");
            _iconSingle = AssetManager.Database.LoadAssetAtPath<Sprite>("Assets/erkle64.WaterPlacer/Sprites/waterplacer-single-fill.png");
            _iconBox = AssetManager.Database.LoadAssetAtPath<Sprite>("Assets/erkle64.WaterPlacer/Sprites/waterplacer-box-fill.png");
        }

        private string GetHelpText()
        {
            return $@"Press {GameRoot.getHotkeyStringFromAction("Action")} to place water.
Press {GameRoot.getHotkeyStringFromAction("Alternate Action")} to flood fill an area.
Press {GameRoot.getHotkeyStringFromAction("Toggle Build Mode")} to {(_freePlaceMode ? "disable" : "enable")} free place mode.";
        }

        public override void Enter()
        {
            if (_waterTemplate == null)
            {
                if (!AssetManager.tryGetAsset("_base_water", out _waterTemplate))
                {
                    WaterPlacerSystem.log.Log($"Failed to find water template.");
                    return;
                }

                _waterIndex = (byte)GameRoot.LiquidIdxLookupTable.getKeyByValue(_waterTemplate.id);
            }
        }

        public override void Exit()
        {
            GameRoot.setInfoText("");
        }

        public override void UpdateBehavoir()
        {
            var helpText = GetHelpText();
            var freePlaceText = _freePlaceMode ? "Free" : "Terrain";
            var modeText = string.Empty;
            switch (_fillMode)
            {
                case FillMode.SINGLE:
                    modeText = "Single Mode";
                    break;
                case FillMode.FLOOD:
                    modeText = "Flood Fill Mode";
                    break;
                case FillMode.BOX:
                    modeText = "Box Mode";
                    break;
            }

            GameRoot.setInfoText(helpText);
            TabletHelper.SetTabletTextAnalyzer($"Water Placer - {modeText} - {freePlaceText}");
            TabletHelper.SetTabletTextQuickActions(helpText);
            TabletHelper.SetTabletTextLastCopiedConfig("");

            if (_waterTemplate == null) return;

            if (GlobalStateManager.getRewiredPlayer0().GetButtonUp("Toggle Build Mode"))
            {
                _freePlaceMode = !_freePlaceMode;
            }

            var worldCellPos = Vector3Int.zero;
            var hasTarget = false;
            if (_freePlaceMode)
            {
                hasTarget = true;

                var camera = GameRoot.World.Systems.Get<CameraFrustumSystem>().getCamera();
                var ray = new Ray(camera.transform.position, camera.transform.forward);
                var targetPos = ray.GetPoint(6.5f);
                worldCellPos = Vector3Int.RoundToInt(targetPos - Vector3.one * 0.5f);
            }
            else
            {
                hasTarget = GameRoot.World.Systems.Get<RaycastHelperSystem>().raycastFromCameraToTerrain(out var worldPos, out worldCellPos);
            }

            switch (_fillMode)
            {
                case FillMode.SINGLE:
                    if (hasTarget)
                    {
                        GameRoot.pushPerFrameHighlighterBox((Vector3)worldCellPos + new Vector3(0.5f, 0.5f, 0.5f), Vector3.one, 1);
                        if (GlobalStateManager.getRewiredPlayer0().GetButtonDown("Action"))
                        {
                            GameRoot.addLockstepEvent(new SetLiquidCellEvent(worldCellPos.x, worldCellPos.y, worldCellPos.z, _waterIndex, byte.MaxValue));
                            if (Config.General.playSounds.value)
                            {
                                var audioClipArray = ResourceDB.resourceLinker.audioClip_liquidExitSound;
                                AudioManager.playUISoundEffect(audioClipArray[Random.Range(0, audioClipArray.Length)]);
                            }
                        }
                    }
                    break;

                case FillMode.FLOOD:
                    if (hasTarget)
                    {
                        GameRoot.pushPerFrameHighlighterBox((Vector3)worldCellPos + new Vector3(0.5f, 0.5f, 0.5f), Vector3.one, 1);
                        if (GlobalStateManager.getRewiredPlayer0().GetButtonDown("Action"))
                        {
                            GameRoot.addLockstepEvent(new LiquidFloodFillEvent(worldCellPos.x, worldCellPos.y, worldCellPos.z, _waterIndex, byte.MaxValue));
                            if (Config.General.playSounds.value)
                            {
                                var audioClipArray = ResourceDB.resourceLinker.audioClip_liquidExitSound;
                                AudioManager.playUISoundEffect(audioClipArray[Random.Range(0, audioClipArray.Length)]);
                            }
                        }
                    }
                    break;

                case FillMode.BOX:
                    switch (_boxStep)
                    {
                        case BoxStep.IDLE:
                            if (hasTarget)
                            {
                                _boxFrom = _boxTo = worldCellPos;
                                if (GlobalStateManager.getRewiredPlayer0().GetButtonDown("Action")) _boxStep = BoxStep.HORIZONTAL;
                            }
                            break;

                        case BoxStep.HORIZONTAL:
                            {
                                var camera = GameRoot.World.Systems.Get<CameraFrustumSystem>().getCamera();
                                var ray = new Ray(camera.transform.position, camera.transform.forward);
                                var plane = new Plane(Vector3.up, _boxFrom + Vector3.up * 0.5f);
                                if (plane.Raycast(ray, out var distance))
                                {
                                    var hitPos = ray.GetPoint(distance);
                                    _boxTo = Vector3Int.RoundToInt(hitPos - Vector3.one * 0.5f);
                                    _boxTo.y = _boxFrom.y;
                                    if (GlobalStateManager.getRewiredPlayer0().GetButtonDown("Action")) _boxStep = BoxStep.VERTICAL;
                                }
                                if (GlobalStateManager.getRewiredPlayer0().GetButtonDown("Action")) _boxStep = BoxStep.VERTICAL;
                            }
                            break;

                        case BoxStep.VERTICAL:
                            {
                                var camera = GameRoot.World.Systems.Get<CameraFrustumSystem>().getCamera();
                                var ray = new Ray(camera.transform.position, camera.transform.forward);
                                var plane = new Plane(camera.transform.forward, _boxTo + Vector3.one * 0.5f);
                                if (plane.Raycast(ray, out var distance))
                                {
                                    var hitPos = ray.GetPoint(distance);
                                    _boxTo.y = Mathf.FloorToInt(hitPos.y);
                                    if (GlobalStateManager.getRewiredPlayer0().GetButtonDown("Action"))
                                    {
                                        if (Config.General.playSounds.value)
                                        {
                                            var audioClipArray = ResourceDB.resourceLinker.audioClip_liquidExitSound;
                                            AudioManager.playUISoundEffect(audioClipArray[Random.Range(0, audioClipArray.Length)]);
                                        }
                                        _boxStep = BoxStep.IDLE;
                                        var from = Vector3Int.Min(_boxFrom, _boxTo);
                                        var to = Vector3Int.Max(_boxFrom, _boxTo);
                                        for (int z = from.z; z <= to.z; z++)
                                        {
                                            for (int x = from.x; x <= to.x; x++)
                                            {
                                                var wx = x;
                                                var wy = to.y;
                                                var wz = z;
                                                ActionManager.AddQueuedEvent(() =>
                                                {
                                                    GameRoot.addLockstepEvent(new SetLiquidCellEvent(wx, wy, wz, _waterIndex, byte.MaxValue));
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                    }
                    if (hasTarget || _boxStep != BoxStep.IDLE)
                    {
                        var size = _boxTo - _boxFrom;
                        size.x = Mathf.Abs(size.x);
                        size.y = Mathf.Abs(size.y);
                        size.z = Mathf.Abs(size.z);
                        GameRoot.pushPerFrameHighlighterBox((_boxFrom + _boxTo + Vector3.one) * 0.5f, size + Vector3.one, 1);
                    }
                    break;
            }
        }

        public override bool OnRotateY() => false;

        public override void ShowMenu()
        {
            if (menuStateControl == null)
            {
                menuStateControl = new CustomRadialMenuStateControl(
                    new CustomRadialMenuOption(
                        "Single Mode", _iconSingle, "",
                        () => _fillMode = FillMode.SINGLE),

                    new CustomRadialMenuOption(
                        "Flood Fill Mode", _iconFlood, "",
                        () => _fillMode = FillMode.FLOOD),

                    new CustomRadialMenuOption(
                        "Box Mode", _iconBox, "",
                        () => _fillMode = FillMode.BOX)
                );
            }

            if (menuStateControl != null)
            {
                CustomRadialMenuSystem.Instance.ShowMenu(menuStateControl.GetMenuOptions());
            }
        }
    }

}
