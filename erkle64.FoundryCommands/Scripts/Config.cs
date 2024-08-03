using C3.ModKit;

namespace FoundryCommands
{

    [ModSettingGroup, ModSettingIdentifier("Drag")]
    [ModSettingOrder(0)]
    public static class Config
    {
        [ModSettingTitle("Drag Range")]
        [ModSettingDescription("Default drag range.", "Automatically set by the /drag command.")]
        public static ModSetting<float> dragRange = 38f;

        [ModSettingTitle("Maximum Drag Buffer")]
        [ModSettingDescription(
            "WARNING: Experimental feature!",
            "May cause crashing if used incorrectly.",
            "The maximum number of blocks that can be dragged at once.",
            "Will be rounded up to the next multiple of 1023.")]
        public static ModSetting<int> maxDragBuffer = 2046;
    }

    [ModSettingGroup]
    [ModSettingTitle("Command Info")]
    [ModSettingOrder(100)]
    public static class CommandInfo
    {
        [ModSettingTitle("<b>/drag</b> <i>range</i>")]
        [ModSettingDescription(
            "Change the maximum range for drag building.",
            "Use <b>/drag</b> 0 to restore default.")]
        public static ModSettingHeader drag;

        [ModSettingTitle("<b>/tp</b> <i>waypoint-name</i>\n<b>/teleport</b> <i>waypoint-name</i>")]
        [ModSettingDescription("Teleport to the named waypoint.")]
        public static ModSettingHeader teleport;

        [ModSettingTitle("<b>/tpr</b>\n<b>/ret</b>\n<b>/return</b>")]
        [ModSettingDescription("Teleport to the named waypoint.")]
        public static ModSettingHeader ret;

        [ModSettingTitle("<b>/monitor</b>\n<b>/mon</b>")]
        [ModSettingDescription("Monitors a tank, modular tank or logistics container's contents once per second. Use while not looking at a valid building to stop.")]
        public static ModSettingHeader monitor;

        [ModSettingTitle("<b>/monitor</b> <i>interval</i>\n<b>/mon</b> <i>interval</i>")]
        [ModSettingDescription("Monitors a tank, modular tank or logistics container's contents with a custom interval. Use while not looking at a valid building to stop.")]
        public static ModSettingHeader monitorInterval;

        [ModSettingTitle("<b>/skyPlatform</b>")]
        [ModSettingDescription("Opens the sky platform frame.")]
        public static ModSettingHeader skyPlatform;

        [ModSettingTitle("<b>/time</b>")]
        [ModSettingDescription("Displays the current time of day.")]
        public static ModSettingHeader time;

        [ModSettingTitle("<b>/time</b> <i>HH</i>")]
        [ModSettingDescription("Set the time of day to HH:00")]
        public static ModSettingHeader timeHH;

        [ModSettingTitle("<b>/time</b> <i>HH:MM</i>")]
        [ModSettingDescription("Set the time of day to HH:MM")]
        public static ModSettingHeader timeHHMM;

        [ModSettingTitle("<b>/calculate</b> <i>expression</i>\n<b>/calc</b> <i>expression</i>\n<b>/c</b> <i>expression</i>")]
        [ModSettingDescription("Calculate the result of a mathematical expression. See Expressive wiki for available functions.")]
        public static ModSettingHeader calculate;

        [ModSettingTitle("<b>count</b>")]
        [ModSettingDescription(@"Dump counts for all buildings within loading distance of the player. Saves to %AppData%\\..\\LocalLow\\Channel 3 Entertainment\\Foundry\\FoundryCommands\\count.txt")]
        public static ModSettingHeader count;

        [ModSettingTitle("<b>/give</b> <i>item</i>")]
        [ModSettingDescription(@"Spawn a stack of items into your inventory.")]
        public static ModSettingHeader give;

        [ModSettingTitle("<b>/give</b> <i>item</i> <i>amount</i>")]
        [ModSettingDescription(@"Spawn a specific number of items into your inventory.")]
        public static ModSettingHeader giveAmount;
    }

}