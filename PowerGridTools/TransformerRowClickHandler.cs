using UnityEngine;

namespace PowerGridTools
{
    public class TransformerRowClickHandler : MonoBehaviour
    {
        private ulong _itemTemplateId;
        private bool _enabled;

        public void init(ulong itemTemplateId, bool enabled)
        {
            _itemTemplateId = itemTemplateId;
            _enabled = enabled;
        }

        public void OnClick()
        {
            var trWidget = GetComponentInParent<PGF_TR_Widget>();

            if (LoggingSettings.verbose.value) Debug.Log($"Transformer row clicked. {trWidget.relatedHvGridId} {_itemTemplateId} {_enabled}");

            Plugin.SetTargetTransformerItemTemplateId(_itemTemplateId);
            EnergySystemManager.energySystem_iterateTransformers(
                trWidget.relatedHvGridId,
                trWidget.relatedLvGridId,
                _enabled
                    ? new EnergySystemManager._nativeHelper_perIteratedTransformer(Plugin.DisableSingleTransformerCallback)
                    : new EnergySystemManager._nativeHelper_perIteratedTransformer(Plugin.EnableSingleTransformerCallback));
        }
    }
}
