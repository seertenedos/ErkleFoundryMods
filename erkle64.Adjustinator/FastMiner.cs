using C3;
using C3.ModKit;
using System.Collections.Generic;

namespace FastMiner
{

    [ModSettingGroup, ModSettingIdentifier("FastMiner")]
    [ModSettingServerSync]
    public static class Config
    {
        [ModSettingTitle("Enabled")]
        public static ModSetting<bool> enabled = false;

        [ModSettingTitle("Speed Multiplier")]
        [ModSettingDescription("Mining speed multiplication factor.")]
        public static ModSetting<float> speedMultiplier = 8.0f;
    }

    [AddSystemToGameSimulation]
    public class FastMinerSystem : SystemManager.System
    {
        private Dictionary<ulong, float> _originalMiningTimeReductionInSec = new();

        public override void OnAddedToWorld()
        {
            foreach (var itemTemplate in ItemTemplateManager.getAllItemTemplates().Values)
            {
                if (itemTemplate.flags.HasFlagNonAlloc(ItemTemplate.ItemTemplateFlags.MINING_TOOL))
                {
                    _originalMiningTimeReductionInSec.Add(itemTemplate.id, itemTemplate.miningTimeReductionInSec);

                    if (Config.enabled.value)
                    {
                        itemTemplate.miningTimeReductionInSec *= Config.speedMultiplier.value;
                    }
                }
            }
        }

        public override void OnRemovedFromWorld()
        {
            foreach (var kvp in _originalMiningTimeReductionInSec)
            {
                var itemTemplate = ItemTemplateManager.getItemTemplate(kvp.Key);
                itemTemplate.miningTimeReductionInSec = kvp.Value;
            }
        }
    }

}
