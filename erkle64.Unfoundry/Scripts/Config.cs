using C3.ModKit;

namespace Unfoundry
{

    [ModSettingGroup, ModSettingIdentifier("Action Manager")]
    public static class Config
    {
        [ModSettingIdentifier("Max Queued Events Per Frame")]
        public static ModSetting<int> maxQueuedEventsPerFrame = 40;
    }

}