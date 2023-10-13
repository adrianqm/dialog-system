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
        AddNodeSearchWindow();
        AddMinimap();
        OnGroupsChanged();
        
        // Import StyleSheets 
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/dialog-system/DialogSystemEditor.uss");
        styleSheets.Add(styleSheet);

        Undo.undoRedoPerformed += OnUndoRedo;
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
        //EditorApplication.delayCall -= FrameAllNodes;
        PopulateView(tree);
        //EditorApplication.delayCall += FrameAllNodes;
    }

    internal void PopulateView(ConversationTree tree)
    {
        if (tree == null) return;
        
        this._tree = tree;

        graphViewChanged -= OnGraphViewChanged;
        serializeGraphElements -= CutCopyOperation;
        unserializeAndPaste -= PasteOperation;
        canPasteSerializedData -= CanPaste;
        _tree.OnUpdateViewStates -= OnUpdateStates;
        
        DeleteElements(graphElements);
        
        graphViewChanged += OnGraphViewChanged;
        serializeGraphElements += CutCopyOperation;
        unserializeAndPaste += PasteOperation;
        canPasteSerializedData += CanPaste;
        _tree.OnUpdateViewStates += OnUpdateStates;
        
        
        if (_tree.startNode == null)
        {
            _tree.startNode = _tree.CreateNode(_currentDatabase, typeof(StartNode), Vector2.zero) as StartNode;
            EditorUtility.SetDirty(tree);
            AssetDatabase.SaveAssets();
        }
        
        if (_tree.completeNode == null)
        {
            _tree.completeNode = _tree.CreateNode(_currentDatabase, typeof(CompleteNode), new Vector2(1000f,0f)) as CompleteNode;
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
                foreach(KeyValuePair<Port, Choice> entry in nodeView.portTranslationMap)
                {
                    Port choicePort = entry.Key;
                    CreateEdgeView(entry.Value.children,choicePort);
                }
            }
        });
        
        OnUpdateStates();
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
                    bool hasBeenAAdded = _tree.AddGroupToNode(nodeView.node, groupNodeView.group);
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
                bool hasBeenAAdded = _tree.AddGroupToNode(nodeView.node, groupNodeView.group);
                if (hasBeenAAdded)
                {
                    groupNodeView.AddElement(nodeView);
                }
            }
        }
        
        // Create Dialog Edges
        foreach (SerializableDialogNode serializedDialogNode in cutCopyData.dialogNodesToCopy)
        {
            NodeView parentView = nodeTranslationMap[serializedDialogNode.guid];
            foreach (SerializableNodeChild childOriginalNode in serializedDialogNode.children)
            {
                if (!nodeTranslationMap.TryGetValue(childOriginalNode.guid, out NodeView childView)) continue;
                bool hasAdded = _tree.AddChild(parentView.node, childView.node);
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
                    Port choicePort = parentView.portTranslationMap.FirstOrDefault(x => x.Value.guid == newChoice.guid).Key;
                    bool hasAdded = _tree.AddChild(parentView.node, childView.node, choicePort);
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
            // Delete elements
            foreach (GraphElement element in graphViewChange.elementsToRemove)
            {
                NodeView nodeView = element as NodeView;
                if (nodeView != null)
                {
                    DeleteNode(nodeView.node);
                    nodeHasBeenRemoved = true;
                    continue;
                }

                Edge edge = element as Edge;
                if (edge != null)
                {
                    NodeView  parentView = edge.output.node as NodeView;
                    NodeView  childView = edge.input.node as NodeView;
                    _tree.RemoveChild(parentView?.node, childView?.node, edge.output);
                    continue;
                }
                
                GroupNodeView groupNodeView = element as GroupNodeView;
                if (groupNodeView != null)
                {
                    DeleteGroupNode(groupNodeView.group);
                }
            }
            if(nodeHasBeenRemoved) onNodesRemoved?.Invoke();
        }
        
        if (graphViewChange.edgesToCreate != null)
        {
            // To prevent edges that hasn't added
            List<Edge> edgesNotAdded = new List<Edge>();
            
            foreach (Edge edge in graphViewChange.edgesToCreate)
            {
                NodeView  parentView = edge.output.node as NodeView;
                NodeView  childView = edge.input.node as NodeView;
                bool hasAdded = _tree.AddChild(parentView?.node, childView?.node,edge.output);
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
        GroupNode groupNode = _tree.CreateGroup(_currentDatabase, title, pos);
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
        Node node = _tree.CreateNode(_currentDatabase, type,position);
        CreateNodeView(node);
    }
    private NodeView CreateDialogNodeCopy(Type type,Vector2 position, SerializableDialogNode nodeToCopy)
    {
        Node node = _tree.CreateDialogNodeCopy(_currentDatabase, type,position,nodeToCopy);
        return CreateNodeView(node);
    }
    
    private NodeView CreateChoiceNodeCopy(Vector2 position, SerializableChoiceNode nodeToCopy, Dictionary<string, Choice> choiceMap)
    {
        Node node = _tree.CreateChoiceNodeToCopy(_currentDatabase,position,nodeToCopy, choiceMap);
        return CreateNodeView(node);
    }

    NodeView CreateNodeView(Node node)
    {
        if (node == null) return null;
        
        NodeView nodeView = new NodeView(node,this, ClearSelection);
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
        
        if (!Application.isPlaying) return;

        edges.ForEach(e =>
        {
            NodeView inputNode = e.input.node as NodeView;
            NodeView outputNode = e.output.node as NodeView;
            Color newColor = new Color(255, 253, 0, 50);
            Color neutralColor = new Color(0.8235294f, 0.8235294f, 0.8235294f, 1);
            
            if ((inputNode.node.NodeState == Node.State.Running && outputNode.node.NodeState == Node.State.VisitedUnreachable) ||
                (inputNode.node.NodeState == Node.State.VisitedUnreachable && outputNode.node.NodeState == Node.State.VisitedUnreachable) ||
                (inputNode.node.NodeState == Node.State.Finished && outputNode.node.NodeState == Node.State.VisitedUnreachable))
            {

                if(e.input.connections.Count() == 1) e.input.portColor = newColor;
                else e.input.portColor = neutralColor;
                if (e.output.connections.Count() == 1) e.output.portColor = newColor;
                else e.input.portColor = neutralColor;
                e.style.color = newColor;
            }

            if (inputNode.node.NodeState == Node.State.Initial &&
                (outputNode.node.NodeState == Node.State.Initial || outputNode.node.NodeState == Node.State.Running))
            {
                
                e.input.portColor = neutralColor;
                e.output.portColor = neutralColor;
                e.style.color = neutralColor;
            }
        });
        
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
