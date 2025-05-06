using HarmonyLib;

namespace BlastProofBuildings
{

    [HarmonyPatch]
    public static class Patch
    {
        [HarmonyPatch(typeof(BuildableObjectTemplate), nameof(BuildableObjectTemplate.onLoad))]
        [HarmonyPrefix]
        public static void BuildableObjectTemplate_onLoad(BuildableObjectTemplate __instance)
        {
            if (__instance.type == BuildableObjectTemplate.BuildableObjectType.WorldDecorMineAble)
                return;

            __instance.canBeDestroyedByDynamite = false;
        }
    }

}