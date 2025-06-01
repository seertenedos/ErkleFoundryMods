using C3.ModKit;

namespace FreeStuff
{

    public static class Config
    {
        [ModSettingGroup]
        [ModSettingTitle("Creative Chest")]
        public static class CreativeChest
        {
            [ModSettingTitle("Rate")]
            [ModSettingDescription("Items per minute output for creative chest.")]
            public static ModSetting<int> rate = 1280;
        }

        [ModSettingGroup]
        [ModSettingTitle("Void Chest")]
        public static class VoidChest
        {
            [ModSettingTitle("Rate")]
            [ModSettingDescription("Items per minute input for void chest.")]
            public static ModSetting<int> rate = 1280;
        }

        [ModSettingGroup]
        [ModSettingTitle("Creative Tank")]
        public static class CreativeTank
        {
            [ModSettingTitle("Rate")]
            [ModSettingDescription("Litres per minute output for creative tank.")]
            public static ModSetting<float> rate = 36000f;

        }

        [ModSettingGroup]
        [ModSettingTitle("Void Tank")]
        public static class VoidTank
        {
            [ModSettingTitle("Rate")]
            [ModSettingDescription("Litres per minute input for void tank.")]
            public static ModSetting<float> rate = 36000f;
        }
    }

}