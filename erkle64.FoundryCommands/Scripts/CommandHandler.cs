using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
//   ^\/ give\s + ([\s\w\d] *?)\s(\d +)$

namespace FoundryCommands
{
    public class CommandHandler
    {
        public delegate void ProcessCommandDg(string[] arguments);

        private Regex regex;
        private ProcessCommandDg onProcessCommand;

        public string[] helpNames { get; private set; } = new string[0];
        public string helpText { get; private set; } = string.Empty;

        public CommandHandler(string pattern, ProcessCommandDg onProcessCommand)
        {
            regex = new Regex(pattern, RegexOptions.IgnoreCase|RegexOptions.Singleline);
            this.onProcessCommand = onProcessCommand;
        }

        public CommandHandler SetHelpNames(params string[] helpNames)
        {
            this.helpNames = helpNames;
            return this;
        }

        public CommandHandler SetHelpText(string helpText)
        {
            this.helpText = helpText;
            return this;
        }

        public bool TryProcessCommand(string message)
        {
            var match = regex.Match(message);
            if(!match.Success) return false;

            var arguments = new string[match.Groups.Count - 1];
            for (int i = 0; i < match.Groups.Count - 1; ++i)
            {
                var group = match.Groups[i + 1];
                arguments[i] = "";
                for (int j = 0; j < group.Captures.Count; ++j)
                {
                    arguments[i] += group.Captures[j].Value;
                }
            }
            int argumentCount = arguments.Length;
            for (; argumentCount > 0; --argumentCount) if (arguments[argumentCount - 1].Length > 0) break;

            onProcessCommand?.Invoke((argumentCount == arguments.Length) ? arguments : new ArraySegment<string>(arguments, 0, argumentCount).ToArray());

            ChatFrame.hideMessageBox();
            return true;
        }
    }
}
