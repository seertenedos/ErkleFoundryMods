﻿using Unfoundry;
using UnityEngine;

namespace Duplicationer
{
    internal class BlueprintToolModeMove : BlueprintToolMode
    {
        private enum Mode
        {
            Idle,
            XPos,
            XNeg,
            YPos,
            YNeg,
            ZPos,
            ZNeg
        }

        private Mode mode = Mode.Idle;

        private float _altHeldTime = 0f;

        public BlueprintToolModeMove()
        {
        }

        public override string TabletTitle(BlueprintToolCHM tool) => "Place Blueprint - Move";

        public override void Enter(BlueprintToolCHM tool, BlueprintToolMode fromMode)
        {
            mode = Mode.Idle;
            tool.isDragArrowVisible = false;
            TabletHelper.SetTabletTextQuickActions("");
            tool.ShowBlueprint();
            tool.boxMode = BlueprintToolCHM.BoxMode.Blueprint;
        }

        public override bool AllowCopy(BlueprintToolCHM tool) => false;
        public override bool AllowPaste(BlueprintToolCHM tool) => mode == Mode.Idle;
        public override bool AllowRotate(BlueprintToolCHM tool) => mode == Mode.Idle;
        public override bool AllowMirror(BlueprintToolCHM tool) => mode == Mode.Idle;

        public override void Update(BlueprintToolCHM tool)
        {
            switch (mode)
            {
                case Mode.Idle:
                    tool.boxMode = BlueprintToolCHM.BoxMode.Blueprint;
                    var lookRay = CustomHandheldMode.GetLookRay();
                    var distance = CustomHandheldMode.BoxRayIntersection(tool.RepeatBlueprintMin, tool.RepeatBlueprintMax + Vector3Int.one, lookRay, out var normal, out var faceIndex, out var isInternal);
                    if (distance >= 0.0f)
                    {
                        var point = lookRay.GetPoint(distance);
                        tool.isDragArrowVisible = true;
                        tool.dragFaceRay = new Ray(point, normal);
                        tool.dragArrowOffset = isInternal ? -0.5f : 0.5f;
                        switch (faceIndex)
                        {
                            case 0:
                                tool.dragArrowMaterial = ResourceDB.material_glow_red;
                                SetQuickActionsIdle("X", tool.CurrentBlueprint.SizeX);
                                break;

                            case 1:
                                tool.dragArrowMaterial = ResourceDB.material_glow_red;
                                SetQuickActionsIdle("X", tool.CurrentBlueprint.SizeX);
                                break;

                            case 2:
                                tool.dragArrowMaterial = ResourceDB.material_glow_yellow;
                                SetQuickActionsIdle("Y", tool.CurrentBlueprint.SizeY);
                                break;

                            case 3:
                                tool.dragArrowMaterial = ResourceDB.material_glow_yellow;
                                SetQuickActionsIdle("Y", tool.CurrentBlueprint.SizeY);
                                break;

                            case 4:
                                tool.dragArrowMaterial = ResourceDB.material_glow_purple;
                                SetQuickActionsIdle("Z", tool.CurrentBlueprint.SizeZ);
                                break;

                            case 5:
                                tool.dragArrowMaterial = ResourceDB.material_glow_purple;
                                SetQuickActionsIdle("Z", tool.CurrentBlueprint.SizeZ);
                                break;
                        }

                        if (InputHelpers.IsMouseInputAllowed && !tool.IsAnyFrameOpen)
                        {
                            if (GlobalStateManager.getRewiredPlayer0().GetButtonDown("Action"))
                            {
                                mode = Mode.XPos + faceIndex;
                            }
                            else if (GlobalStateManager.getRewiredPlayer0().GetButtonUp("Modifier 2"))
                            {
                                if (_altHeldTime < 0.5)
                                {
                                    tool.SelectMode(tool.modeMoveSideways);
                                    AudioManager.playUISoundEffect(ResourceDB.resourceLinker.audioClip_UIButtonClick);
                                    _altHeldTime = 0f;
                                    return;
                                }
                                _altHeldTime = 0f;
                            }
                            else if (GlobalStateManager.getRewiredPlayer0().GetButton("Modifier 2"))
                            {
                                _altHeldTime += Time.unscaledDeltaTime;
                            }
                        }
                    }
                    else
                    {
                        tool.isDragArrowVisible = false;
                        SetQuickActionsIdle();
                    }
                    break;

                case Mode.XPos:
                case Mode.XNeg:
                    SetQuickActionsIdle("X", tool.CurrentBlueprint.SizeX);
                    HandleMove(tool);
                    break;

                case Mode.YPos:
                case Mode.YNeg:
                    SetQuickActionsIdle("Y", tool.CurrentBlueprint.SizeY);
                    HandleMove(tool);
                    break;

                case Mode.ZPos:
                case Mode.ZNeg:
                    SetQuickActionsIdle("Z", tool.CurrentBlueprint.SizeZ);
                    HandleMove(tool);
                    break;
            }
        }

        private void HandleMove(BlueprintToolCHM tool)
        {
            if (!GlobalStateManager.getRewiredPlayer0().GetButton("Action"))
            {
                mode = Mode.Idle;
            }
            else
            {
                float offset;
                if (CustomHandheldMode.TryGetAxialDragOffset(tool.dragFaceRay, CustomHandheldMode.GetLookRay(), out offset))
                {
                    int direction = mode - Mode.XPos;
                    int axis = direction >> 1;
                    int dragStep = InputHelpers.IsAltHeld ? tool.CurrentBlueprintSize[axis] : 1;
                    var roundedOffset = Mathf.RoundToInt(offset / dragStep) * dragStep;
                    if (Mathf.Abs(roundedOffset) >= dragStep)
                    {
                        var offsetVector = CustomHandheldMode.faceNormals[direction] * roundedOffset;
                        tool.MoveBlueprint(tool.CurrentBlueprintAnchor + offsetVector);

                        tool.dragFaceRay.origin += CustomHandheldMode.faceNormals[direction] * roundedOffset;
                    }
                }
            }
        }

        private static void SetQuickActionsIdle()
        {
            TabletHelper.SetTabletTextQuickActions($@"{GameRoot.getHotkeyStringFromAction("RotateY")}: Rotate
Tap {GameRoot.getHotkeyStringFromAction("Modifier 2")}: Move Sideways Mode");
        }

        private static void SetQuickActionsIdle(string axis, int size)
        {
            TabletHelper.SetTabletTextQuickActions($@"{GameRoot.getHotkeyStringFromAction("Action")}: Drag {axis}
Alt+{GameRoot.getHotkeyStringFromAction("Action")}: Drag {axis}*{size}
{GameRoot.getHotkeyStringFromAction("RotateY")}: Rotate
Tap {GameRoot.getHotkeyStringFromAction("Modifier 2")}: Move Sideways Mode");
        }

        private static void SetQuickActionsMoving(string axis, int size)
        {
            TabletHelper.SetTabletTextQuickActions($@"{GameRoot.getHotkeyStringFromAction("Action")}: Drag {axis}
Alt+{GameRoot.getHotkeyStringFromAction("Action")}: Drag {axis}*{size}");
        }
    }
}