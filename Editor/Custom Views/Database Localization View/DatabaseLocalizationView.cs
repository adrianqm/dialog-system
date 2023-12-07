using System;
using System.Collections;
using System.Collections.Generic;
using AQM.Tools;
using Codice.CM.Common;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.UIElements;
using UnityEngine;
#if LOCALIZATION_EXIST
using UnityEditor.Localization;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
#endif
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

public class DatabaseLocalizationView : VisualElement
{
    public Action onCloseModal;
    public Action onLocalizeCreated;
    private TextField _dbTitle, _dbDesc;
    private VisualElement _bodyContainerVE;
    private VisualElement _noLocalizationVE;
    private Button _installPackageButton;
    private Label _installingLabel;
    private ObjectField _localeOf;
    private ObjectField _stringTableOf;
    static AddRequest LocalizationRequest;
    
    public new class UxmlFactory:  UxmlFactory<DatabaseLocalizationView, DatabaseEditorView.UxmlTraits> {}

    public DatabaseLocalizationView()
    {
        string uriFile = "Assets/dialog-system/Editor/Custom Views/Database Localization View/DatabaseLocalizationView.uxml";
        (EditorGUIUtility.Load(uriFile) as VisualTreeAsset)?.CloneTree(this);
        
        Button closeButton = this.Q<Button>("db-editor-close");
        var texture = EditorGUIUtility.IconContent("winbtn_win_close@2x").image;
        closeButton.focusable = false;
        closeButton.Add(new Image {
            image = texture,
        });
        closeButton.clicked += OnCloseButtonClicked;
        
        SetUpView();
        SetUpSelectors();
        SetUpDefaultSelectorValues();
        SetUpBindButton();
    }

    private void SetUpSelectors()
    {
#if LOCALIZATION_EXIST
        VisualElement localeObjectVe = this.Q("locale-object");
        _localeOf = new ObjectField
        {
            objectType = typeof(Locale)
        };
        localeObjectVe.Add(_localeOf);
        _stringTableOf.RegisterValueChangedCallback(HandleLocaleChange);
        
        VisualElement stringTableVe = this.Q("string-table-object");
        _stringTableOf = new ObjectField
        {
            objectType = typeof(StringTableCollection)
        };
        stringTableVe.Add(_stringTableOf);
        _stringTableOf.RegisterValueChangedCallback(HandleTableChange);
#endif
    }
    
    private void HandleLocaleChange(ChangeEvent<Object> evt)
    {
        if (evt.newValue) _localeOf.RemoveFromClassList("field-object-error");
        else _localeOf.AddToClassList("field-object-error");
    }
    private void HandleTableChange(ChangeEvent<Object> evt)
    {
        if (evt.newValue) _stringTableOf.RemoveFromClassList("field-object-error");
        else _stringTableOf.AddToClassList("field-object-error");
    }

    private void SetUpView()
    {
        _bodyContainerVE = this.Q<VisualElement>("bodyContainer");
        _noLocalizationVE = this.Q<VisualElement>("noLocalization");
        
#if LOCALIZATION_EXIST
        _bodyContainerVE.style.display = DisplayStyle.Flex;
        _noLocalizationVE.style.display = DisplayStyle.None;
#else
        LocalizationNotExist();
#endif
    }

    public void SetUpDefaultSelectorValues()
    {
#if LOCALIZATION_EXIST
        _stringTableOf.value = null;
        if (DSData.instance.tableCollection != null)
        {
            _stringTableOf.value = DSData.instance.tableCollection;
        }
        _localeOf.value = null;
        if (DSData.instance.database?.defaultLocale != null)
        {
            _localeOf.value = DSData.instance.database.defaultLocale;
        }
        
#endif
    }
    
    private void SetUpBindButton()
    {
#if LOCALIZATION_EXIST
        Button createButton = this.Q<Button>("create-localization-button");
        createButton.clickable = new Clickable(() =>
        {
            Locale defaultLocale = _localeOf.value as Locale;
            StringTableCollection tableCollection = _stringTableOf.value as StringTableCollection;
            if (tableCollection != null && defaultLocale)
            {
                
                DSData.instance.tableCollection = tableCollection;
                _stringTableOf.RemoveFromClassList("field-object-error");
                DSData.instance.database.defaultLocale = defaultLocale;
                _localeOf.RemoveFromClassList("field-object-error");
                
                StringTable table = LocalizationSettings.StringDatabase.GetTable(tableCollection.name,defaultLocale);
                DSData.instance.defaultStringTable = table;
                DSData.instance.database.tableCollectionName = tableCollection.TableCollectionName;
                
                while (tableCollection.SharedData.Contains(""))
                {
                    tableCollection.SharedData.RemoveKey("");
                }
                
                // Create entries
                /*foreach (var conversation in DSData.instance.database.conversations)
                {
                    foreach (var node in conversation.nodes)
                    {
                        DialogNode dialogNode = node as DialogNode;
                        if (dialogNode)
                        {
                            table.AddEntry(dialogNode.guid, dialogNode.message);
                        }
                    
                        ChoiceNode choiceNode = node as ChoiceNode;
                        if (choiceNode)
                        {
                            table.AddEntry(choiceNode.guid, choiceNode.message);
                            choiceNode.choices.ForEach(c =>
                            {
                                table.AddEntry(c.guid, c.choiceMessage);
                            });
                        }
                    }
                }*/
                EditorUtility.SetDirty(tableCollection);
                EditorUtility.SetDirty(table);
                EditorUtility.SetDirty(table.SharedData);
                AssetDatabase.SaveAssets();
                onLocalizeCreated?.Invoke();
            }
            else
            {
                if (tableCollection != null) _stringTableOf.RemoveFromClassList("field-object-error");
                else _stringTableOf.AddToClassList("field-object-error");

                if (defaultLocale != null) _localeOf.RemoveFromClassList("field-object-error");
                else _localeOf.AddToClassList("field-object-error");
            }
        });
#endif
    }

    private void InstallProgress()
    {
        if (!LocalizationRequest.IsCompleted) return;
        if (LocalizationRequest.Status == StatusCode.Success)
            Debug.Log("Installed: " + LocalizationRequest.Result.packageId);
        else if (LocalizationRequest.Status >= StatusCode.Failure)
        {
            _installPackageButton.style.display = DisplayStyle.Flex;
            _installingLabel.style.display = DisplayStyle.None;
            Debug.Log(LocalizationRequest.Error.message);
        }
        EditorApplication.update -= InstallProgress;
    }
    
    private void LocalizationNotExist()
    {
        _bodyContainerVE.style.display = DisplayStyle.None;
        _noLocalizationVE.style.display = DisplayStyle.Flex;
        _installingLabel = this.Q<Label>("installing-label");
        _installPackageButton = this.Q<Button>("install-button");
        _installPackageButton.clickable = new Clickable(() =>
        {
            _installPackageButton.style.display = DisplayStyle.None;
            _installingLabel.style.display = DisplayStyle.Flex;
            LocalizationRequest = Client.Add("com.unity.localization");
            EditorApplication.update += InstallProgress;
        });
    }
    
    private void OnCloseButtonClicked()
    {
        onCloseModal?.Invoke();
    }
}

