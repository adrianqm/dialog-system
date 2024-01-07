using System.Collections.Generic;
using AQM.Tools.Serializable;
using UnityEditor.Experimental.GraphView;

namespace AQM.Tools
{
    public static class GeneralUtils
    {
        public static SerializableDialogNode ConvertToSerializableDialogNode(DialogNodeSO dialogNodeSo)
        {
            SerializableDialogNode serializableNode = new SerializableDialogNode
            {
                guid = dialogNodeSo.guid,
                position = new Vector2Serializable(dialogNodeSo.position),
                message = dialogNodeSo.message,
                actorGuid = dialogNodeSo.actor?dialogNodeSo.actor.guid: ""
            };

            if (dialogNodeSo.group != null)
            {
                serializableNode.group = ConvertToSerializableGroupNode(dialogNodeSo.group);
            }

            serializableNode.children = new List<SerializableNodeChild>();
            foreach (var nodeChild in dialogNodeSo.outputPorts[0].targetNodes)
            {
                serializableNode.children.Add(new SerializableNodeChild()
                {
                    guid = nodeChild.guid
                });
            }
            return serializableNode;
        }
        
        public static SerializableChoiceNode ConvertToSerializableChoiceNode(ChoiceNodeSO choiceNodeSo)
        {
            SerializableChoiceNode serializableNode = new SerializableChoiceNode
            {
                guid = choiceNodeSo.guid,
                position = new Vector2Serializable( choiceNodeSo.position),
                message =  choiceNodeSo.message,
                actorGuid =  choiceNodeSo.actor? choiceNodeSo.actor.guid: ""
            };

            if (choiceNodeSo.group != null)
            {
                serializableNode.group = ConvertToSerializableGroupNode(choiceNodeSo.group);
            }

            serializableNode.choices = new List<SerializableChoice>();
            foreach (var choice in choiceNodeSo.choices)
            {
                SerializableChoice newSerializableChoice = new SerializableChoice()
                {
                    guid = choice.guid,
                    choiceMessage = choice.choiceMessage,
                };
                
                newSerializableChoice.children = new List<SerializableNodeChild>();
                foreach (var child in choice.port.targetNodes)
                {
                    newSerializableChoice.children.Add(new SerializableNodeChild()
                    {
                        guid = child.guid
                    });
                }
                serializableNode.choices.Add(newSerializableChoice);
            }
            
            serializableNode.defaultChildren = new List<SerializableNodeChild>();
            foreach (var child in choiceNodeSo.defaultPort.targetNodes)
            {
                serializableNode.defaultChildren.Add(new SerializableNodeChild()
                {
                    guid = child.guid
                });
            }
            
            return serializableNode;
        }

        public static SerializableGroupNode ConvertToSerializableGroupNode(GroupNode groupNode)
        {
            SerializableGroupNode serializableGroupNode = new SerializableGroupNode
            {
                guid = groupNode.guid,
                position = new Vector2Serializable(groupNode.position),
                title = groupNode.title
            };

            return serializableGroupNode;
        }

        public static CutCopySerializedData GenerateCutCopyObject(IEnumerable<GraphElement> elements)
        {
            CutCopySerializedData cutCopyData = new CutCopySerializedData();
            foreach (GraphElement n in elements)
            {
                NodeView nodeView = n as NodeView;
                if(nodeView != null) 
                {
                    DialogNodeSO dialogNodeSo = nodeView.node as DialogNodeSO;
                    if (dialogNodeSo)
                    {
                        cutCopyData.dialogNodesToCopy.Add(ConvertToSerializableDialogNode(dialogNodeSo));
                        continue;
                    }
                    
                    ChoiceNodeSO choiceNodeSo = nodeView.node as ChoiceNodeSO;
                    if (choiceNodeSo)
                    {
                        cutCopyData.choiceNodesToCopy.Add(ConvertToSerializableChoiceNode(choiceNodeSo));
                        continue;
                    }
                }
                
                GroupNodeView groupView = n as GroupNodeView;
                if(groupView != null)
                {
                    cutCopyData.groupNodesToCopy.Add(GeneralUtils.ConvertToSerializableGroupNode(groupView.group));
                }
            }
            return cutCopyData;
        }
    }
}

