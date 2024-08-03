using C3;
using HarmonyLib;
using UnityEngine;

namespace FoundryCommands
{

    [HarmonyPatch]
    public static class Patch
    {
        private static Vector3 scale_wall_x = Vector3.one;
        private static Vector3 scale_wall_z = Vector3.one;
        private static Vector3 scale_slope = Vector3.one;
        private static Material material_scaled = null;

        [HarmonyPatch(typeof(BuildingModeHelpers.DragModeWorkingData), nameof(BuildingModeHelpers.DragModeWorkingData.init))]
        [HarmonyPostfix]
        public static void DragModeWorkingData_init(BuildingModeHelpers.DragModeWorkingData __instance)
        {
            var maxDragBuffer = Config.maxDragBuffer.value;
            if (maxDragBuffer > 1023 * 2)
            {
                var maxDragBufferCount = Mathf.CeilToInt(maxDragBuffer / 1023.0f);
                var maxDragBufferSize = maxDragBufferCount * 1023;
                __instance.dragPositions = new Vector3Int[maxDragBufferSize];
                __instance.dragValidationArray = new bool[maxDragBufferSize];
                __instance.dmrc_dragMatrices_green = new DrawMeshRenderingContainer(maxDragBufferCount, false);
                __instance.dmrc_dragMatrices_red = new DrawMeshRenderingContainer(maxDragBufferCount, false);
            }
        }

        [HarmonyPatch(typeof(DragHelperGO), "Awake")]
        [HarmonyPostfix]
        public static void DragHelperGO_Awake(DragHelperGO __instance)
        {
            scale_wall_x = __instance.go_wall_x.transform.localScale;
            scale_wall_z = __instance.go_wall_z.transform.localScale;
            scale_slope = __instance.go_slope.transform.localScale;
            material_scaled = __instance.go_wall_x.GetComponent<MeshRenderer>().sharedMaterial;
        }

        [HarmonyPatch(typeof(DragHelperGO), nameof(DragHelperGO.setMode))]
        [HarmonyPrefix]
        public static void DragHelperGO_setMode(ref float dragPlanScaleModifier)
        {
            var scaleModifier = (Config.dragRange.value - 0.5f) / 37.5f;
            if (dragPlanScaleModifier < scaleModifier) dragPlanScaleModifier = scaleModifier;
        }

        [HarmonyPatch(typeof(DragHelperGO), nameof(DragHelperGO.setMode))]
        [HarmonyPostfix]
        public static void DragHelperGO_setMode(DragHelperGO __instance, BuildableObjectTemplate bot, BuildableObjectTemplate.DragBuildType dragBuildType, float dragPlanScaleModifier)
        {
            __instance.go_wall_x.transform.localScale = new Vector3(scale_wall_x.x * dragPlanScaleModifier, scale_wall_x.y * dragPlanScaleModifier, scale_wall_x.z);
            __instance.go_wall_z.transform.localScale = new Vector3(scale_wall_z.x, scale_wall_z.y * dragPlanScaleModifier, scale_wall_z.z * dragPlanScaleModifier);
            __instance.go_slope.transform.localScale = new Vector3(scale_slope.x * dragPlanScaleModifier, scale_slope.y, scale_slope.z * dragPlanScaleModifier);
            __instance.collider_wall_x.size = new Vector3(scale_wall_x.x * dragPlanScaleModifier + 0.5f, scale_wall_x.y * dragPlanScaleModifier + 0.5f, scale_wall_x.z);
            __instance.collider_wall_z.size = new Vector3(scale_wall_z.x, scale_wall_z.y * dragPlanScaleModifier + 0.5f, scale_wall_z.z * dragPlanScaleModifier + 0.5f);

            if (material_scaled != null)
            {
                material_scaled.SetTextureScale("_TextureY", new Vector2(scale_wall_x.x * dragPlanScaleModifier, scale_wall_x.y * dragPlanScaleModifier));
            }
        }

        [HarmonyPatch(typeof(ChatFrame), nameof(ChatFrame.onReturnCB))]
        [HarmonyPrefix]
        public static bool ChatFrame_onReturnCB()
        {
            Character clientCharacter = GameRoot.getClientCharacter();
            if (clientCharacter == null) return true;

            try
            {
                var message = ChatFrame.getMessage();

                return !FoundryCommandsSystem.Instance.TryProcessCommand(message, clientCharacter);
            }
            catch (System.Exception e)
            {
                ChatFrame.addMessage(e.ToString(), 0);
            }

            return true;
        }
    }

}