using C3.ModKit;

[ModSettingGroup, ModSettingIdentifier("Logging")]
public static class LoggingSettings
{
    [ModSettingDescription("Adds extra details to log files.")]
    public static ModSetting<bool> verbose = false;
}
/*
[ModSettingGroup, ModSettingIdentifier("Testing")]
public static class TestingSettings
{
    [ModSettingIdentifier("number")]
    [ModSettingDescription("Testing 1 2 3...",
        "More testing 4 5 6...",
        "Final testing 7 8 9")]
    [ModSettingRange(100)]
    [ModSettingServerSync]
    public static ModSetting<float> testNumber = 42f;

    [ModSettingIdentifier("string")]
    [ModSettingSaveLocked]
    public static ModSetting<string> testString = "test";

    [ModSettingIdentifier("int")]
    [ModSettingRange(100)]
    [ModSettingRequiresRestart]
    public static ModSetting<int> testInt;

    [ModSettingTitle("Test Header")]
    [ModSettingDescription("Testing 1 2 3...")]
    public static ModSettingHeader testHeader;

    [ModSettingOption("Test Value 1", 1)]
    [ModSettingOption("Test Value 2", 2)]
    [ModSettingOption("Test Value 3", 3)]
    [ModSettingOption("Test Value 42", 42)]
    [ModSettingOption("Test Value 56", 56)]
    public static ModSetting<int> dropdownTest = 3;

    public static ModSetting<UnityEngine.KeyCode> testKey = UnityEngine.KeyCode.BackQuote;
}

[ModSettingGroup, ModSettingIdentifier("SaveTest")]
[ModSettingSaveLocked]
public static class SaveTestSettings
{
    public static ModSetting<bool> testBool = true;
    public static ModSetting<int> testInt = 37;
    public static ModSetting<float> testFloat = 37f;
    public static ModSetting<string> testString = "37";
}
*/