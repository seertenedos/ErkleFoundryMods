using C3.ModKit;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace OreMultiplier
{

    public static class Config
    {
        [ModSettingGroup]
        public static class General
        {
            [ModSettingTitle("Override Session Settings")]
            [ModSettingDescription(
                "Override session settings.",
                "Setting this to on will apply settings even if they are already set in the session.",
                "Even with this enabled, already generated terrain will not be affected.")]
            public static ModSetting<bool> overrideSessionSettings = false;
        }

        [ModSettingGroup]
        [ModSettingIdentifier("Ore Multiplication")]
        public static class OreMultiplication
        {
            [ModSettingTitle("Frequency Multiplier")]
            [ModSettingDescription(
                "Ore patch chance multiplication factor.",
                "Increase this to spawn more ore patches, reservoirs and veins.",
                "Large numbers will cause olumite reservoirs to fill all available positions, leaving no space for ore patches.",
                "Use reservoirChanceMultiplierOverride to prevent that from happening.")]
            public static ModSetting<float> chanceMultiplier = 2f;

            [ModSettingTitle("Richness Multiplier")]
            [ModSettingDescription(
                "Ore patch yield multiplication factor.",
                "Increase this to make ore patches and veins contain more ore per block and reservoirs contain more olumite.")]
            public static ModSetting<float> yieldMultiplier = 8f;
        }

        [ModSettingGroup]
        [ModSettingIdentifier("Reservoir Overrides")]
        public static class ReservoirOverrides
        {
            [ModSettingTitle("Reservoir Frequency Override")]
            [ModSettingDescription(
                "Override chance multiplier for olumite reservoirs.",
                "0 or negative = use chanceMultiplier.",
                "1 = disable reservoir chance multiplication.",
                "Numbers larger than 1 increase the number of olumite reservoirs on the map.",
                "Recommended to keep this below 10.")]
            public static ModSetting<float> reservoirChanceMultiplierOverride = 0f;

            [ModSettingTitle("Reservoir Richness Override")]
            [ModSettingDescription(
                "Override yield multiplier for olumite reservoirs.",
                "0 = use yieldMultiplier.",
                "1 = disable reservoir yield multiplication.",
                "Numbers larger than 1 increase the amount of olumite in each reservoir.")]
            public static ModSetting<float> reservoirYieldMultiplierOverride = 0f;
        }

        [ModSettingGroup]
        [ModSettingIdentifier("Vein Overrides")]
        public static class VeinOverrides
        {
            [ModSettingTitle("Vein Frequency Override")]
            [ModSettingDescription(
                "Override chance multiplier for ore veins.",
                "Ore veins are the large patches of underground ore unlocked later in the game.",
                "0 = use chanceMultiplier.",
                "1 = disable ore vein chance multiplication.",
                "Numbers larger than 1 increase the number of ore veins on the map.",
                "Very large numbers will cause ore veins to take all the available spaces, leaving no room for ore patches or olumite reservoirs.",
                "Recommended to keep this below 5.")]
            public static ModSetting<float> veinChanceMultiplierOverride = 1f;

            [ModSettingTitle("Vein Richness Override")]
            [ModSettingDescription(
                "Override yield multiplier for ore veins.",
                "Ore veins are the large patches of underground ore unlocked later in the game.",
                "0 = use yieldMultiplier.",
                "1 = disable ore vein yield multiplication.",
                "Numbers larger than 1 increase the amount of ore in each block for ore veins.")]
            public static ModSetting<float> veinYieldMultiplierOverride = 1f;
        }
    }

    [HarmonyPatch]
    public static class Patch
    {
        [HarmonyPatch(typeof(SessionSettingsManager), nameof(SessionSettingsManager.Init))]
        [HarmonyPrefix]
        public static void SessionSettingsManager_Init(CubeSavegame savegame)
        {
            var oreChanceFactor = Config.OreMultiplication.chanceMultiplier.value;
            var oreYieldFactor = Config.OreMultiplication.yieldMultiplier.value;

            foreach (var tbt in ItemTemplateManager.getAllTerrainTemplates().Values)
            {
                if (tbt.flags.HasFlagNonAlloc(TerrainBlockType.TerrainTypeFlags.Ore) == false)
                    continue;

                var frequencyId = SessionSettingsManager.getIdForResourceFrequency(tbt.id);
                var richnessId = SessionSettingsManager.getIdForResourceRichness(tbt.id);

                if (oreChanceFactor > 0.0f && oreChanceFactor != 1.0f)
                {
                    if (Config.General.overrideSessionSettings.value || !savegame.sessionSettings.ContainsKey(frequencyId))
                    {
                        var value = (long)Mathf.Ceil(SessionSettingsManager.ORE_DEFAULT_VALUE_FREQUENCY * oreChanceFactor);
                        savegame.sessionSettings[frequencyId] = value;
                        Debug.Log($"Changed {tbt.name} frequency to {value}");
                    }
                }

                if (oreYieldFactor > 0.0f && oreYieldFactor != 1.0f)
                {
                    if (Config.General.overrideSessionSettings.value || !savegame.sessionSettings.ContainsKey(richnessId))
                    {
                        var value = (long)Mathf.Ceil(SessionSettingsManager.ORE_DEFAULT_VALUE_RICHNESS * oreYieldFactor);
                        savegame.sessionSettings[richnessId] = value;
                        Debug.Log($"Changed {tbt.name} richness to {value}");
                    }
                }
            }

            var reservoirChanceFactor = oreChanceFactor;
            if (Config.ReservoirOverrides.reservoirChanceMultiplierOverride.value > 0.0f)
                reservoirChanceFactor = Config.ReservoirOverrides.reservoirChanceMultiplierOverride.value;

            var reservoirYieldFactor = oreYieldFactor;
            if (Config.ReservoirOverrides.reservoirYieldMultiplierOverride.value > 0.0f)
                reservoirYieldFactor = Config.ReservoirOverrides.reservoirYieldMultiplierOverride.value;

            foreach (var rt in ItemTemplateManager.getAllReservoirTemplates().Values)
            {
                var frequencyId = SessionSettingsManager.getIdForResourceFrequency(rt.id);
                var richnessId = SessionSettingsManager.getIdForResourceRichness(rt.id);

                if (reservoirChanceFactor > 0.0f && reservoirChanceFactor != 1.0f)
                {
                    if (Config.General.overrideSessionSettings.value || !savegame.sessionSettings.ContainsKey(frequencyId))
                    {
                        var value = (long)Mathf.Ceil(SessionSettingsManager.ORE_DEFAULT_VALUE_FREQUENCY * reservoirChanceFactor);
                        savegame.sessionSettings[frequencyId] = value;
                        Debug.Log($"Changed {rt.name} frequency to {value}");
                    }
                }

                if (Config.ReservoirOverrides.reservoirYieldMultiplierOverride.value > 0.0f)
                {
                    if (Config.General.overrideSessionSettings.value || !savegame.sessionSettings.ContainsKey(richnessId))
                    {
                        var value = (long)Mathf.Ceil(SessionSettingsManager.ORE_DEFAULT_VALUE_RICHNESS * reservoirYieldFactor);
                        savegame.sessionSettings[richnessId] = value;
                        Debug.Log($"Changed {rt.name} richness to {value}");
                    }
                }
            }

            var veinChanceFactor = oreChanceFactor;
            if (Config.VeinOverrides.veinChanceMultiplierOverride.value > 0.0f)
                veinChanceFactor = Config.VeinOverrides.veinChanceMultiplierOverride.value;

            var veinYieldFactor = oreYieldFactor;
            if (Config.VeinOverrides.veinYieldMultiplierOverride.value > 0.0f)
                veinYieldFactor = Config.VeinOverrides.veinYieldMultiplierOverride.value;

            foreach (var ovt in ItemTemplateManager.getOreVeinTemplateDictionary().Values)
            {
                var frequencyId = SessionSettingsManager.getIdForResourceFrequency(ovt.id);
                var richnessId = SessionSettingsManager.getIdForResourceRichness(ovt.id);

                if (veinChanceFactor > 0.0f && veinChanceFactor != 1.0f)
                {
                    if (Config.General.overrideSessionSettings.value || !savegame.sessionSettings.ContainsKey(frequencyId))
                    {
                        var value = (long)Mathf.Ceil(SessionSettingsManager.ORE_DEFAULT_VALUE_FREQUENCY * veinChanceFactor);
                        savegame.sessionSettings[frequencyId] = value;
                        Debug.Log($"Changed {ovt.name} frequency to {value}");
                    }
                }

                if (veinYieldFactor > 0.0f && veinYieldFactor != 1.0f)
                {
                    if (Config.General.overrideSessionSettings.value || !savegame.sessionSettings.ContainsKey(richnessId))
                    {
                        var value = (long)Mathf.Ceil(SessionSettingsManager.ORE_DEFAULT_VALUE_RICHNESS * veinYieldFactor);
                        savegame.sessionSettings[richnessId] = value;
                        Debug.Log($"Changed {ovt.name} richness to {value}");
                    }
                }
            }
        }

        /*[HarmonyPatch(typeof(TerrainBlockType), nameof(TerrainBlockType.onLoad))]
        [HarmonyPrefix]
        public static void TerrainBlockType_onLoad(TerrainBlockType __instance)
        {
            if (__instance.flags.HasFlagNonAlloc(TerrainBlockType.TerrainTypeFlags.Ore))
            {
                var chanceFactor = Config.OreMultiplication.chanceMultiplier.value;
                if (chanceFactor > 0.0f && chanceFactor != 1.0f)
                {
                    Debug.Log($"OreMultiplier: Multiplying {__instance.identifier} chance x{chanceFactor}");
                    __instance.oreSpawn_chancePerChunk_ground = (uint)Mathf.Ceil(__instance.oreSpawn_chancePerChunk_ground / (float)chanceFactor);
                    __instance.oreSpawn_chancePerChunk_surface = (uint)Mathf.Ceil(__instance.oreSpawn_chancePerChunk_surface / (float)chanceFactor);
                }

                var yieldFactor = Config.OreMultiplication.yieldMultiplier.value;
                if (yieldFactor > 0.0f && yieldFactor != 1.0f)
                {
                    Debug.Log($"OreMultiplier: Multiplying {__instance.identifier} yield x{yieldFactor}");
                    __instance.averageYield = (int)Mathf.Ceil(__instance.averageYield * yieldFactor);
                }
            }
            else if (__instance.flags.HasFlagNonAlloc(TerrainBlockType.TerrainTypeFlags.OreVeinMineable))
            {
                var yieldFactor = Config.OreMultiplication.yieldMultiplier.value;
                if (Config.VeinOverrides.veinYieldMultiplierOverride.value > 0.0f) yieldFactor = Config.VeinOverrides.veinYieldMultiplierOverride.value;
                if (yieldFactor > 0.0f && yieldFactor != 1.0f)
                {
                    Debug.Log($"OreMultiplier: Multiplying {__instance.identifier} yield x{yieldFactor}");
                    __instance.oreVeinMineable_averageYield = (int)Mathf.Ceil(__instance.oreVeinMineable_averageYield * yieldFactor);
                }
            }
        }

        [HarmonyPatch(typeof(ReservoirTemplate), nameof(ReservoirTemplate.onLoad))]
        [HarmonyPrefix]
        public static void ReservoirTemplate_onLoad(ReservoirTemplate __instance)
        {
            if (__instance.chancePerChunk_ground > 0)
            {
                var chanceFactor = Config.OreMultiplication.chanceMultiplier.value;
                if (Config.ReservoirOverrides.reservoirChanceMultiplierOverride.value > 0.0f) chanceFactor = Config.ReservoirOverrides.reservoirChanceMultiplierOverride.value;
                if (chanceFactor > 0.0f && chanceFactor != 1.0f)
                {
                    Debug.Log($"OreMultiplier: Multiplying {__instance.identifier} chance x{chanceFactor}");
                    __instance.chancePerChunk_ground = (uint)Mathf.Ceil(__instance.chancePerChunk_ground / (float)chanceFactor);
                    __instance.chancePerChunk_surface = (uint)Mathf.Ceil(__instance.chancePerChunk_surface / (float)chanceFactor);
                }

                var yieldFactor = Config.OreMultiplication.yieldMultiplier.value;
                if (Config.ReservoirOverrides.reservoirYieldMultiplierOverride.value > 0.0f) yieldFactor = Config.ReservoirOverrides.reservoirYieldMultiplierOverride.value;
                if (yieldFactor > 0.0f && yieldFactor != 1.0f)
                {
                    Debug.Log($"OreMultiplier: Multiplying {__instance.identifier} yield x{yieldFactor}");
                    __instance.minContent_str = (System.Convert.ToDouble(__instance.minContent_str) * yieldFactor).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    __instance.maxContent_str = (System.Convert.ToDouble(__instance.maxContent_str) * yieldFactor).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    Debug.Log($"OreMultiplier: Multiplying {__instance.minContent_str} -> {__instance.maxContent_str}");
                }
            }
        }

        [HarmonyPatch(typeof(OreVeinTemplate), nameof(OreVeinTemplate.onLoad))]
        [HarmonyPrefix]
        public static void OreVeinTemplate_onLoad(OreVeinTemplate __instance)
        {
            if (__instance.spawnChancePerOreChunk > 0)
            {
                var chanceFactor = Config.OreMultiplication.chanceMultiplier.value;
                if (Config.VeinOverrides.veinChanceMultiplierOverride.value > 0.0f) chanceFactor = Config.VeinOverrides.veinChanceMultiplierOverride.value;
                if (chanceFactor > 0.0f && chanceFactor != 1.0f)
                {
                    Debug.Log($"OreMultiplier: Multiplying {__instance.identifier} chance x{chanceFactor}");
                    __instance.spawnChancePerOreChunk = (uint)Mathf.Ceil(__instance.spawnChancePerOreChunk / (float)chanceFactor);
                }
            }
        }*/
    }

}
