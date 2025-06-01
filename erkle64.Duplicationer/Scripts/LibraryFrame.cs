using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unfoundry;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Duplicationer
{
    internal class LibraryFrame : DuplicationerFrame
    {
        [Header("Library Frame")]
        [SerializeField] private UIHeaderBar _libraryFrameHeading = null;
        [SerializeField] private GameObject _libraryGridObject = null;

        private string _lastLibraryRelativePath = "";

        public void Show(SaveFrame saveFrame = null)
        {
            if (IsOpen) return;

            if (saveFrame == null) _tool.HideSaveFrame(true);

            _tool.HideBlueprintFrame(true);
            _tool.HideFolderFrame(true);

            FillLibraryGrid(_lastLibraryRelativePath, saveFrame);

            Shown();
        }

        internal void FillLibraryGrid(string relativePath, SaveFrame saveFrame = null)
        {
            if (_libraryGridObject == null) return;

            _lastLibraryRelativePath = relativePath;

            _libraryGridObject.transform.DestroyAllChildren();

            _libraryFrameHeading.setText(string.IsNullOrEmpty(relativePath) ? "Duplicationer - Blueprints" : $"Duplicationer - Blueprints\\{relativePath}");

            var prefabs = new GameObject[5]
            {
                _tool.prefabBlueprintButtonDefaultIcon.Prefab, _tool.prefabBlueprintButton1Icon.Prefab, _tool.prefabBlueprintButton2Icon.Prefab, _tool.prefabBlueprintButton3Icon.Prefab, _tool.prefabBlueprintButton4Icon.Prefab
            };

            if (!string.IsNullOrEmpty(relativePath))
            {
                var backGameObject = Object.Instantiate(_tool.prefabBlueprintButtonFolderBack.Prefab, _libraryGridObject.transform);
                var backButton = backGameObject.GetComponentInChildren<Button>();
                if (backButton != null)
                {
                    backButton.onClick.AddListener(new UnityAction(() =>
                    {
                        AudioManager.playUISoundEffect(ResourceDB.resourceLinker.audioClip_UIButtonClick);

                        var backPath = Path.GetDirectoryName(relativePath);
                        FillLibraryGrid(backPath, saveFrame);
                    }));
                }
            }

            if (saveFrame == null)
            {
                var newFolderGameObject = Object.Instantiate(_tool.prefabBlueprintButtonFolderNew.Prefab, _libraryGridObject.transform);
                var newFolderButton = newFolderGameObject.GetComponentInChildren<Button>();
                if (newFolderButton != null)
                {
                    newFolderButton.onClick.AddListener(new UnityAction(() =>
                    {
                        _tool.ShowFolderFrame(relativePath, "");
                    }));
                }
            }

            foreach (var path in Directory.GetDirectories(Path.Combine(DuplicationerSystem.BlueprintFolder, relativePath)))
            {
                var name = Path.GetFileName(path);

                var gameObject = Object.Instantiate(_tool.prefabBlueprintButtonFolder.Prefab, _libraryGridObject.transform);

                var label = gameObject.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
                if (label != null) label.text = name;

                ItemTemplate iconItemTemplate = null;
                var iconNamePath = Path.Combine(path, "__folder_icon.txt");
                if (File.Exists(iconNamePath))
                {
                    var identifier = File.ReadAllText(iconNamePath).Trim();
                    var hash = ItemTemplate.generateStringHash(identifier);
                    iconItemTemplate = ItemTemplateManager.getItemTemplate(hash);
                }

                var iconImage = gameObject.transform.Find("Icon1")?.GetComponent<Image>();
                if (iconImage != null)
                {
                    iconImage.sprite = iconItemTemplate?.icon ?? _tool.iconEmpty.Sprite;
                }

                var button = gameObject.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(new UnityAction(() =>
                    {
                        ActionManager.AddQueuedEvent(() =>
                        {
                            AudioManager.playUISoundEffect(ResourceDB.resourceLinker.audioClip_UIButtonClick);

                            FillLibraryGrid(Path.Combine(relativePath, name), saveFrame);
                        });
                    }));
                }

                var deleteButton = gameObject.transform.Find("DeleteButton")?.GetComponent<Button>();
                if (deleteButton != null)
                {
                    if (saveFrame != null)
                    {
                        deleteButton.gameObject.SetActive(false);
                    }
                    else
                    {
                        var nameToDelete = name;
                        var pathToDelete = path;
                        deleteButton.onClick.AddListener(new UnityAction(() =>
                        {
                            ActionManager.AddQueuedEvent(() =>
                            {
                                ConfirmationFrame.Show($"Delete folder '{name}'", "Delete", () =>
                                {
                                    try
                                    {
                                        Directory.Delete(pathToDelete, true);
                                        FillLibraryGrid(relativePath);
                                    }
                                    catch (System.Exception) { }
                                });
                            });
                        }));
                    }

                    var renameButton = gameObject.transform.Find("RenameButton")?.GetComponent<Button>();
                    if (renameButton != null)
                    {
                        if (saveFrame != null)
                        {
                            renameButton.gameObject.SetActive(false);
                        }
                        else
                        {
                            var nameToRename = name;
                            var pathToRename = path;
                            renameButton.onClick.AddListener(new UnityAction(() =>
                            {
                                ActionManager.AddQueuedEvent(() =>
                                {
                                    _tool.ShowFolderFrame(relativePath, nameToRename);
                                });
                            }));
                        }
                    }
                }
            }

            foreach (var path in Directory.GetFiles(Path.Combine(DuplicationerSystem.BlueprintFolder, relativePath), $"*.{DuplicationerSystem.BLUEPRINT_EXTENSION}"))
            {
                try
                {
                    if (Blueprint.TryLoadFileHeader(path, out var header, out var name))
                    {
                        var iconItemTemplates = new List<ItemElementTemplate>();
                        if (!string.IsNullOrEmpty(header.icon1))
                        {
                            var template = ItemElementTemplate.Get(header.icon1);
                            if (template.isValid && template.icon != null) iconItemTemplates.Add(template);
                        }
                        if (!string.IsNullOrEmpty(header.icon2))
                        {
                            var template = ItemElementTemplate.Get(header.icon2);
                            if (template.isValid && template.icon != null) iconItemTemplates.Add(template);
                        }
                        if (!string.IsNullOrEmpty(header.icon3))
                        {
                            var template = ItemElementTemplate.Get(header.icon3);
                            if (template.isValid && template.icon != null) iconItemTemplates.Add(template);
                        }
                        if (!string.IsNullOrEmpty(header.icon4))
                        {
                            var template = ItemElementTemplate.Get(header.icon4);
                            if (template.isValid && template.icon != null) iconItemTemplates.Add(template);
                        }

                        int iconCount = iconItemTemplates.Count;

                        var gameObject = Object.Instantiate(prefabs[iconCount], _libraryGridObject.transform);

                        var label = gameObject.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
                        if (label != null) label.text = name;

                        var iconImages = new Image[] {
                        gameObject.transform.Find("Icon1")?.GetComponent<Image>(),
                        gameObject.transform.Find("Icon2")?.GetComponent<Image>(),
                        gameObject.transform.Find("Icon3")?.GetComponent<Image>(),
                        gameObject.transform.Find("Icon4")?.GetComponent<Image>()
                    };

                        for (int iconIndex = 0; iconIndex < iconCount; iconIndex++)
                        {
                            iconImages[iconIndex].sprite = iconItemTemplates[iconIndex].icon;
                        }

                        var button = gameObject.GetComponent<Button>();
                        if (button != null)
                        {
                            if (saveFrame != null)
                            {
                                var nameForSaveInfo = Path.Combine(relativePath, Path.GetFileNameWithoutExtension(path));
                                button.onClick.AddListener(new UnityAction(() =>
                                {
                                    ActionManager.AddQueuedEvent(() =>
                                    {
                                        saveFrame.BlueprintName = nameForSaveInfo;
                                        saveFrame.IconCount = iconCount;
                                        for (int i = 0; i < 4; i++)
                                        {
                                            saveFrame.IconItemTemplates[i] = (i < iconCount) ? iconItemTemplates[i] : ItemElementTemplate.Empty;
                                        }
                                        saveFrame.FillSaveFrameIcons();
                                        saveFrame.FillSavePreview();
                                        Hide();
                                    });
                                }));
                            }
                            else
                            {
                                button.onClick.AddListener(new UnityAction(() =>
                                {
                                    ActionManager.AddQueuedEvent(() =>
                                    {
                                        _tool.ClearBlueprintPlaceholders();
                                        _tool.LoadBlueprintFromFile(path);
                                        if (_tool.CurrentBlueprint != null)
                                        {
                                            _tool.SelectMode(_tool.modePlace);
                                            Hide();
                                        }
                                        else
                                        {
                                            MessageBox.showBox("Error", "Failed to decompress blueprint data");
                                            _tool.SelectMode(_tool.modeSelectArea);
                                        }
                                    });
                                }));
                            }
                        }

                        var deleteButton = gameObject.transform.Find("DeleteButton")?.GetComponent<Button>();
                        if (deleteButton != null)
                        {
                            if (saveFrame != null)
                            {
                                deleteButton.gameObject.SetActive(false);
                            }
                            else
                            {
                                var nameToDelete = name;
                                var pathToDelete = path;
                                deleteButton.onClick.AddListener(new UnityAction(() =>
                                {
                                    ActionManager.AddQueuedEvent(() =>
                                    {
                                        ConfirmationFrame.Show($"Delete '{name}'", "Delete", () =>
                                        {
                                            try
                                            {
                                                File.Delete(pathToDelete);
                                                FillLibraryGrid(relativePath);
                                            }
                                            catch (System.Exception) { }
                                        });
                                    });
                                }));
                            }

                            var renameButton = gameObject.transform.Find("RenameButton")?.GetComponent<Button>();
                            if (renameButton != null)
                            {
                                if (saveFrame != null)
                                {
                                    renameButton.gameObject.SetActive(false);
                                }
                                else
                                {
                                    var nameToRename = name;
                                    var pathToRename = path;
                                    renameButton.onClick.AddListener(new UnityAction(() =>
                                    {
                                        ActionManager.AddQueuedEvent(() =>
                                        {
                                            TextEntryFrame.Show($"Rename Blueprint", nameToRename, "Rename", (string newName) =>
                                            {
                                                string filenameBase = Path.Combine(Path.GetDirectoryName(newName), PathHelpers.MakeValidFileName(Path.GetFileName(newName)));
                                                string newPath = Path.Combine(DuplicationerSystem.BlueprintFolder, relativePath, $"{filenameBase}.{DuplicationerSystem.BLUEPRINT_EXTENSION}");
                                                if (File.Exists(newPath))
                                                {
                                                    ConfirmationFrame.Show($"Overwrite '{newName}'?", "Overwrite", () =>
                                                    {
                                                        try
                                                        {
                                                            DuplicationerSystem.log.Log($"Renaming blueprint '{nameToRename}' to '{newName}'");
                                                            File.Delete(newPath);
                                                            File.Move(pathToRename, newPath);
                                                            RenameBlueprint(newPath, Path.GetFileName(newName));
                                                            FillLibraryGrid(relativePath);
                                                        }
                                                        catch (System.Exception) { }
                                                    });
                                                }
                                                else
                                                {
                                                    try
                                                    {
                                                        DuplicationerSystem.log.Log($"Renaming blueprint '{nameToRename}' to '{newName}'");
                                                        File.Move(pathToRename, newPath);
                                                        RenameBlueprint(newPath, Path.GetFileName(newName));
                                                        FillLibraryGrid(relativePath);
                                                    }
                                                    catch (System.Exception) { }
                                                }
                                            });
                                        });
                                    }));
                                }
                            }
                        }
                    }
                }
                catch(System.Exception ex)
                {
                    DuplicationerSystem.log.LogWarning($"Error loading blueprint info from '{path}': {ex}");
                }
            }
        }

        private void RenameBlueprint(string path, string name)
        {
            var iconItemElementTemplates = new ItemElementTemplate[4];
            var reader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read));

            var magic = reader.ReadUInt32();
            var version = reader.ReadUInt32();

            if (version < 4)
            {
                for (int i = 0; i < 4; i++)
                {
                    var iconItemTemplateId = reader.ReadUInt64();
                    if (iconItemTemplateId != 0)
                    {
                        var template = ItemTemplateManager.getItemTemplate(iconItemTemplateId);
                        if (template != null)
                        {
                            iconItemElementTemplates[i] = new ItemElementTemplate(template);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    var iconItemTemplateIdentifier = reader.ReadString();
                    if (!string.IsNullOrEmpty(iconItemTemplateIdentifier))
                    {
                        var template = ItemElementTemplate.Get(iconItemTemplateIdentifier);
                        if (template.isValid)
                        {
                            iconItemElementTemplates[i] = template;
                        }
                    }
                }
            }

            var oldName = reader.ReadString();

            var data = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));

            reader.Close();
            reader.Dispose();

            var writer = new BinaryWriter(new FileStream(path, FileMode.Create, FileAccess.Write));

            writer.Write(magic);
            writer.Write(version);

            for (int i = 0; i < 4; i++)
            {
                writer.Write(iconItemElementTemplates[i].fullIdentifier);
            }

            writer.Write(name);

            writer.Write(data);

            writer.Close();
            writer.Dispose();
        }
    }
}
