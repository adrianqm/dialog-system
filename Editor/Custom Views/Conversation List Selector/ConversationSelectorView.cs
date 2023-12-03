using System.Collections;
using System.Collections.Generic;
using AQM.Tools;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ConversationSelectorView : VisualElement
{
    public new class UxmlFactory:  UxmlFactory<ConversationSelectorView, ConversationSelectorView.UxmlTraits> {}
    
    public ConversationSelectorView()
    {
        string uriFile = "Assets/dialog-system/Editor/Custom Views/Conversation List Selector/ConversationSelectorView.uxml";
        (EditorGUIUtility.Load(uriFile) as VisualTreeAsset)?.CloneTree(this);

        SetUpTree();
    }

    private void SetUpTree()
    {
        List<ConversationGroup> groups = new List<ConversationGroup>();
        /*groups.Add(new ConversationGroup("Level 1",DSData.instance.database.conversations, new List<ConversationGroup>(
        {
            new ("Level 1-1", DSData.instance.database.conversations,new List<ConversationGroup>()),
            new ("Level 1-2", DSData.instance.database.conversations,new List<ConversationGroup>()),
        }));*/
        
        /*groups.Add(new ConversationGroup("Level 2",DSData.instance.database.conversations, new List<ConversationGroup>(
        {
            new ("Level 2-1", DSData.instance.database.conversations,new List<ConversationGroup>()),
            new ("Level 2-2", DSData.instance.database.conversations,new List<ConversationGroup>()),
        })));*/
        List<ConversationGroup> groups1 = new List<ConversationGroup>();
        groups1.Add(new ConversationGroup("Level 1-1",new List<ConversationTree>(),new List<ConversationGroup>()));
        groups.Add(new ConversationGroup("Level 1",new List<ConversationTree>(),groups1));
        
        List<ConversationGroup> groups2 = new List<ConversationGroup>();
        groups2.Add(new ConversationGroup("Level 2-1",new List<ConversationTree>(),new List<ConversationGroup>()));
        groups2.Add(new ConversationGroup("Level 2-2",new List<ConversationTree>(),new List<ConversationGroup>()));
        groups.Add(new ConversationGroup("Level 2",new List<ConversationTree>(),groups2));

        List<TreeViewItemData<ConversationGroup>> treeRoots = new List<TreeViewItemData<ConversationGroup>>();
        int id = 0;
        foreach (var group in groups)
        {
            var groupsInGroup = new List<TreeViewItemData<ConversationGroup>>(group.groups.Count);
            foreach (var planet in group.groups)
            {
                groupsInGroup.Add(new TreeViewItemData<ConversationGroup>(id++, planet));
            }

            treeRoots.Add(new TreeViewItemData<ConversationGroup>(id++, group, groupsInGroup));
        }
        
        var treeView = this.Q<MultiColumnTreeView>();
        treeView.SetRootItems(treeRoots);
        
        treeView.columns["groups"].makeCell = () =>
        {
            var ve = new VisualElement();
            ve.AddToClassList("group-ve");
            ve.focusable = true;
            
            var label = new Label();
            label.AddToClassList("group-label");
            ve.Add(label);
            
            var textField = new TextField();
            textField.AddToClassList("group-textfield");
            textField.AddToClassList("hidden");
            textField.focusable = true;
            ve.Add(textField);
            
            return ve;
        };
        treeView.columns["groups"].bindCell = (VisualElement ve, int index) =>
        {
            var label = ve.Q<Label>(className: "group-label");
            var textField = ve.Q<TextField>(className: "group-textfield");
            label.bindingPath = "name";
            label.Bind(new SerializedObject(treeView.GetItemDataForIndex<ConversationGroup>(index)));
            textField.bindingPath = "name";
            textField.Bind(new SerializedObject(treeView.GetItemDataForIndex<ConversationGroup>(index)));
            
            label.RegisterCallback<MouseDownEvent>((evt) =>
            {
                if (evt.clickCount == 2)
                {
                    label.AddToClassList("hidden");
                    textField.RemoveFromClassList("hidden");
                    ve.Focus();
                    textField.Focus();
                    var l = textField.Q<VisualElement>("unity-text-input");
                    l.Focus();
                    textField.SelectAll();
                }
            });
            textField.RegisterCallback<FocusOutEvent>((evt) =>
            {
                textField.AddToClassList("hidden");
                label.RemoveFromClassList("hidden");
            });
        };
    }
}
