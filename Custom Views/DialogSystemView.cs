using System;
using System.Collections.Generic;
using System.Linq;
using AQM.Tools.Serializable;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Random = UnityEngine.Random;
public class DialogSystemView : GraphView
{
    public Action<NodeView> OnNodeSelected;
    public Action onNodesRemoved;
    public new class UxmlFactory : UxmlFactory<DialogSystemView, GraphView.UxmlTraits> {}
    
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
        AddNodeSearchWindow();
        AddMinimap();
        OnGroupsChanged();
        
        // Import StyleSheets 
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/dialog-system/DialogSystemEditor.uss");
        styleSheets.Add(styleSheet);

        Undo.undoRedoPerformed += OnUndoRedo;
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

                _tree.AddGroupToNode(nodeView.node, groupView.group);
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

                _tree.RemoveGroupFromNode(nodeView.node);
            }
        };
        
        groupTitleChanged = (group, name) =>
        {
            GroupNodeView groupNodeView = (GroupNodeView) group;
            _tree.SetGroupTitle(groupNodeView.group, name);
        };
    }

    public void SetUpEditorWindor(DialogSystemEditor dialogSystemEditor)
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
    
    NodeView FindNodeView(Node node)
    {
        return GetNodeByGuid(node.guid) as NodeView;
    }
    
    GroupNodeView FindGroupNodeView(GroupNode node)
    {
        return GetElementByGuid(node.guid) as GroupNodeView;
    }

    internal void PopulateViewAndFrameNodes(ConversationTree tree)
    {
        EditorApplication.delayCall -= FrameAllNodes;
        PopulateView(tree);
        EditorApplication.delayCall += FrameAllNodes;
    }

    internal void PopulateView(ConversationTree tree)
    {
        if (tree == null) return;
        
        this._tree = tree;

        graphViewChanged -= OnGraphViewChanged;
        serializeGraphElements -= CutCopyOperation;
        unserializeAndPaste -= PasteOperation;
        canPasteSerializedData -= CanPaste;
        
        DeleteElements(graphElements);
        
        graphViewChanged += OnGraphViewChanged;
        serializeGraphElements += CutCopyOperation;
        unserializeAndPaste += PasteOperation;
        canPasteSerializedData += CanPaste;
        
        if (_tree.rootNode == null)
        {
            _tree.rootNode = _tree.CreateNode(typeof(RootNode), Vector2.zero) as RootNode;
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
            var children = _tree.GetChildren(n);
            NodeView parentView = FindNodeView(n);
            children.ForEach(c =>
            {
                NodeView childView = FindNodeView(c);

                if (childView != null)
                {
                    Edge edge = parentView.output.ConnectTo(childView.input);
                    AddElement(edge);
                }
            });
        });
    }

    public void FrameAllNodes()
    {
        FrameAll();
    }

    private string CutCopyOperation(IEnumerable<GraphElement> elements)
    {
        CutCopySerializedData cutCopyData = new CutCopySerializedData();
        
        foreach (GraphElement n in elements)
        {
            NodeView nodeView = n as NodeView;
            if(nodeView != null) 
            {
                
                cutCopyData.dialogNodesToCopy.Add(GeneralUtils.ConvertToSerializableNode(nodeView.node));
                continue;
            }
            
            GroupNodeView groupView = n as GroupNodeView;
            if(groupView != null)
            {
                cutCopyData.groupNodesToCopy.Add(GeneralUtils.ConvertToSerializableGroupNode(groupView.group));
            }
        }
        
        _canPaste = cutCopyData.dialogNodesToCopy.Count > 0 || cutCopyData.groupNodesToCopy.Count > 0;
        //string jsonStr = JsonUtility.ToJson(cutCopyData,);
        string jsonStr = JsonConvert.SerializeObject(cutCopyData, Formatting.Indented);
        return jsonStr;
    }
    
    private void PasteOperation(string operationName, string data)
    {
        CutCopySerializedData cutCopyData = JsonConvert.DeserializeObject<CutCopySerializedData>(data);
        ClearSelection();
        Dictionary<string, NodeView> nodeTranslationMap = new Dictionary<string, NodeView>();
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
                    bool hasBeenAAdded = _tree.AddGroupToNode(nodeView.node, groupNodeView.group);
                    if (hasBeenAAdded)
                    {
                        groupNodeView.AddElement(nodeView);
                    }
                }
            }
        }
        
        // Create Edges
        foreach (SerializableDialogNode serializedDialogNode in cutCopyData.dialogNodesToCopy)
        {
            NodeView parentView = nodeTranslationMap[serializedDialogNode.guid];
            foreach (SerializableNodeChild childOriginalNode in serializedDialogNode.children)
            {
                NodeView childView = nodeTranslationMap[childOriginalNode.guid];
                if (childView != null)
                {
                    bool hasAdded = _tree.AddChild(parentView.node, childView.node);
                    if (hasAdded)
                    {
                        Edge edge = parentView.output.ConnectTo(childView.input);
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
            foreach (GraphElement element in graphViewChange.elementsToRemove)
            {
                NodeView nodeView = element as NodeView;
                if (nodeView != null)
                {
                    DeleteNode(nodeView.node);
                    continue;
                }

                Edge edge = element as Edge;
                if (edge != null)
                {
                    NodeView  parentView = edge.output.node as NodeView;
                    NodeView  childView = edge.input.node as NodeView;
                    _tree.RemoveChild(parentView.node, childView.node);
                    continue;
                }
                
                GroupNodeView groupNodeView = element as GroupNodeView;
                if (groupNodeView != null)
                {
                    DeleteGroupNode(groupNodeView.group);
                }
            }
            
            onNodesRemoved?.Invoke();
        }
        
        if (graphViewChange.edgesToCreate != null)
        {
            // To prevent edges that hasn't added
            List<Edge> edgesNotAdded = new List<Edge>();
            
            foreach (Edge edge in graphViewChange.edgesToCreate)
            {
                NodeView  parentView = edge.output.node as NodeView;
                NodeView  childView = edge.input.node as NodeView;
                bool hasAdded = _tree.AddChild(parentView.node, childView.node);
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
                if (type != typeof(RootNode))
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
        GroupNode groupNode = _tree.CreateGroup(title, pos);
        return CreateGroupBoxView(groupNode);
    }
    
    private GroupNodeView CreateGroupBoxView(GroupNode groupNode)
    {
        if (groupNode == null) return null;
        
        GroupNodeView groupNodeView = new GroupNodeView(groupNode);
        AddElement(groupNodeView);
        foreach (GraphElement selectedElement  in selection)
        {
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
        _tree.DeteleGroupNode(groupNode);
    }

    public void CreateNode(Type type,Vector2 position)
    {
        Node node = _tree.CreateNode(type,position);
        CreateNodeView(node);
    }
    private NodeView CreateDialogNodeCopy(Type type,Vector2 position, SerializableDialogNode nodeToCopy)
    {
        Node node = _tree.CreateDialogNodeCopy(type,position,nodeToCopy);
        return CreateNodeView(node);
    }

    NodeView CreateNodeView(Node node)
    {
        if (node == null) return null;
        
        NodeView nodeView = new NodeView(node,this);
        nodeView.onNodeSelected = OnNodeSelected;
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
        _tree.DeteleNode(node);
        
        // In case that suppress button works
        if (node is RootNode)
        {
            _tree.rootNode = null;
        }
    }

    public void UpdateNodeStates()
    {
        nodes.ForEach(n =>
        {
            NodeView view = n as NodeView;
            view.UpdateState();
        });
    }

    public void ClearGraph()
    {
        graphViewChanged -= OnGraphViewChanged;
        DeleteElements(graphElements);
    }
    
}
