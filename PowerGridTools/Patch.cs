using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace PowerGridTools
{
    public static class Plugin
    {
        private static ulong _targetTransformerItemTemplateId;

        internal static IOBool EnableTransformersCallback(ulong entityId, ulong buildableObjectTemplateId, ulong itemTemplateId, IOBool enabled)
        {
            if (enabled == IOBool.iotrue) return IOBool.iotrue;
            if (LoggingSettings.verbose.value) Debug.Log($"Enabling transformer {entityId}");
            GameRoot.addLockstepEvent(new ToggleBuildableEntity(GameRoot.getClientUsernameHash(), entityId));
            return IOBool.iotrue;
        }

        internal static IOBool DisableTransformersCallback(ulong entityId, ulong buildableObjectTemplateId, ulong itemTemplateId, IOBool enabled)
        {
            if (enabled == IOBool.iofalse) return IOBool.iotrue;
            if (LoggingSettings.verbose.value) Debug.Log($"Disabling transformer {entityId}");
            GameRoot.addLockstepEvent(new ToggleBuildableEntity(GameRoot.getClientUsernameHash(), entityId));
            return IOBool.iotrue;
        }

        internal static IOBool EnableSingleTransformerCallback(ulong entityId, ulong buildableObjectTemplateId, ulong itemTemplateId, IOBool enabled)
        {
            if (enabled == IOBool.iotrue) return IOBool.iotrue;
            if (itemTemplateId != _targetTransformerItemTemplateId) return IOBool.iotrue;
            if (LoggingSettings.verbose.value) Debug.Log($"Enabling transformer {entityId}");
            GameRoot.addLockstepEvent(new ToggleBuildableEntity(GameRoot.getClientUsernameHash(), entityId));
            return IOBool.iofalse;
        }

        internal static IOBool DisableSingleTransformerCallback(ulong entityId, ulong buildableObjectTemplateId, ulong itemTemplateId, IOBool enabled)
        {
            if (enabled == IOBool.iofalse) return IOBool.iotrue;
            if (itemTemplateId != _targetTransformerItemTemplateId) return IOBool.iotrue;
            if (LoggingSettings.verbose.value) Debug.Log($"Disabling transformer {entityId}");
            GameRoot.addLockstepEvent(new ToggleBuildableEntity(GameRoot.getClientUsernameHash(), entityId));
            return IOBool.iofalse;
        }

        internal static void SetTargetTransformerItemTemplateId(ulong itemTemplateId)
        {
            _targetTransformerItemTemplateId = itemTemplateId;
        }

        [HarmonyPatch]
        public static class Patch
        {
            private static FieldInfo enabledTransformerArray = typeof(PGF_TR_Widget).GetField("enabledTransformerArray", BindingFlags.NonPublic | BindingFlags.Instance);

            [HarmonyPatch(typeof(PGF_TR_Widget), "Awake")]
            [HarmonyPostfix]
            public static void PGF_TR_Widget_Awake(PGF_TR_Widget __instance)
            {
                var hvgId = __instance.relatedHvGridId;
                var lvgId = __instance.relatedLvGridId;

                var headerObject = __instance.uiText_sum.transform.parent.gameObject;

                var image = headerObject.AddComponent<Image>();
                image.color = Color.white;

                var button = headerObject.AddComponent<Button>();
                Debug.Assert(button != null);
                button.navigation = new Navigation { mode = Navigation.Mode.None };
                button.targetGraphic = image;
                button.transition = Selectable.Transition.ColorTint;
                button.colors = new ColorBlock
                {
                    normalColor = Color.clear,
                    highlightedColor = new Color(0.2f, 0.2f, 0.2f, 1.0f),
                    pressedColor = new Color(0.4f, 0.4f, 0.4f, 1.0f),
                    selectedColor = Color.clear,
                    disabledColor = Color.clear,
                    colorMultiplier = 1.0f,
                    fadeDuration = 0.1f
                };
                button.onClick.AddListener(() =>
                {
                    if (LoggingSettings.verbose.value) Debug.Log($"Transformer widget clicked. {lvgId}");

                    var enabledTransformerCounts = (int[])enabledTransformerArray.GetValue(__instance);
                    var hasEnabledTransformers = false;
                    for (int i = 0; i < GameRoot.RUNNING_IDX_MAX_COUNT_BOT_TRANSFORMER; i++)
                    {
                        if (enabledTransformerCounts[i] > 0)
                        {
                            hasEnabledTransformers = true;
                            break;
                        }
                    }

                    if (hasEnabledTransformers)
                    {
                        EnergySystemManager.energySystem_iterateTransformers(hvgId, lvgId, new EnergySystemManager._nativeHelper_perIteratedTransformer(DisableTransformersCallback));
                    }
                    else
                    {
                        EnergySystemManager.energySystem_iterateTransformers(hvgId, lvgId, new EnergySystemManager._nativeHelper_perIteratedTransformer(EnableTransformersCallback));
                    }
                });
            }

            [HarmonyPatch(typeof(PGF_TR_Row), nameof(PGF_TR_Row.init))]
            [HarmonyPostfix]
            public static void PGF_TR_Row_init(PGF_TR_Row __instance, ItemTemplate itemTemplate, bool enabled)
            {
                if (__instance.TryGetComponent<TransformerRowClickHandler>(out var handler))
                {
                    handler.init(itemTemplate.id, enabled);
                    return;
                }

                var frameObject = __instance.gameObject;

                handler = frameObject.AddComponent<TransformerRowClickHandler>();
                handler.init(itemTemplate.id, enabled);

                var image = frameObject.AddComponent<Image>();
                image.color = Color.white;

                var button = frameObject.AddComponent<Button>();
                button.navigation = new Navigation { mode = Navigation.Mode.None };
                button.targetGraphic = image;
                button.transition = Selectable.Transition.ColorTint;
                button.colors = new ColorBlock
                {
                    normalColor = Color.clear,
                    highlightedColor = new Color(0.2f, 0.2f, 0.2f, 1.0f),
                    pressedColor = new Color(0.4f, 0.4f, 0.4f, 1.0f),
                    selectedColor = Color.clear,
                    disabledColor = Color.clear,
                    colorMultiplier = 1.0f,
                    fadeDuration = 0.1f
                };
                button.onClick.AddListener(handler.OnClick);
            }
        }
    }
}


