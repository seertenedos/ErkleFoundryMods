using C3.ModKit;
using HarmonyLib;

namespace RapidPocketCrafting
{

    [ModSettingGroup, ModSettingIdentifier("Pocket Crafting")]
    [ModSettingServerSync]
    [ModSettingOrder(500)]
    public static class Config
    {
        [ModSettingTitle("Enable")]
        public static ModSetting<bool> enabled = false;

        [ModSettingTitle("Crafting Time Decrease Percentage")]
        [ModSettingDescription(
            "Percentage of crafting time to remove.",
            "Maximum: 90",
            "Negative numbers will increase crafting time")]
        public static ModSetting<float> craftingTimeDecreasePercentage = 90f;
    }

    [HarmonyPatch]
    public static class Patch
    {
        public static long GetCraftingTimeDecrease()
        {
            var value = (long)(Config.craftingTimeDecreasePercentage.value * 10000.0f);
            return value > 900000L ? 900000L : value;
        }

        [HarmonyPatch(typeof(CharacterManager), nameof(CharacterManager.Init))]
        [HarmonyPostfix]
        public static void CharacterManagerInit()
        {
            CharacterManager.characterManager_setCharacterCraftingSpeedDecrementPercentage(GetCraftingTimeDecrease());
        }

        [HarmonyPatch(typeof(CharacterManager), nameof(CharacterManager.increasePlayerCharacterCraftingSpeedByResearch))]
        [HarmonyPostfix]
        public static void CharacterManagerIncreasePlayerCharacterCraftingSpeedByResearch()
        {
            CharacterManager.characterManager_setCharacterCraftingSpeedDecrementPercentage(GetCraftingTimeDecrease());
        }
    }

}
