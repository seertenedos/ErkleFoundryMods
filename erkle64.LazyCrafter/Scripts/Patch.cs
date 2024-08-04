using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace LazyCrafter
{

    [HarmonyPatch]
    public static class Patch
    {
        private static readonly Dictionary<ulong, ulong> _recipeIndex = new Dictionary<ulong, ulong>();

        [HarmonyPatch(typeof(Character.ClientData), nameof(Character.ClientData.userPressedBarSlotCallback))]
        [HarmonyPrefix]
        public static bool CharacterClientDataUserPressedBarSlotCallback(Character.ClientData __instance, uint slotIdx)
        {
            var itemTemplate = __instance.getItemTemplateForHotkeybarSlot(__instance.hotkeyBar_currentBarIdx, slotIdx);
            var clientCharacter = GameRoot.getClientCharacter();

            if (itemTemplate != null && _recipeIndex.TryGetValue(itemTemplate.id, out var recipeId))
            {
                var recipe = ItemTemplateManager.getCraftingRecipeById(recipeId);
                if (recipe == null)
                    return true;

                if (!recipe.isResearched())
                    return true;

                if (recipe.isLockedByMissingEntitlement())
                    return true;

                if (!GlobalStateManager.getRewiredPlayer0().GetButton("Modifier 1")) return true;
                if (Config.General.autoCraftOnlyWhenEmpty.value && InventoryManager.inventoryManager_countByItemTemplateByPtr(clientCharacter.inventoryPtr, itemTemplate.id, IOBool.iotrue) > 0U) return true;

                HotkeyBar.triggerClickAnimationForSlot(slotIdx);

                var amount = GlobalStateManager.getRewiredPlayer0().GetButton("Modifier 2") ? Config.General.autoCraftAltAmount.value : Config.General.autoCraftAmount.value;
                if (amount > itemTemplate.stackSize) amount = (int)itemTemplate.stackSize;

                InfoMessageSystem.addSingleTextInfoMessage($"Crafting {amount}x {itemTemplate.name}");

                GameRoot.addLockstepEvent(new Character.CharacterCraftingEvent(clientCharacter.usernameHash, recipeId, amount));

                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(CraftingRecipe), nameof(CraftingRecipe.onLoad))]
        [HarmonyPostfix]
        public static void CraftingRecipeOnLoad(CraftingRecipe __instance)
        {
            if (__instance.output_data.Length > 0
                && __instance.output[0].itemTemplate != null
                && __instance.tags.Contains("character")
                && !__instance.isHiddenInCharacterCraftingFrame)
            {
                _recipeIndex[__instance.output[0].itemTemplate.id] = __instance.id;
            }
        }
    }

}
