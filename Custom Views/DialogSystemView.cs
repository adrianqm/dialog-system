using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class DialogSystemView : GraphView
{
    
    public Action<NodeView> OnNodeSelected;
    public Action onNodesRemoved;
    public new class UxmlFactory : UxmlFactory<DialogSystemView, GraphView.UxmlTraits> {}
    
    private ConversationTree _tree;
    public DialogSystemView()
    {
        Insert(0, new GridBackground());
        
        this.AddManipulator(new ContentZoomer());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        
        // Import StyleSheets 
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/dialog-system/DialogSystemEditor.uss");
        styleSheets.Add(styleSheet);

        
    }
    
    NodeView FindNodeView(Node node)
    {
        return GetNodeByGuid(node.guid) as NodeView;
    }

    internal void PopulateView(ConversationTree tree)
    {
        if (tree == null) return;
        
        this._tree = tree;

        graphViewChanged -= OnGraphViewChanged;
        DeleteElements(graphElements);
        graphViewChanged += OnGraphViewChanged;
        
        if (_tree.rootNode == null)
        {
            _tree.rootNode = _tree.CreateNode(typeof(RootNode), Vector2.zero) as RootNode;
            EditorUtility.SetDirty(tree);
            AssetDatabase.SaveAssets();
        }
        
        // Create node view
        _tree.nodes.ForEach(CreateNodeView);
        
        // Create edges
        _tree.nodes.ForEach(n =>
        {
            var children = _tree.GetChildren(n);
            NodeView parentView = FindNodeView(n);
            children.ForEach(c =>
            {
                NodeView childView = FindNodeView(c);

                Edge edge = parentView.output.ConnectTo(childView.input);
                AddElement(edge);
            });
        });
        
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
                }

                Edge edge = element as Edge;
                if (edge != null)
                {
                    NodeView  parentView = edge.output.node as NodeView;
                    NodeView  childView = edge.input.node as NodeView;
                    _tree.RemoveChild(parentView.node, childView.node);
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
        var pos = MouseToContent(evt.mousePosition);
        {
            var types = TypeCache.GetTypesDerivedFrom<Node>();
            foreach (var type in types)
            {
                if (type != typeof(RootNode))
                {
                    evt.menu.AppendAction("Create "+type.Name+" Node", (a)=> CreateNode(type,pos));
                }
            }
            
            evt.menu.AppendSeparator();
        }
        base.BuildContextualMenu(evt);
    }

    private Vector2 MouseToContent(Vector2 position)
    {
        position.x = (position.x - contentViewContainer.worldBound.x) / scale;
        position.y = (position.y - contentViewContainer.worldBound.y) / scale;
        return position;
    }

    void CreateNode(Type type,Vector2 position)
    {
        Node node = _tree.CreateNode(type,position);
        CreateNodeView(node);
    }

    void CreateNodeView(Node node)
    {
        NodeView nodeView = new NodeView(node);
        nodeView.onNodeSelected = OnNodeSelected;
        AddElement(nodeView);
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
    
}
