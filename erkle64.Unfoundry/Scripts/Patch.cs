using HarmonyLib;
using System.Collections.Generic;

namespace Unfoundry
{

    public struct HandheldData
    {
        public int CurrentlySetMode { get; set; }

        public HandheldData(int currentlySetMode)
        {
            CurrentlySetMode = currentlySetMode;
        }

        internal static readonly Dictionary<ulong, HandheldData> handheldData = new Dictionary<ulong, HandheldData>();
    }

    [HarmonyPatch]
    public static class Patch
    {
        private static ulong lastSpawnedBuildableWrapperEntityId = 0;

        [HarmonyPatch(typeof(BuildEntityEvent), nameof(BuildEntityEvent.processEvent))]
        [HarmonyPrefix]
        private static void BuildEntityEvent_processEvent_prefix(BuildEntityEvent __instance)
        {
            if (__instance == null) return;
            if (!(GameRoot.getClientCharacter() is Character character)) return;
            if (__instance.characterHash != character.usernameHash) return;
            lastSpawnedBuildableWrapperEntityId = 0;
        }

        [HarmonyPatch(typeof(BuildEntityEvent), nameof(BuildEntityEvent.processEvent))]
        [HarmonyPostfix]
        private static void BuildEntityEvent_processEvent_postfix(BuildEntityEvent __instance)
        {
            if (__instance == null) return;
            if (!(GameRoot.getClientCharacter() is Character character)) return;
            if (__instance.characterHash != character.usernameHash) return;
            ActionManager.InvokeAndRemoveBuildEvent(__instance, lastSpawnedBuildableWrapperEntityId);
        }

        [HarmonyPatch(typeof(BuildingManager), nameof(BuildingManager.buildingManager_constructBuildableWrapper))]
        [HarmonyPostfix]
        private static void BuildingManager_buildingManager_constructBuildableWrapper(ulong __result)
        {
            lastSpawnedBuildableWrapperEntityId = __result;
        }

        [HarmonyPatch(typeof(Character.DemolishBuildingEvent), nameof(Character.DemolishBuildingEvent.processEvent))]
        [HarmonyPrefix]
        private static bool DemolishBuildingEvent_processEvent(Character.DemolishBuildingEvent __instance)
        {
            if (__instance.clientPlaceholderId == -2)
            {
                __instance.clientPlaceholderId = 0;
                BuildingManager.buildingManager_demolishBuildingEntityForDynamite(__instance.entityId);
                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(Character.RemoveTerrainEvent), nameof(Character.RemoveTerrainEvent.processEvent))]
        [HarmonyPrefix]
        private static bool RemoveTerrainEvent_processEvent(Character.RemoveTerrainEvent __instance)
        {
            if (__instance.terrainRemovalPlaceholderId == ulong.MaxValue)
            {
                __instance.terrainRemovalPlaceholderId = 0ul;

                ChunkManager.getChunkIdxAndTerrainArrayIdxFromWorldCoords(__instance.worldPos.x, __instance.worldPos.y, __instance.worldPos.z, out ulong chunkIndex, out uint blockIndex);

                byte terrainType = 0;
                ChunkManager.chunks_removeTerrainBlock(chunkIndex, blockIndex, ref terrainType);
                ChunkManager.flagChunkVisualsAsDirty(chunkIndex, true, true);
                return false;
            }

            return true;
        }
    }

}
