using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Duplicationer
{
    internal class FolderFrame : DuplicationerFrame
    {
        [Header("Folder Frame")]
        [SerializeField] private GameObject _folderGridObject = null;
        [SerializeField] private GameObject _folderFramePreviewContainer = null;
        [SerializeField] private Image _folderFrameIconImage = null;
        [SerializeField] private TMP_InputField _folderFrameNameInputField = null;
        [SerializeField] private Button _buttonConfirm = null;

        private Image _folderFramePreviewIconImage = null;
        private TextMeshProUGUI _folderFramePreviewLabel = null;
        private ItemTemplate _folderFrameIconItemTemplate = null;
        private string _relativePath = null;
        private string _originalFolderName = null;

        public void Show(string relativePath, string folderName)
        {
            if (IsOpen) return;

            _tool.HideBlueprintFrame(true);
            _tool.HideSaveFrame(true);

            _relativePath = relativePath;
            _originalFolderName = folderName;

            var originalPath = Path.Combine(DuplicationerSystem.BlueprintFolder, relativePath, folderName);

            /*ulong usernameHash = GameRoot.getClientCharacter().usernameHash;
            UIBuilder.BeginWith(GameRoot.getDefaultCanvas())
                .Element_Panel("Folder Frame", "corner_cut_outline", new Color(0.133f, 0.133f, 0.133f, 1.0f), new Vector4(13, 10, 8, 13))
                    .Keep(out _frameRoot)
                    .SetRectTransform(100, 100, -100, -100, 0.5f, 0.5f, 0, 0, 1, 1)
                    .Element_Header("HeaderBar", "corner_cut_outline", new Color(0.0f, 0.6f, 1.0f, 1.0f), new Vector4(13, 3, 8, 13))
                        .SetRectTransform(0.0f, -60.0f, 0.0f, 0.0f, 0.5f, 1.0f, 0.0f, 1.0f, 1.0f, 1.0f)
                        .Element("Heading")
                            .SetRectTransform(0.0f, 0.0f, -60.0f, 0.0f, 0.0f, 0.5f, 0.0f, 0.0f, 1.0f, 1.0f)
                            .Component_Text($"Folder - /{relativePath}", "OpenSansSemibold SDF", 34.0f, Color.white)
                        .Done
                        .Element_Button("Button Close", "corner_cut_fully_inset", Color.white, new Vector4(13.0f, 1.0f, 4.0f, 13.0f))
                            .SetOnClick(() => Hide())
                            .SetRectTransform(-60.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.5f, 1.0f, 0.0f, 1.0f, 1.0f)
                            .SetTransitionColors(new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 0.25f, 0.0f, 1.0f), new Color(1.0f, 0.0f, 0.0f, 1.0f), new Color(1.0f, 0.25f, 0.0f, 1.0f), new Color(0.5f, 0.5f, 0.5f, 1.0f), 1.0f, 0.1f)
                            .Element("Image")
                                .SetRectTransform(5.0f, 5.0f, -5.0f, -5.0f, 0.5f, 0.5f, 0.0f, 0.0f, 1.0f, 1.0f)
                                .Component_Image("cross", Color.white, Image.Type.Sliced, Vector4.zero)
                            .Done
                        .Done
                    .Done
                    .Element("Content")
                        .SetRectTransform(0.0f, 0.0f, 0.0f, -60.0f, 0.5f, 0.5f, 0.0f, 0.0f, 1.0f, 1.0f)
                        .SetHorizontalLayout(new RectOffset(0, 0, 0, 0), 0, TextAnchor.UpperLeft, false, true, true, false, true, false, false)
                        .Element("ContentLeft")
                            .Layout()
                                .FlexibleWidth(1)
                            .Done
                            .Element("Padding")
                                .SetRectTransform(10.0f, 10.0f, -10.0f, -10.0f, 0.5f, 0.5f, 0.0f, 0.0f, 1.0f, 1.0f)
                                .Do(builder =>
                                {
                                    var gameObject = Object.Instantiate(_tool.prefabGridScrollView.Prefab, builder.GameObject.transform);
                                    var grid = gameObject.GetComponentInChildren<GridLayoutGroup>();
                                    if (grid == null) throw new System.Exception("Grid not found.");
                                    folderGridObject = grid.gameObject;
                                    grid.cellSize = new Vector2(80.0f, 80.0f);
                                    grid.padding = new RectOffset(4, 4, 4, 4);
                                    grid.spacing = new Vector2(0.0f, 0.0f);
                                })
                            .Done
                        .Done
                        .Element("ContentRight")
                            .Layout()
                                .MinWidth(132 + 4 + 132 + 4 + 132 + 10)
                                .FlexibleWidth(0)
                            .Done
                            .SetVerticalLayout(new RectOffset(0, 10, 10, 10), 10, TextAnchor.UpperLeft, false, true, true, true, false, false, false)
                            .Element("Icons Row")
                                .Layout()
                                    .MinHeight(132 + 6 + 132)
                                    .FlexibleHeight(0)
                                .Done
                                .Element_Button("Icon 1 Button", _tool.iconBlack.Sprite, Color.white, Vector4.zero, Image.Type.Simple)
                                    .SetRectTransform(0, -270, 270, 0, 0, 1, 0, 1, 0, 1)
                                    .SetOnClick(() => {
                                        folderFrameIconImage.sprite = _tool.iconEmpty.Sprite;
                                        folderFrameIconItemTemplate = null;
                                        FillFolderPreview();
                                        AudioManager.playUISoundEffect(ResourceDB.resourceLinker.audioClip_UIButtonClick);
                                    })
                                    .SetTransitionColors(new Color(0.2f, 0.2f, 0.2f, 1.0f), new Color(0.0f, 0.6f, 1.0f, 1.0f), new Color(0.222f, 0.667f, 1.0f, 1.0f), new Color(0.0f, 0.6f, 1.0f, 1.0f), new Color(0.5f, 0.5f, 0.5f, 1.0f), 1.0f, 0.1f)
                                    .Element("Image")
                                        .SetRectTransform(0, 0, 0, 0, 0.5f, 0.5f, 0, 0, 1, 1)
                                        .Component_Image(_tool.iconEmpty.Sprite, Color.white, Image.Type.Sliced, Vector4.zero)
                                        .Keep(out folderFrameIconImage)
                                    .Done
                                .Done
                                .Element("Preview")
                                    .SetRectTransform(132 + 4 + 132 + 10 + 64 - 50, -(132 + 5 - 60), 132 + 4 + 132 + 10 + 64 - 50, -(132 + 5 - 60), 0, 1, 0, 1, 0, 1)
                                    .SetSizeDelta(100, 120)
                                    .Keep(out folderFramePreviewContainer)
                                .Done
                            .Done
                            .Element("Name Row")
                                .Layout()
                                    .MinHeight(40)
                                    .FlexibleHeight(0)
                                .Done
                                .Do(builder =>
                                {
                                    var gameObject = Object.Instantiate(_tool.prefabBlueprintNameInputField.Prefab, builder.GameObject.transform);
                                    folderFrameNameInputField = gameObject.GetComponentInChildren<TMP_InputField>();
                                    if (folderFrameNameInputField == null) throw new System.Exception("TextMeshPro Input field not found.");
                                    folderFrameNameInputField.text = "";
                                    folderFrameNameInputField.onValueChanged.AddListener(new UnityAction<string>((string value) =>
                                    {
                                        if (folderFramePreviewLabel != null) folderFramePreviewLabel.text = Path.GetFileName(value);
                                    }));
                                    EventSystem.current.SetSelectedGameObject(folderFrameNameInputField.gameObject, null);
                                })
                            .Done
                            .Element("Row Buttons")
                                .Layout()
                                    .MinHeight(40)
                                    .FlexibleHeight(0)
                                .Done
                                .SetHorizontalLayout(new RectOffset(0, 0, 0, 0), 5.0f, TextAnchor.UpperLeft, false, true, true, false, true, false, false)
                                .Element_TextButton("Button Confirm", "Confirm")
                                    .Updater<Button>(_guiUpdaters, () => !string.IsNullOrWhiteSpace(folderFrameNameInputField?.text))
                                    .SetOnClick(() => { ConfirmFolderEdit(relativePath, folderName, folderFrameNameInputField?.text); })
                                .Done
                                .Element_TextButton("Button Cancel", "Cancel")
                                    .SetOnClick(() => Hide())
                                .Done
                            .Done
                        .Done
                    .Done
                .Done
            .End();*/

            FillFolderGrid();

            if (_folderFrameNameInputField != null) _folderFrameNameInputField.text = folderName;

            _folderFrameIconItemTemplate = null;

            if (!string.IsNullOrEmpty(folderName))
            {
                string iconNamePath = Path.Combine(originalPath, "__folder_icon.txt");
                if (File.Exists(iconNamePath))
                {
                    var iconName = File.ReadAllText(iconNamePath).Trim();
                    var iconHash = ItemTemplate.generateStringHash(iconName);
                    _folderFrameIconItemTemplate = ItemTemplateManager.getItemTemplate(iconHash);
                }
            }

            FillFolderPreview();
            FillFolderFrameIcons();

            Shown();

            EventSystem.current.SetSelectedGameObject(_folderFrameNameInputField.gameObject, null);
        }

        private void ConfirmFolderEdit(string relativePath, string originalName, string newName)
        {
            var newPath = Path.Combine(DuplicationerSystem.BlueprintFolder, relativePath, newName);
            var iconNamePath = Path.Combine(newPath, "__folder_icon.txt");
            if (string.IsNullOrWhiteSpace(originalName))
            {
                try
                {
                    Directory.CreateDirectory(newPath);
                    if (Directory.Exists(newPath))
                    {
                        if (_folderFrameIconItemTemplate != null)
                        {
                            File.WriteAllText(iconNamePath, _folderFrameIconItemTemplate.identifier);
                        }

                        Hide();
                        _tool.FillLibraryGrid(relativePath);
                    }
                }
                catch { }
            }
            else
            {
                var originalPath = Path.Combine(DuplicationerSystem.BlueprintFolder, relativePath, originalName);

                try
                {
                    if (originalPath != newPath && Directory.Exists(originalPath) && !Directory.Exists(newPath))
                    {
                        Directory.Move(originalPath, newPath);
                    }

                    if (File.Exists(iconNamePath)) File.Delete(iconNamePath);

                    if (_folderFrameIconItemTemplate != null)
                    {
                        File.WriteAllText(iconNamePath, _folderFrameIconItemTemplate.identifier);
                    }

                    Hide();
                    _tool.FillLibraryGrid(relativePath);
                }
                catch { }
            }
        }

        private void FillFolderGrid()
        {
            if (_folderGridObject == null) return;

            _folderGridObject.transform.DestroyAllChildren();

            foreach (var kv in ItemTemplateManager.getAllItemTemplates())
            {
                var itemTemplate = kv.Value;
                if (itemTemplate.isHiddenItem) continue;

                var gameObject = Object.Instantiate(_tool.prefabBlueprintButtonIcon.Prefab, _folderGridObject.transform);

                var iconImage = gameObject.transform.Find("Icon1")?.GetComponent<Image>();
                if (iconImage != null) iconImage.sprite = itemTemplate.icon;

                var button = gameObject.GetComponentInChildren<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(new UnityAction(() => {
                        AudioManager.playUISoundEffect(ResourceDB.resourceLinker.audioClip_UIButtonClick);
                        _folderFrameIconItemTemplate = itemTemplate;
                        _folderFrameIconImage.sprite = itemTemplate?.icon ?? _tool.iconEmpty.Sprite;
                        FillFolderPreview();
                    }));
                }

                var panel = gameObject.GetComponent<Image>();
                if (panel != null) panel.color = Color.clear;
            }
        }

        private void FillFolderPreview()
        {
            _folderFramePreviewContainer.transform.DestroyAllChildren();
            var gameObject = Object.Instantiate(_tool.prefabBlueprintButtonFolder.Prefab, _folderFramePreviewContainer.transform);
            var deleteButton = gameObject.transform.Find("DeleteButton")?.gameObject;
            if (deleteButton != null) deleteButton.SetActive(false);
            var renameButton = gameObject.transform.Find("RenameButton")?.gameObject;
            if (renameButton != null) renameButton.SetActive(false);
            _folderFramePreviewLabel = gameObject.GetComponentInChildren<TextMeshProUGUI>();
            _folderFramePreviewIconImage = gameObject.transform.Find("Icon1")?.GetComponent<Image>();

            if (_folderFramePreviewLabel != null && _folderFrameNameInputField != null)
            {
                _folderFramePreviewLabel.text = Path.GetFileName(_folderFrameNameInputField.text);
            }

            if (_folderFramePreviewIconImage != null)
            {
                _folderFramePreviewIconImage.sprite = _folderFrameIconItemTemplate?.icon ?? _tool.iconEmpty.Sprite;
            }
        }

        private void FillFolderFrameIcons()
        {
            if (_folderFrameIconImage != null)
            {
                _folderFrameIconImage.sprite = _folderFrameIconItemTemplate?.icon_256 ?? _tool.iconEmpty.Sprite;
            }
        }

        public void OnChange_FolderName(string value)
        {
            if (_folderFramePreviewLabel != null) _folderFramePreviewLabel.text = Path.GetFileName(value);
            _buttonConfirm.interactable = !string.IsNullOrWhiteSpace(value);
        }

        public void OnClick_RemoveIcon()
        {
            _folderFrameIconImage.sprite = _tool.iconEmpty.Sprite;
            _folderFrameIconItemTemplate = null;
            FillFolderPreview();
            AudioManager.playUISoundEffect(ResourceDB.resourceLinker.audioClip_UIButtonClick);
        }

        public void OnClick_Confirm()
        {
            ConfirmFolderEdit(_relativePath, _originalFolderName, _folderFrameNameInputField?.text);
        }
    }
}
