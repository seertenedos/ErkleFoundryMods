using C3.ModKit;

namespace LazyCrafter
{

    public static class Config
    {
        [ModSettingGroup]
        public static class General
        {
            [ModSettingTitle("Auto-Craft Amount")]
            [ModSettingDescription("How many items to craft.")]
            public static ModSetting<int> autoCraftAmount = 5;

            [ModSettingTitle("Auto-Craft Alt Amount")]
            [ModSettingDescription("How many items to craft if alt is held.")]
            public static ModSetting<int> autoCraftAltAmount = 50;

            [ModSettingTitle("Auto-Craft Only When Empty")]
            [ModSettingDescription("Don't craft if the player already has the item.")]
            public static ModSetting<bool> autoCraftOnlyWhenEmpty = false;
        }
    }

}