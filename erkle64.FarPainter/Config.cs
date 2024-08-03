using C3.ModKit;

namespace FarPainter
{

    [ModSettingGroup]
    [ModSettingIdentifier("Paint Tool")]
    [ModSettingTitle("Paint Tool")]
    public static class Config
    {
        [ModSettingTitle("Paint Range")]
        [ModSettingDescription("The range of the paint roller.")]
        public static ModSetting<float> paintRange = 50f;
    }

}