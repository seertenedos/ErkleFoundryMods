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
