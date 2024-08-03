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

            /*if (_frame == null)
            {
                *//*UIBuilder.BeginWith(GameRoot.getDefaultCanvas())
                    .Element_Panel("ItemSelectFrame", "corner_cut_outline", new Color(0.133f, 0.133f, 0.133f, 1.0f), new Vector4(13, 10, 8, 13))
                        .Keep(out _frame)
                        .SetRectTransform(20.0f, 20.0f, -20.0f, -20.0f, 0.5f, 0.5f, 0.0f, 0.0f, 1.0f, 1.0f)
                        .Element_Header("HeaderBar", "corner_cut_outline", new Color(0.0f, 0.6f, 1.0f, 1.0f), new Vector4(13, 3, 8, 13))
                            .SetRectTransform(0.0f, -60.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, 1.0f)
                            .Layout()
                                .MinWidth(200)
                                .MinHeight(60)
                            .Done
                            .Element("Heading")
                                .SetRectTransform(0.0f, 0.0f, -60.0f, 0.0f, 0.0f, 0.5f, 0.0f, 0.0f, 1.0f, 1.0f)
                                .Component_Text("PlanIt - Select Item", "OpenSansSemibold SDF", 34.0f, Color.white)
                            .Done
                            .Element_Button("Button Close", "corner_cut_fully_inset", Color.white, new Vector4(13.0f, 1.0f, 4.0f, 13.0f))
                                .SetOnClick(() => Hide(false))
                                .SetRectTransform(-60.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.5f, 1.0f, 0.0f, 1.0f, 1.0f)
                                .SetTransitionColors(new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 0.25f, 0.0f, 1.0f), new Color(1.0f, 0.0f, 0.0f, 1.0f), new Color(1.0f, 0.25f, 0.0f, 1.0f), new Color(0.5f, 0.5f, 0.5f, 1.0f), 1.0f, 0.1f)
                                .Element("Image")
                                    .SetRectTransform(5.0f, 5.0f, -5.0f, -5.0f, 0.5f, 0.5f, 0.0f, 0.0f, 1.0f, 1.0f)
                                    .Component_Image("cross", Color.white, Image.Type.Sliced, Vector4.zero)
                                .Done
                            .Done
                        .Done
                        .Element("Content")
                            .SetRectTransform(0.0f, 0.0f, 0.0f, -60.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 1.0f)
                            .SetGridLayout(new RectOffset(10, 10, 10, 10), new Vector2(74, 74), new Vector2(4, 4), GridLayoutGroup.Corner.UpperLeft, GridLayoutGroup.Axis.Horizontal, TextAnchor.UpperLeft, GridLayoutGroup.Constraint.Flexible)
                            .Do(BuildContent)
                        .Done
                    .Done
                .End();*//*

                AudioManager.playUISoundEffect(ResourceDB.resourceLinker.audioClip_UIOpen);
                GlobalStateManager.addCursorRequirement();
            }*/

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
