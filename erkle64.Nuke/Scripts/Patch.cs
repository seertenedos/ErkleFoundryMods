using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace ErkleNuke {

    [HarmonyPatch]
    public static class Patch
    {
        [HarmonyPatch(typeof(DynamiteTriggerExplosionEvent), nameof(DynamiteTriggerExplosionEvent.processEvent))]
        [HarmonyPrefix]
        public static void DynamiteTriggerExplosionEvent_processEvent(DynamiteTriggerExplosionEvent __instance)
        {
            var character = GameRoot.getClientCharacter();
            if (character != null && __instance.characterHash != character.usernameHash)
                return;

            var nukePositions = new HashSet<Vector3Int>();
            foreach (var dynamite in character.dict_dynamiteEntities.Values)
            {
                if (ReferenceEquals(dynamite.relatedItemTemplate, null) || dynamite.relatedItemTemplate.identifier != "_erkle64_nuke")
                    continue;

                var blockCoordinates = dynamite.positionAfterLanding_fpm.toBlockCoordinates();
                nukePositions.Add(blockCoordinates);
            }

            NukeSystem.TriggerNuke(nukePositions);
        }
    }

}