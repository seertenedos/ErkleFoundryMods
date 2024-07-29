using UnityEngine;
using System.IO;
using Unfoundry;
using System.Collections.Generic;
using C3.ModKit;
using System.Reflection;
using C3;

namespace Duplicationer
{
    public static class Config {
        [ModSettingGroup]
        public static class Events
        {
            [ModSettingTitle("Maximum Building Validations Per Frame")]
            public static ModSetting<int> maxBuildingValidationsPerFrame = 4;

            [ModSettingTitle("Maximum Terrain Validations Per Frame")]
            public static ModSetting<int> maxTerrainValidationsPerFrame = 20;
        }

        [ModSettingGroup]
        public static class Preview
        {
            [ModSettingTitle("Preview Opacity")]
            [ModSettingDescription("Opacity of preview models.", "0.0 = transparent/invisible.", "1.0 = opaque.")]
            [ModSettingRange(0.0f, 1.0f)]
            public static ModSetting<float> previewAlpha = 0.5f;
        }

        [ModSettingGroup]
        [ModSettingDescription(
            "Key Codes: Backspace, Tab, Clear, Return, Pause, Escape, Space, Exclaim,",
            "DoubleQuote, Hash, Dollar, Percent, Ampersand, Quote, LeftParen, RightParen,",
            "Asterisk, Plus, Comma, Minus, Period, Slash,",
            "Alpha0, Alpha1, Alpha2, Alpha3, Alpha4, Alpha5, Alpha6, Alpha7, Alpha8, Alpha9,",
            "Colon, Semicolon, Less, Equals, Greater, Question, At,",
            "LeftBracket, Backslash, RightBracket, Caret, Underscore, BackQuote,",
            "A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,",
            "LeftCurlyBracket, Pipe, RightCurlyBracket, Tilde, Delete,",
            "Keypad0, Keypad1, Keypad2, Keypad3, Keypad4, Keypad5, Keypad6, Keypad7, Keypad8, Keypad9,",
            "KeypadPeriod, KeypadDivide, KeypadMultiply, KeypadMinus, KeypadPlus, KeypadEnter, KeypadEquals,",
            "UpArrow, DownArrow, RightArrow, LeftArrow, Insert, Home, End, PageUp, PageDown,",
            "F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14, F15,",
            "Numlock, CapsLock, ScrollLock,",
            "RightShift, LeftShift, RightControl, LeftControl, RightAlt, LeftAlt, RightApple, RightApple,",
            "LeftCommand, LeftCommand, LeftWindows, RightWindows, AltGr,",
            "Help, Print, SysReq, Break, Menu,",
            "Mouse0, Mouse1, Mouse2, Mouse3, Mouse4, Mouse5, Mouse6")]
        public static class Input
        {
            [ModSettingTitle("Toggle Blueprint Tool")]
            [ModSettingDescription("Keyboard shortcut for toggling the blueprint tool.")]
            public static ModSetting<KeyCode> toggleBlueprintToolKey = KeyCode.K;

            [ModSettingTitle("Paste Blueprint")]
            [ModSettingDescription("Keyboard shortcut key for confirm paste.")]
            public static ModSetting<KeyCode> pasteBlueprintKey = KeyCode.J;

            [ModSettingTitle("Toggle Blueprint Panel")]
            [ModSettingDescription("Keyboard shortcut key to open the blueprint panel.")]
            public static ModSetting<KeyCode> togglePanelKey = KeyCode.N;

            [ModSettingTitle("Save Blueprint")]
            [ModSettingDescription("Keyboard shortcut key to open the save blueprint panel.")]
            public static ModSetting<KeyCode> saveBlueprintKey = KeyCode.Period;

            [ModSettingTitle("Load Blueprint")]
            [ModSettingDescription("Keyboard shortcut key to open the load blueprint panel.")]
            public static ModSetting<KeyCode> loadBlueprintKey = KeyCode.Comma;
        }

        [ModSettingGroup]
        public static class Cheats
        {
            [ModSettingTitle("Cheat Mode Allowed")]
            [ModSettingDescription("Enable the cheat mode button.")]
            public static ModSetting<bool> cheatModeAllowed;

            [ModSettingTitle("Cheat Mode Enabled")]
            [ModSettingDescription("Enable cheat mode if allowed.")]
            public static ModSetting<bool> cheatModeEnabled;

            [ModSettingTitle("Allow Unresearched Recipes")]
            [ModSettingDescription("Allow setting unresearched recipes on production machines.")]
            public static ModSetting<bool> allowUnresearchedRecipes;
        }
    }

    [UnfoundryMod(GUID)]
    public class DuplicationerPlugin : UnfoundryPlugin
    {
        public const string
            MODNAME = "Duplicationer",
            AUTHOR = "erkle64",
            GUID = AUTHOR + "." + MODNAME,
            VERSION = "0.5.0";

        public static LogSource log;

        public static string BlueprintFolder;

        public const string BlueprintExtension = "ebp";

        private static BlueprintToolCHM blueprintTool;

        public static int BlueprintToolModeIndex { get; private set; }

        public DuplicationerPlugin()
        {
            log = new LogSource(MODNAME);
        }

        public override void Load(Mod mod)
        {
            BlueprintFolder = Path.Combine(Application.persistentDataPath, MODNAME.ToLower());

            log.Log("Loading Duplicationer");
            log.Log($"blueprintFolder: {BlueprintFolder}");

            if (!Directory.Exists(BlueprintFolder)) Directory.CreateDirectory(BlueprintFolder);
        }

        public override void GameEnter()
        {
            blueprintTool = new BlueprintToolCHM();
            blueprintTool.LoadIconSprites();
            CommonEvents.OnDeselectTool += CustomHandheldModeManager.ExitCurrentMode2;
            CommonEvents.OnUpdate += OnUpdate;

            BlueprintToolModeIndex = CustomHandheldModeManager.RegisterMode(blueprintTool);
        }

        public override void GameExit()
        {
            CommonEvents.OnDeselectTool -= OnUpdate;
            CommonEvents.OnUpdate -= OnUpdate;

            CustomHandheldModeManager.DeregisterMode(blueprintTool);

            blueprintTool = null;
        }

        private static void OnUpdate()
        {
            var clientCharacter = GameRoot.getClientCharacter();
            if (clientCharacter == null) return;

            if (Input.GetKeyDown(Config.Input.toggleBlueprintToolKey.value) && InputHelpers.IsKeyboardInputAllowed)
            {
                CustomHandheldModeManager.ToggleMode(clientCharacter, BlueprintToolModeIndex);
            }
        }

        public static bool IsCheatModeEnabled => Config.Cheats.cheatModeAllowed.value && Config.Cheats.cheatModeEnabled.value;
    }
}
