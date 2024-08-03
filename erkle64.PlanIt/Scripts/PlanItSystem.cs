using C3;
using C3.ModKit;
using System.IO;
using Unfoundry;
using UnityEngine;

namespace PlanIt
{
    public static class Config {
        [ModSettingGroup]
        [ModSettingDescription("Key Codes: Backspace, Tab, Clear, Return, Pause, Escape, Space, Exclaim,",
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
            [ModSettingTitle("Open Planner")]
            [ModSettingDescription("Keyboard shortcut for openning the planner.")]
            public static ModSetting<KeyCode> openPlannerKey = KeyCode.KeypadDivide;
        }
    }

    [AddSystemToGameClient]
    public class PlanItSystem : SystemManager.System
    {
        public static LogSource log = new LogSource("PlanIt");

        private PlannerFrame _plannerFrame = null;

        public override void OnAddedToWorld()
        {
            var planFolder = Path.Combine(Application.persistentDataPath, "planit");
            if (!Directory.Exists(planFolder)) Directory.CreateDirectory(planFolder);

            _plannerFrame = Object.Instantiate(AssetManager.Database.LoadAssetAtPath<PlannerFrame>("Assets/erkle64.PlanIt/UI/PlannerFrame.prefab"), GameRoot.getDefaultCanvas().transform, false);
            _plannerFrame.Setup(planFolder);
            _plannerFrame.gameObject.SetActive(false);
        }

        public override void OnRemovedFromWorld()
        {
        }

        [EventHandler]
        public void Update(OnUpdate evt)
        {
            if (Input.GetKeyUp(Config.Input.openPlannerKey.value) && InputHelpers.IsKeyboardInputAllowed)
            {
                TogglePlannerFrame();
            }
        }

        private void TogglePlannerFrame()
        {
            if (!_plannerFrame.IsOpen) _plannerFrame.Show();
            else _plannerFrame.Hide();
        }
    }
}


