using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Unfoundry
{
    public static class TabletHelper
    {
        public static void SetTabletTextAnalyzer(string text)
        {
            GameRoot.getTabletHH().uiText_analyzer.setText(text);
        }

        public static void SetTabletTextLastCopiedConfig(string text)
        {
            GameRoot.getTabletHH().uiText_lastCopiedConfig.setText(text);
        }

        public static void SetTabletTextQuickActions(string text)
        {
            GameRoot.getTabletHH().uiText_quickActions.setText(text);
        }

        public static void SetTabletTexts(string analyzer, string quickActions, string lastCopiedConfig)
        {
            HandheldTabletHH tabletHH = GameRoot.getTabletHH();
            tabletHH.uiText_analyzer.setText(analyzer);
            tabletHH.uiText_quickActions.setText(quickActions);
            tabletHH.uiText_lastCopiedConfig.setText(lastCopiedConfig);
        }

        private static readonly FieldInfo list_signalEntries_field = typeof(HandheldTabletHH).GetField("list_signalEntries", BindingFlags.NonPublic | BindingFlags.Instance);
        public static void ClearDataSignals()
        {
            HandheldTabletHH tabletHH = GameRoot.getTabletHH();

            List<SignalGridEntry> list_signalEntries = list_signalEntries_field.GetValue(tabletHH) as List<SignalGridEntry>;
            foreach (var entry in list_signalEntries)
            {
                GameObject.Destroy(entry.gameObject);
            }
            list_signalEntries.Clear();

            tabletHH.container_dataMemoryGrid.SetActive(false);
            tabletHH.container_dataProcessorGrid.SetActive(false);
            tabletHH.container_dataCompareGrid.SetActive(false);
        }
    }
}