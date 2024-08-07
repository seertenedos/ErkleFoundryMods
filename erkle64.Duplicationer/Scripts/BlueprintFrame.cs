using HarmonyLib;
using System.Collections.Generic;
using TMPro;
using Unfoundry;
using UnityEngine;
using UnityEngine.UI;

namespace Duplicationer
{
    internal class BlueprintFrame : DuplicationerFrame
    {
        [Header("Blueprint Frame")]
        [SerializeField] private TextMeshProUGUI _textMaterialReport = null;
        [SerializeField] private TextMeshProUGUI _textPositionX = null;
        [SerializeField] private TextMeshProUGUI _textPositionY = null;
        [SerializeField] private TextMeshProUGUI _textPositionZ = null;
        [SerializeField] private GameObject _rowCheats;
        [SerializeField] private Button _buttonCheatMode;
        [SerializeField] private Button _buttonSave;
        [SerializeField] private Button _buttonConfirmCopy;
        [SerializeField] private Button _buttonConfirmPaste;
        [SerializeField] private GameObject _containerClearRecipes;
        [SerializeField] private GameObject _containerDemolish;
        [SerializeField] private GameObject _containerDestroy;
        [SerializeField] private GameObject _containerPosition;
        [SerializeField] private GameObject _containerMaterialReport;

        private float _nextUpdateTimeCountTexts = 0.0f;
        private int _materialReportMarkedLine = -1;
        private List<ulong> _materialReportTemplateIds = new List<ulong>();

        private string CheatModeButtonText => DuplicationerSystem.IsCheatModeEnabled ? "Disable Cheat Mode" : "Enable Cheat Mode";

        public void Show()
        {
            if (IsOpen) return;

            _tool.HideSaveFrame(true);
            _tool.HideLibraryFrame(true);
            _tool.HideFolderFrame(true);

            if (!Config.Cheats.cheatModeAllowed.value)
            {
                _rowCheats.SetActive(false);
            }
            else
            {
                var textComponent = _buttonCheatMode.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null) textComponent.text = CheatModeButtonText;
            }

            Shown();

            UpdateBlueprintPositionText();
            ForceUpdateMaterialReport();
        }

        void Update()
        {
            if (!IsOpen) return;

            _containerClearRecipes.gameObject.SetActive(_tool.CurrentBlueprint != null && _tool.CurrentBlueprint.HasRecipes);
            _containerDemolish.gameObject.SetActive(_tool.boxMode != BlueprintToolCHM.BoxMode.None && _tool.CurrentMode != _tool.modeSelectArea);
            //_containerDestroy.gameObject.SetActive(_tool.boxMode != BlueprintToolCHM.BoxMode.None && _tool.CurrentMode != _tool.modeSelectArea);
            _containerDestroy.gameObject.SetActive(false);
            _containerPosition.gameObject.SetActive(_tool.boxMode == BlueprintToolCHM.BoxMode.Blueprint);
            _buttonSave.interactable = _tool.IsBlueprintLoaded;
            _buttonConfirmPaste.interactable = _tool.CurrentMode != null && _tool.CurrentMode.AllowPaste(_tool);
            _buttonConfirmCopy.interactable = _tool.CurrentMode != null && _tool.CurrentMode.AllowCopy(_tool);
        }

        public void OnClick_Save()
        {
            _tool.BeginSaveBlueprint();
        }

        public void OnClick_Load()
        {
            _tool.BeginLoadBlueprint();
        }

        public void OnClick_Copy()
        {
            _tool.CopySelection();
        }

        public void OnClick_Paste()
        {
            _tool.PlaceBlueprintMultiple(_tool.CurrentBlueprintAnchor, _tool.repeatFrom, _tool.repeatTo);
        }

        public void OnClick_ClearRecipes()
        {
            _tool.ClearBlueprintRecipes();
        }

        public void OnClick_MoveX(bool negative)
        {
            _tool.MoveBlueprint(_tool.CurrentBlueprintAnchor + new Vector3Int(negative ? -1 : 1, 0, 0) * _tool.NudgeX);
        }

        public void OnClick_MoveY(bool negative)
        {
            _tool.MoveBlueprint(_tool.CurrentBlueprintAnchor + new Vector3Int(0, negative ? -1 : 1, 0) * _tool.NudgeY);
        }

        public void OnClick_MoveZ(bool negative)
        {
            _tool.MoveBlueprint(_tool.CurrentBlueprintAnchor + new Vector3Int(0, 0, negative ? -1 : 1) * _tool.NudgeZ);
        }

        public void OnClick_CheatMode()
        {
            if (_buttonCheatMode == null) return;
            if (!Config.Cheats.cheatModeAllowed.value) return;
            Config.Cheats.cheatModeEnabled.value = !Config.Cheats.cheatModeEnabled.value;
            var textComponent = _buttonCheatMode.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent == null) return;
            textComponent.text = CheatModeButtonText;
        }

        public void OnClick_DemolishDestroy(DemolishDestroyButtonMode mode)
        {
            if (mode.isDestroy)
            {
                ConfirmationFrame.Show($"Permanently destroy {mode.label} in selection?",
                    () => _tool.DestroyArea(mode.includeBuildings, mode.includeBlocks, mode.includeTerrain, mode.includeDecor));
            }
            else
            {
                _tool.DemolishArea(mode.includeBuildings, mode.includeBlocks, mode.includeTerrain, mode.includeDecor);
            }
        }

        public void OnChange_PreviewOpacity(float value)
        {
            Config.Preview.previewAlpha.value = value;
            _tool.SetPlaceholderOpacity(value);
        }

        public void OnLineChanged_MaterialReport(int line)
        {
            _materialReportMarkedLine = line;
        }

        public void OnLineClicked_MaterialReport(int line)
        {
            if (line < 0 || line >= _materialReportTemplateIds.Count) return;

            var templateId = _materialReportTemplateIds[line];
            var template = ItemTemplateManager.getItemTemplate(templateId);
            if (template == null) return;

            ConfirmationFrame.Show($"Remove all '{template.name}'?", "Remove", () => {
                _tool.RemoveItemFromBlueprint(template);
            });

            ForceUpdateMaterialReport();
        }

        internal void UpdateBlueprintPositionText()
        {
            if (!IsOpen) return;

            if (_textPositionX != null) _textPositionX.text = string.Format("Position X: {0}", _tool.CurrentBlueprintAnchor.x);
            if (_textPositionY != null) _textPositionY.text = string.Format("Position Y: {0}", _tool.CurrentBlueprintAnchor.y);
            if (_textPositionZ != null) _textPositionZ.text = string.Format("Position Z: {0}", _tool.CurrentBlueprintAnchor.z);
        }

        internal void UpdateMaterialReport()
        {
            if (!IsOpen || Time.time < _nextUpdateTimeCountTexts) return;

            ForceUpdateMaterialReport();
        }

        internal void ForceUpdateMaterialReport()
        {
            _nextUpdateTimeCountTexts = Time.time + 0.5f;

            if (_tool.CurrentBlueprint == null)
            {
                _containerMaterialReport.SetActive(false);
                return;
            }

            int repeatCount = _tool.RepeatCount.x * _tool.RepeatCount.y * _tool.RepeatCount.z;
            ulong inventoryId = GameRoot.getClientCharacter().inventoryId;
            ulong inventoryPtr = inventoryId != 0 ? InventoryManager.inventoryManager_getInventoryPtr(inventoryId) : 0;

            var materialReportBuilder = new System.Text.StringBuilder();
            var lineIndex = 0;
            void AppendLine(string text)
            {
                if (lineIndex == _materialReportMarkedLine) materialReportBuilder.AppendLine($"<mark>{text}</mark>");
                else materialReportBuilder.AppendLine(text);
                lineIndex++;
            }

            _materialReportTemplateIds.Clear();

            int totalItemCount = 0;
            int totalDoneCount = 0;
            foreach (var kv in _tool.CurrentBlueprint.ShoppingList)
            {
                var itemCount = kv.Value.count * repeatCount;
                if (itemCount > 0)
                {
                    totalItemCount += itemCount;

                    var name = kv.Value.name;
                    var templateId = kv.Value.itemTemplateId;
                    if (templateId != 0)
                    {
                        var doneCount = BlueprintPlaceholder.GetStateCount(templateId, BlueprintPlaceholder.State.Done);
                        totalDoneCount += doneCount;

                        _materialReportTemplateIds.Add(templateId);

                        if (inventoryPtr != 0)
                        {
                            var inventoryCount = InventoryManager.inventoryManager_countByItemTemplateByPtr(inventoryPtr, templateId, IOBool.iotrue);

                            if (doneCount > 0)
                            {
                                AppendLine($"<color=#CCCCCC>{name}:</color> {itemCount - doneCount} <color=#FFFFAA>({inventoryCount})</color> (<color=#AACCFF>{doneCount}</color>/{itemCount})");
                            }
                            else
                            {
                                AppendLine($"<color=#CCCCCC>{name}:</color> {itemCount} <color=#FFFFAA>({inventoryCount})</color>");
                            }
                        }
                        else
                        {
                            AppendLine($"<color=#CCCCCC>{name}:</color> {itemCount} <color=#FFFFAA>(###)</color>");
                        }
                    }
                    else
                    {
                        AppendLine($"<color=#CCCCCC>{name}:</color> {itemCount}");
                    }
                }
            }

            if (totalItemCount > 0)
            {
                if (totalDoneCount > 0)
                {
                    materialReportBuilder.AppendLine($"<color=#CCCCCC>Total:</color> {totalItemCount - totalDoneCount} (<color=#AACCFF>{totalDoneCount}</color>/{totalItemCount})");
                }
                else
                {
                    materialReportBuilder.AppendLine($"<color=#CCCCCC>Total:</color> {totalItemCount}");
                }
            }

            var text = materialReportBuilder.ToString();
            _textMaterialReport.text = text;

            _containerMaterialReport.SetActive(!string.IsNullOrEmpty(text));
        }
    }
}
