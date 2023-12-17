using System;
using System.Collections.Generic;
using System.Linq;
using AQM.Tools;
using AQM.Tools.Serializable;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

#if LOCALIZATION_EXIST
    using UnityEditor.Localization;
    using UnityEngine.Localization.Tables;
#endif

using Edge = UnityEditor.Experimental.GraphView.Edge;
using Node = AQM.Tools.Node;
using Random = UnityEngine.Random;

public class DialogSystemView : GraphView
{
    public Action<NodeView> onNodeSelected;
    public Action onRefreshInspector;
    public Action onNodesRemoved;
    public new class UxmlFactory : UxmlFactory<DialogSystemView, GraphView.UxmlTraits> {}

    private DialogSystemDatabase _currentDatabase;
    private ConversationTree _tree;
    private NodeSearchProvider _nodeSearchProvider;
    private DialogSystemEditor _editorWindow;
    private MiniMap _miniMap;
    private Boolean _canPaste;
    
    public DialogSystemView()
    {
        Insert(0, new GridBackground());
        
        this.AddManipulator(new ContentZoomer());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        
        AddMinimap();
        OnGroupsChanged();
        
        // Import StyleSheets 
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/dialog-system/Editor/DialogSystemEditor.uss");
        styleSheets.Add(styleSheet);

        Undo.undoRedoPerformed += OnUndoRedo;
        #if LOCALIZATION_EXIST
            LocalizationEditorSettings.EditorEvents.TableEntryModified += TableEntryModified;
        #endif
    }
    
    public void SetUpTreeView(DialogSystemDatabase db)
    {
        _currentDatabase = db;
    }
    
    private void OnGroupsChanged()
    {
        elementsAddedToGroup = (group, elements) =>
        {
            foreach (GraphElement element in elements)
            {
                if (!(element is NodeView))
                {
                    continue;
                }

                GroupNodeView groupView = (GroupNodeView) group;
                NodeView nodeView = (NodeView) element;

                ConversationUtils.AddGroupToNode(nodeView.node, groupView.group);
            }
        };
        
        elementsRemovedFromGroup = (group, elements) =>
        {
            foreach (GraphElement element in elements)
            {
                if (!(element is NodeView))
                {
                    continue;
                }

                GroupNodeView groupView = (GroupNodeView) group;
                NodeView nodeView = (NodeView) element;

                ConversationUtils.RemoveGroupFromNode(nodeView.node);
            }
        };
        
        groupTitleChanged = (group, name) =>
        {
            GroupNodeView groupNodeView = (GroupNodeView) group;
            ConversationUtils.SetGroupTitle(groupNodeView.group, name);
        };
    }

    public void SetUpEditorWindow(DialogSystemEditor dialogSystemEditor)
    {
        _editorWindow = dialogSystemEditor;
    }

    private void AddNodeSearchWindow()
    {
        if (_nodeSearchProvider == null)
        {
            _nodeSearchProvider = ScriptableObject.CreateInstance<NodeSearchProvider>();
            _nodeSearchProvider.SetUp(this);
        }

        nodeCreationRequest = context =>
            SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _nodeSearchProvider);
    }

    public void ResetTreeData()
    {
        ResetNodeSearchWindow();
    }
    
    private void ResetNodeSearchWindow()
    {
        nodeCreationRequest = null;
    }
    
    private void AddMinimap()
    {
        _miniMap = new MiniMap(){maxHeight = 137};
        //miniMap.SetPosition(new Rect(0,0,250,200));
        _miniMap.style.position = Position.Absolute;
        _miniMap.style.display = DisplayStyle.None;
        _miniMap.style.right = 0;
        _miniMap.style.bottom = 0;
        _miniMap.style.height = 137;
        _miniMap.style.width = 200;
        
        Add(_miniMap);
    }

    public void ChangeMinimapDisplay()
    {
        var currentStyle = _miniMap.style.display;
        if (currentStyle == DisplayStyle.None)
        {
            _miniMap.style.display = DisplayStyle.Flex;
        }
        else
        {
            _miniMap.style.display = DisplayStyle.None;
        }
    }

    private void OnUndoRedo()
    {
        PopulateView(_tree);
        AssetDatabase.SaveAssets();
    }
    
    public NodeView FindNodeView(Node node)
    {
        return GetNodeByGuid(node.guid) as NodeView;
    }
    
    GroupNodeView FindGroupNodeView(GroupNode node)
    {
        return GetElementByGuid(node.guid) as GroupNodeView;
    }

    internal void PopulateViewAndFrameNodes(ConversationTree tree)
    {
        //EditorApplication.delayCall -= FrameAllNodes;
        PopulateView(tree);
        //EditorApplication.delayCall += FrameAllNodes;
    }

    private void PopulateView(ConversationTree tree)
    {
        if (tree == null) return;
        
        this._tree = tree;

        graphViewChanged -= OnGraphViewChanged;
        serializeGraphElements -= CutCopyOperation;
        unserializeAndPaste -= PasteOperation;
        canPasteSerializedData -= CanPaste;
        _tree.onUpdateViewStates -= OnUpdateStates;
        UnregisterCallback<KeyDownEvent>(DisableRedoAction);
        
        DeleteElements(graphElements);
        
        
        graphViewChanged += OnGraphViewChanged;
        serializeGraphElements += CutCopyOperation;
        unserializeAndPaste += PasteOperation;
        canPasteSerializedData += CanPaste;
        _tree.onUpdateViewStates += OnUpdateStates;
        AddNodeSearchWindow();
        RegisterCallback<KeyDownEvent>(DisableRedoAction);
        
        if (_tree.startNode == null)
        {
            _tree.startNode = ConversationUtils.CreateNode(_currentDatabase,_tree,typeof(StartNode),new Vector2(0f,200f)) as StartNode;
            EditorUtility.SetDirty(tree);
            AssetDatabase.SaveAssets();
        }
        
        if (_tree.completeNode == null)
        {
            _tree.completeNode = ConversationUtils.CreateNode(_currentDatabase,_tree, typeof(CompleteNode), new Vector2(1000f,200f)) as CompleteNode;
            EditorUtility.SetDirty(tree);
            AssetDatabase.SaveAssets();
        }
        
        // Create node Groups
        _tree.groups?.ForEach(n =>
        {
            CreateGroupBoxView(n);
        });
        
        // Create node view
        _tree.nodes.ForEach(n =>
        {
            CreateNodeView(n);
        });
        
        // Create edges
        _tree.nodes.ForEach(n =>
        {
            ParentNode parentNode = n as ParentNode;
            if (parentNode != null)
            {
                NodeView parentView = FindNodeView(n);
                CreateEdgeView(_tree.GetChildren(parentNode),parentView.output);
            }
            
            ChoiceNode choiceNode = n as ChoiceNode;
            if (choiceNode != null)
            {
                NodeView nodeView = FindNodeView(n);
                foreach(KeyValuePair<Port, Choice> entry in nodeView.choicePortTranslationMap)
                {
                    Port choicePort = entry.Key;
                    CreateEdgeView(entry.Value.children,choicePort);
                }
                // Default Choice Edges
                CreateEdgeView(choiceNode.defaultChildren, nodeView.output);
            }
        });
        
        OnUpdateStates();
        onRefreshInspector?.Invoke();
    }

    private void CreateEdgeView(List<Node> children, Port port)
    {
        children.ForEach(child =>
        {
            NodeView childView = FindNodeView(child);
            if (childView != null)
            {
                Edge edge = port.ConnectTo(childView.input);
                AddElement(edge);
            }
        }); 
    }

    public void FrameAllNodes()
    {
        FrameAll();
    }
    
    private void DisableRedoAction(KeyDownEvent evt)
    {
        if (evt.modifiers == EventModifiers.Control && evt.keyCode == KeyCode.Y)
        {
            //commandInvoker.Redo();

            evt.StopImmediatePropagation();
            evt.PreventDefault();
        }
    }

    private string CutCopyOperation(IEnumerable<GraphElement> elements)
    {
        CutCopySerializedData cutCopyData =  GeneralUtils.GenerateCutCopyObject(elements);
        _canPaste = cutCopyData.choiceNodesToCopy.Count > 0 || cutCopyData.dialogNodesToCopy.Count > 0 || 
                    cutCopyData.groupNodesToCopy.Count > 0;
        //string jsonStr = JsonUtility.ToJson(cutCopyData,);
        string jsonStr = JsonConvert.SerializeObject(cutCopyData, Formatting.Indented);
        return jsonStr;
    }
    
    private void PasteOperation(string operationName, string data)
    {
        CutCopySerializedData cutCopyData = JsonConvert.DeserializeObject<CutCopySerializedData>(data);
        ClearSelection();
        Dictionary<string, NodeView> nodeTranslationMap = new Dictionary<string, NodeView>();
        Dictionary<string, Choice> choiceTranslationMap = new Dictionary<string, Choice>();
        Dictionary<string, GroupNodeView> groupTranslationMap = new Dictionary<string, GroupNodeView>();
        Vector2 offset = new Vector2(Random.Range(-15, 15), Random.Range(-175, -225));
        
        // Create Groups
        foreach (SerializableGroupNode originalGroupNode in cutCopyData.groupNodesToCopy)
        {
            GroupNodeView groupNodeView = 
                CreateGroupBox(
                    originalGroupNode.position.ToVector2() - offset,
                    originalGroupNode.title);
            groupTranslationMap.Add(originalGroupNode.guid, groupNodeView);
            AddToSelection(groupNodeView);
        }
        
        // Create DialogNode
        foreach (SerializableDialogNode serializedDialogNode in cutCopyData.dialogNodesToCopy)
        {
            NodeView nodeView = 
                CreateDialogNodeCopy(
                    typeof(DialogNode), 
                    serializedDialogNode.position.ToVector2() - offset,serializedDialogNode);
            nodeTranslationMap.Add(serializedDialogNode.guid,nodeView);
            AddToSelection(nodeView);

            if (serializedDialogNode.group != null)
            {
                GroupNodeView groupNodeView = groupTranslationMap[serializedDialogNode.group.guid];
                if (groupNodeView != null)
                {
                    bool hasBeenAAdded = ConversationUtils.AddGroupToNode(nodeView.node, groupNodeView.group);
                    if (hasBeenAAdded)
                    {
                        groupNodeView.AddElement(nodeView);
                    }
                }
            }
        }
        
        // Create Choice Node
        foreach (SerializableChoiceNode serializableChoiceNode in cutCopyData.choiceNodesToCopy)
        {
            NodeView nodeView = 
                CreateChoiceNodeCopy(
                    serializableChoiceNode.position.ToVector2() - offset,serializableChoiceNode,choiceTranslationMap);
            nodeTranslationMap.Add(serializableChoiceNode.guid,nodeView);
            AddToSelection(nodeView);
            

            if (serializableChoiceNode.group != null)
            {
                if (!groupTranslationMap.TryGetValue(serializableChoiceNode.group.guid, out GroupNodeView groupNodeView)) continue;
                bool hasBeenAAdded = ConversationUtils.AddGroupToNode(nodeView.node, groupNodeView.group);
                if (hasBeenAAdded)
                {
                    groupNodeView.AddElement(nodeView);
                }
            }
        }
#if LOCALIZATION_EXIST
        LocalizationUtils.RefreshStringTableCollection(DSData.instance.tableCollection);
#endif
        
        // Create Dialog Edges
        foreach (SerializableDialogNode serializedDialogNode in cutCopyData.dialogNodesToCopy)
        {
            NodeView parentView = nodeTranslationMap[serializedDialogNode.guid];
            foreach (SerializableNodeChild childOriginalNode in serializedDialogNode.children)
            {
                if (!nodeTranslationMap.TryGetValue(childOriginalNode.guid, out NodeView childView)) continue;
                bool hasAdded = ConversationUtils.AddChild(parentView.node, childView.node);
                if (hasAdded)
                {
                    Edge edge = parentView.output.ConnectTo(childView.input);
                    AddElement(edge);
                    AddToSelection(edge);
                }
            }
        }
        
        // Create Choice Edges
        foreach (SerializableChoiceNode serializableChoiceNode in cutCopyData.choiceNodesToCopy )
        {
            NodeView parentView = nodeTranslationMap[serializableChoiceNode.guid];
            foreach (var choice in serializableChoiceNode.choices)
            {
                foreach (SerializableNodeChild childOriginalNode in choice.children)
                {
                    if (!nodeTranslationMap.TryGetValue(childOriginalNode.guid, out NodeView childView)) continue;
                    if (!choiceTranslationMap.TryGetValue(choice.guid, out Choice newChoice)) continue;
                    Port choicePort = parentView.choicePortTranslationMap.FirstOrDefault(x => x.Value.guid == newChoice.guid).Key;
                    bool hasAdded = ConversationUtils.AddChild(parentView.node, childView.node, choicePort);
                    if (hasAdded)
                    {
                        Edge edge = choicePort.ConnectTo(childView.input);
                        AddElement(edge);
                        AddToSelection(edge);
                    } 
                } 
            }
           
        }
    }
    
    private bool CanPaste(string data)
    {
        return _canPaste;
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.ToList().Where(endPort =>
            endPort.direction != startPort.direction && 
            endPort.node != startPort.node
        ).ToList();
    }

    private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
    {
        if (graphViewChange.elementsToRemove != null)
        {
            // To prevent edges that has to be removed
            List<Edge> edgesToRemoveLater = new List<Edge>();
            foreach (GraphElement element in graphViewChange.elementsToRemove)
            {
                NodeView nodeView = element as NodeView;
                if (nodeView != null)
                {
                    nodeView.DeleteAndGetAllChoiceEdges().ForEach((edge) =>{edgesToRemoveLater.Add(edge);});
                }
            }
            
            foreach (var edge in edgesToRemoveLater)
            {
                graphViewChange.elementsToRemove.Add(edge);
            }

            bool nodeHasBeenRemoved = false;
#if LOCALIZATION_EXIST
            List<string> keysDeleted = new List<string>();
#endif
            // Delete elements
            foreach (GraphElement element in graphViewChange.elementsToRemove)
            {
                NodeView nodeView = element as NodeView;
                if (nodeView != null)
                {
#if LOCALIZATION_EXIST
                    keysDeleted.Add(nodeView.node.guid);
#endif
                    DeleteNode(nodeView.node);
                    nodeHasBeenRemoved = true;
                    continue;
                }

                Edge edge = element as Edge;
                if (edge != null)
                {
                    NodeView  parentView = edge.output.node as NodeView;
                    NodeView  childView = edge.input.node as NodeView;
                    ConversationUtils.RemoveChild(parentView?.node, childView?.node, edge.output);
                    continue;
                }
                
                GroupNodeView groupNodeView = element as GroupNodeView;
                if (groupNodeView != null)
                {
                    DeleteGroupNode(groupNodeView.group);
                }
            }
            if(nodeHasBeenRemoved) onNodesRemoved?.Invoke();
#if LOCALIZATION_EXIST
            if(keysDeleted.Count > 0) LocalizationUtils.RemoveKeysFromCollection(keysDeleted);
#endif
        }
        
        if (graphViewChange.edgesToCreate != null)
        {
            // To prevent edges that hasn't added
            List<Edge> edgesNotAdded = new List<Edge>();
            
            foreach (Edge edge in graphViewChange.edgesToCreate)
            {
                NodeView  parentView = edge.output.node as NodeView;
                NodeView  childView = edge.input.node as NodeView;
                bool hasAdded = ConversationUtils.AddChild(parentView?.node, childView?.node,edge.output);
                if(!hasAdded) edgesNotAdded.Add(edge);
            }

            foreach (Edge edge in edgesNotAdded)
            {
                graphViewChange.edgesToCreate.Remove(edge);
            }
        }

        if (graphViewChange.movedElements != null)
        {
            nodes.ForEach((n) =>
            {
                NodeView view = n as NodeView;
                view.SortChildren();
            });
        }
        return graphViewChange;
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        var pos = MouseToContent(evt.mousePosition,false);
        {
            var types = TypeCache.GetTypesDerivedFrom<Node>();
            foreach (var type in types)
            {
                if (type != typeof(StartNode) && type != typeof(CompleteNode) && type != typeof(ParentNode))
                {
                    evt.menu.AppendAction("Create "+type.Name+" Node", (a)=> CreateNode(type,pos));
                }
            }
            evt.menu.AppendAction("Create Group Box", actionEvent => CreateGroupBox(pos));
            evt.menu.AppendSeparator();
        }
        base.BuildContextualMenu(evt);
    }

    public Vector2 MouseToContent(Vector2 mousePosition, bool isSearchWindow)
    {
        Vector2 worldMousePosition = mousePosition;

        if (isSearchWindow)
        {
            worldMousePosition = _editorWindow
                .rootVisualElement.ChangeCoordinatesTo(
                    _editorWindow.rootVisualElement.parent, mousePosition - _editorWindow.position.position);
        }

        Vector2 localMousePosition = contentViewContainer.WorldToLocal(worldMousePosition);
        return localMousePosition;
    }
    
    public GroupNodeView CreateGroupBox(Vector2 pos, string title = "Group Box")
    {
        GroupNode groupNode = ConversationUtils.CreateGroup(_currentDatabase, _tree, title, pos);
        return CreateGroupBoxView(groupNode);
    }
    
    private GroupNodeView CreateGroupBoxView(GroupNode groupNode)
    {
        if (groupNode == null) return null;
        
        GroupNodeView groupNodeView = new GroupNodeView(groupNode);
        AddElement(groupNodeView);
        foreach (var selectable  in selection)
        {
            var selectedElement = (GraphElement) selectable;
            if (!(selectedElement is NodeView))
            {
                continue;
            }

            NodeView node = (NodeView) selectedElement;
            groupNodeView.AddElement(node);
        }

        return groupNodeView;
    }
    
    private void DeleteGroupNode(GroupNode groupNode)
    {
        ConversationUtils.DeleteGroupNode(_tree,groupNode);
    }

    public void CreateNode(Type type,Vector2 position)
    {
        Node node = ConversationUtils.CreateNode(_currentDatabase,_tree,type,position);
        CreateNodeView(node);
    }
    private NodeView CreateDialogNodeCopy(Type type,Vector2 position, SerializableDialogNode nodeToCopy)
    {
        Node node = ConversationUtils.CreateDialogNodeCopy(_currentDatabase, _tree, type, position, nodeToCopy);
        return CreateNodeView(node);
    }
    
    private NodeView CreateChoiceNodeCopy(Vector2 position, SerializableChoiceNode nodeToCopy, Dictionary<string, Choice> choiceMap)
    {
        Node node = ConversationUtils.CreateChoiceNodeToCopy(_currentDatabase, _tree, position, nodeToCopy, choiceMap);
        return CreateNodeView(node);
    }

    NodeView CreateNodeView(Node node)
    {
        if (node == null) return null;
        
        NodeView nodeView = new NodeView(node,this, ClearSelection);
        nodeView.onNodeSelected = onNodeSelected;
        nodeView.onRefreshInspector = onRefreshInspector;
        AddElement(nodeView);
        
        if (node.group)
        {
            GroupNodeView groupNodeView = FindGroupNodeView(node.group);
            groupNodeView.AddElement(nodeView);
        }

        return nodeView;
    }
    private void DeleteNode(Node node)
    {
        ConversationUtils.DeleteNode(_tree,node);
        
        // In case that suppress button works
        if (node is StartNode)
        {
            _tree.startNode = null;
        }
    }

    private void OnUpdateStates()
    {
        nodes.ForEach(n =>
        {
            NodeView view = n as NodeView;
            view.UpdateState();
        });
    }

    private void TableEntryModified(SharedTableData.SharedTableEntry tableEntry)
    {
        if(_tree == null) return;
        string entryGuid = tableEntry.Key;
        Node node = _tree.nodes.Find(n =>
        {
            if (n.guid == entryGuid) return true;
            if (n is not ChoiceNode choiceNode) return false;
            return choiceNode.choices.Find(c => c.guid == entryGuid);
        });
        if(!node) return;
        if (GetNodeByGuid(node.guid) is NodeView nodeView)
        {
            nodeView.UpdateLocalizedMessage();
        }
    }

    public void ClearGraph()
    {
        graphViewChanged -= OnGraphViewChanged;
        DeleteElements(graphElements);
    }

    public DialogSystemDatabase GetDatabase()
    {
        return _currentDatabase;
    }
    
}

