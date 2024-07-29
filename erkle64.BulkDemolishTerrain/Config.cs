using static BulkDemolishTerrain.Plugin;
using UnityEngine;
using C3.ModKit;

namespace BulkDemolishTerrain
{
    public static class Config
    {
        [ModSettingGroup]
        public static class General
        {
            [ModSettingServerSync]
            [ModSettingTitle("Player Placed Only")]
            [ModSettingDescription("Only allow demolishing player placed terrain (Includes concrete).")]
            public static ModSetting<bool> playerPlacedOnly = false;

            [ModSettingTitle("Remove Liquids")]
            [ModSettingDescription("Remove all liquids, water, when demolishing.")]
            public static ModSetting<bool> removeLiquids = true;

            [ModSettingServerSync]
            [ModSettingTitle("Ignore Mining Level")]
            [ModSettingDescription("Ignore mining level research and remove terrain anyway.")]
            public static ModSetting<bool> ignoreMiningLevel = false;
        }

        [ModSettingGroup]
        [ModSettingHidden]
        public static class Modes
        {
            [ModSettingTitle("Current Terrain Mode")]
            [ModSettingDescription("Collect, Destroy, Ignore, CollectTerrainOnly, DestroyTerrainOnly, LiquidOnly")]
            public static ModSetting<TerrainMode> currentTerrainMode = TerrainMode.Collect;
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
            [ModSettingTitle("Change Mode")]
            public static ModSetting<KeyCode> changeModeKey = KeyCode.Backslash;
        }
    }
}
