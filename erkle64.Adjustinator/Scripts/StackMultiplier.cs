using C3;
using C3.ModKit;
using System.Collections.Generic;
using UnityEngine;

namespace StackMultiplier
{

    [ModSettingGroup, ModSettingIdentifier("Item Stacks")]
    [ModSettingServerSync]
    [ModSettingOrder(600)]
    public static class Config
    {
        [ModSettingTitle("Enable")]
        public static ModSetting<bool> enabled = false;

        [ModSettingTitle("Stack Multiplier")]
        [ModSettingDescription("Multiplier for stack size.")]
        public static ModSetting<float> stackMultiplier = 2f;

        [ModSettingTitle("Nonstackable Size")]
        [ModSettingDescription("Stack size for normally nonstackable items.")]
        public static ModSetting<int> nonStackableSize = 1;
    }

    [AddSystemToGameSimulation]
    public class FastMinerSystem : SystemManager.System
    {
        public const ItemTemplate.ItemTemplateFlags exclusionFlags =
            ItemTemplate.ItemTemplateFlags.MINING_TOOL |
            ItemTemplate.ItemTemplateFlags.EMOTE |
            ItemTemplate.ItemTemplateFlags.CONSTRUCTION_MATERIAL |
            ItemTemplate.ItemTemplateFlags.CONSTRUCTION_RUBBLE;

        private Dictionary<ulong, uint> _originalStackSizes = new();

        public override void OnAddedToWorld()
        {
            if (!Config.enabled.value) return;

            _originalStackSizes.Clear();

            foreach (var itemTemplate in ItemTemplateManager.getAllItemTemplates().Values)
            {
                var stackMultiplier = Config.stackMultiplier.value;
                if ((itemTemplate.flags & exclusionFlags) == 0)
                {
                    if (itemTemplate.stackSize <= 1)
                    {
                        _originalStackSizes.Add(itemTemplate.id, itemTemplate.stackSize);
                        itemTemplate.stackSize = (uint)Mathf.Max(1, Config.nonStackableSize.value);
                    }
                    else if (stackMultiplier > 0.0f && stackMultiplier != 1.0f)
                    {
                        _originalStackSizes.Add(itemTemplate.id, itemTemplate.stackSize);
                        itemTemplate.stackSize = (uint)Mathf.CeilToInt(itemTemplate.stackSize * stackMultiplier);
                    }
                }
            }
        }

        public override void OnRemovedFromWorld()
        {
            if (!Config.enabled.value) return;

            foreach (var kvp in _originalStackSizes)
            {
                var itemTemplate = ItemTemplateManager.getItemTemplate(kvp.Key);
                itemTemplate.stackSize = kvp.Value;
            }
        }
    }

}
