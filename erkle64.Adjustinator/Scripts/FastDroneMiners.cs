/*using C3;
using C3.ModKit;
using HarmonyLib;
using System;
using Unfoundry;

namespace FastDroneMiners
{

    [ModSettingGroup]
    [ModSettingIdentifier("FastDroneMiners")]
    [ModSettingTitle("Drone Miners")]
    [ModSettingServerSync]
    [ModSettingOrder(300)]
    public static class Config
    {
        [ModSettingTitle("Enable")]
        public static ModSetting<bool> enabled = false;

        [ModSettingTitle("Speed Multiplier")]
        [ModSettingDescription("Multiplies the mining speed of the drones.")]
        public static ModSetting<float> miningSpeedMultiplier = 4f;

        [ModSettingTitle("Capacity Multiplier")]
        [ModSettingDescription("Multiplies the ore capacity of the drones.")]
        public static ModSetting<float> capacityMultiplier = 2f;

        [ModSettingTitle("Charge Multiplier")]
        [ModSettingDescription("Multiplies the energy required to charge each drones.")]
        public static ModSetting<float> chargeMultiplier = 1f;
    }

    *//*[AddSystemToGameSimulation]
    public class FastDroneMinersSystem : SystemManager.System
    {
        struct OriginalValues
        {
            public long droneMiner_miningSpeed_fpm;
            public int droneMiner_itemCapacityPerDrone;
            public long droneMiner_droneCharge_fpm;
        }

        private Dictionary<ulong, OriginalValues> _originalValues = new();

        static readonly MethodInfo templateManager_botTypeSpecific_droneMiner = typeof(NativeWrapper).GetMethod("templateManager_botTypeSpecific_droneMiner", BindingFlags.NonPublic | BindingFlags.Static);

        public override void OnAddedToWorld()
        {
            if (!Config.enabled.value) return;

            _originalValues.Clear();

            foreach (var bot in ItemTemplateManager.getAllBuildableObjectTemplates().Values)
            {
                if (bot.type != BuildableObjectTemplate.BuildableObjectType.DroneMiner)
                    continue;

                _originalValues.Add(bot.id, new OriginalValues {
                    droneMiner_miningSpeed_fpm = bot.droneMiner_miningSpeed_fpm,
                    droneMiner_itemCapacityPerDrone = bot.droneMiner_itemCapacityPerDrone,
                    droneMiner_droneCharge_fpm = bot.droneMiner_droneCharge_fpm
                });

                bot.droneMiner_miningSpeed_fpm = (long)Math.Ceiling(bot.droneMiner_miningSpeed_fpm * Config.miningSpeedMultiplier.value);
                bot.droneMiner_itemCapacityPerDrone = (int)Math.Ceiling(bot.droneMiner_itemCapacityPerDrone * Config.capacityMultiplier.value);
                bot.droneMiner_droneCharge_fpm = (long)Math.Ceiling(bot.droneMiner_droneCharge_fpm * Config.chargeMultiplier.value);

                templateManager_botTypeSpecific_droneMiner.Invoke(null, new object[] { bot.id, bot.droneMiner_oreSearchRadius, bot.droneMiner_itemCapacityPerDrone, bot.droneMiner_miningSpeed_fpm, bot.droneMiner_droneCharge_fpm, bot.droneMiner_droneCount, new v3i(bot.droneMiner_dockPositionInside), new v3i(bot.droneMiner_dockPositionOutside) });
            }
        }

        public override void OnRemovedFromWorld()
        {
            if (!Config.enabled.value) return;

            foreach (var kvp in _originalValues)
            {
                var bot = ItemTemplateManager.getBuildableObjectTemplate(kvp.Key);
                bot.droneMiner_miningSpeed_fpm = kvp.Value.droneMiner_miningSpeed_fpm;
                bot.droneMiner_itemCapacityPerDrone = kvp.Value.droneMiner_itemCapacityPerDrone;
                bot.droneMiner_droneCharge_fpm = kvp.Value.droneMiner_droneCharge_fpm;

                templateManager_botTypeSpecific_droneMiner.Invoke(null, new object[] { bot.id, bot.droneMiner_oreSearchRadius, bot.droneMiner_itemCapacityPerDrone, bot.droneMiner_miningSpeed_fpm, bot.droneMiner_droneCharge_fpm, bot.droneMiner_droneCount, new v3i(bot.droneMiner_dockPositionInside), new v3i(bot.droneMiner_dockPositionOutside) });
            }
        }
    }*//*

    [HarmonyPatch]
    public static class Patch
    {
        public static LogSource log = new LogSource("FastDroneMiners");

        [HarmonyPatch(typeof(BuildableObjectTemplate), nameof(BuildableObjectTemplate.onLoad))]
        [HarmonyPrefix]
        public static void BuildableObjectTemplateOnLoad(BuildableObjectTemplate __instance)
        {
            if (__instance.identifier.StartsWith("_base_drone_miner_"))
            {
                var miningSpeedMultiplier = Config.miningSpeedMultiplier.value;
                if (miningSpeedMultiplier > 0.0f && miningSpeedMultiplier != 1.0f)
                {
                    var oldValue = Convert.ToSingle(__instance.droneMiner_miningSpeed_str, System.Globalization.CultureInfo.InvariantCulture);
                    var newValue = oldValue * miningSpeedMultiplier;
                    log.LogFormat("Changing mining speed of {0} from {1} to {2}", __instance.identifier, oldValue, newValue);
                    __instance.droneMiner_miningSpeed_str = newValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
                }

                var chargeMultiplier = Config.chargeMultiplier.value;
                if (chargeMultiplier > 0.0f && chargeMultiplier != 1.0f)
                {
                    var oldValue = Convert.ToSingle(__instance.droneMiner_droneCharge_str, System.Globalization.CultureInfo.InvariantCulture);
                    var newValue = oldValue * chargeMultiplier;
                    log.LogFormat("Changing drone charge of {0} from {1} to {2}", __instance.identifier, oldValue, newValue);
                    __instance.droneMiner_droneCharge_str = newValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
                }

                var capacityMultiplier = Config.capacityMultiplier.value;
                if (capacityMultiplier > 0.0f && capacityMultiplier != 1.0f)
                {
                    var oldValue = __instance.droneMiner_itemCapacityPerDrone;
                    var newValue = UnityEngine.Mathf.CeilToInt(oldValue * capacityMultiplier);
                    log.LogFormat("Changing drone capacity of {0} from {1} to {2}", __instance.identifier, oldValue, newValue);
                    __instance.droneMiner_itemCapacityPerDrone = newValue;
                }
            }
        }
    }

}
*/