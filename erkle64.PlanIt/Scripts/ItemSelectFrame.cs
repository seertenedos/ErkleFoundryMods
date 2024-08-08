using System.Collections.Generic;
using Unfoundry;
using UnityEngine;

namespace PlanIt
{
    internal class ItemSelectFrame : UIFrame
    {
        [Header("Item Select Frame")]
        [SerializeField] private IconButton _itemSelectButtonPrefab;
        [SerializeField] private Transform _itemListTransform;

        public delegate void OnConfirmDelegate(ItemElementTemplate result);
        public delegate void OnCancelDelegate();

        private OnConfirmDelegate _onConfirm;
        private OnCancelDelegate _onCancel;
        private ItemElementTemplate _result = ItemElementTemplate.Empty;

        public bool IsOpen => gameObject.activeSelf;

        public void Show(OnConfirmDelegate onConfirm, OnCancelDelegate onCancel = null)
        {
            _onConfirm = onConfirm;
            _onCancel = onCancel;

            gameObject.SetActive(true);
            AudioManager.playUISoundEffect(ResourceDB.resourceLinker.audioClip_UIOpen);
            GlobalStateManager.addCursorRequirement();
        }

        public void Hide(bool result)
        {
            if (IsOpen)
            {
                gameObject.SetActive(false);
                AudioManager.playUISoundEffect(ResourceDB.resourceLinker.audioClip_UIClose);
                GlobalStateManager.removeCursorRequirement();

                if (result) _onConfirm?.Invoke(_result);
                else _onCancel?.Invoke();
            }
        }

        public void BuildContent()
        {
            var done = new HashSet<ItemElementTemplate>();

            var categories = ItemTemplateManager.getCraftingRecipeCategoryDictionary();
            foreach (var category in categories.Values)
            {
                foreach (var rowGroup in category.list_rowGroups)
                {
                    foreach (var recipe in rowGroup.list_recipes)
                    {
                        var itemElement = ItemElementTemplate.Empty;

                        foreach (var output in recipe.output_elemental)
                        {
                            itemElement = new ItemElementTemplate(output.Key);
                            break;
                        }

                        foreach (var output in recipe.output)
                        {
                            itemElement = new ItemElementTemplate(output.itemTemplate);
                            break;
                        }

                        if (itemElement.isValid && !done.Contains(itemElement))
                        {
                            done.Add(itemElement);

                            var itemSelectButton = Instantiate(_itemSelectButtonPrefab, _itemListTransform);
                            itemSelectButton.Setup(itemElement.icon, itemElement.name);
                            itemSelectButton.onClick += () =>
                            {
                                _result = itemElement;
                                Hide(true);
                            };
                        }
                    }
                }
            }

            foreach (var recipe in ItemElementRecipe.AllRecipes)
            {
                var itemElement = ItemElementTemplate.Empty;
                if (recipe.outputs.Length > 0 && recipe.inputs.Length > 0)
                {
                    itemElement = recipe.outputs[0].itemElement;

                    if (!done.Contains(itemElement))
                    {
                        done.Add(itemElement);

                        var itemSelectButton = Instantiate(_itemSelectButtonPrefab, _itemListTransform);
                        itemSelectButton.Setup(itemElement.icon, itemElement.name);
                        itemSelectButton.onClick += () =>
                        {
                            _result = itemElement;
                            Hide(true);
                        };
                    }
                }
            }
        }

        public override void iec_triggerFrameClose()
        {
            Hide(false);
        }

        public override bool IsModal()
        {
            return true;
        }
    }
}
