using System.Collections.Generic;
using AQM.Tools.Serializable;
using UnityEditor.Experimental.GraphView;

namespace AQM.Tools
{
    public static class GeneralUtils
    {
        public static SerializableDialogNode ConvertToSerializableDialogNode(DialogNode dialogNode)
        {
            SerializableDialogNode serializableNode = new SerializableDialogNode
            {
                guid = dialogNode.guid,
                position = new Vector2Serializable(dialogNode.position),
                message = dialogNode.message,
                actorGuid = dialogNode.actor?dialogNode.actor.guid: ""
            };

            if (dialogNode.group != null)
            {
                serializableNode.group = ConvertToSerializableGroupNode(dialogNode.group);
            }

            serializableNode.children = new List<SerializableNodeChild>();
            foreach (var nodeChild in dialogNode.children)
            {
                serializableNode.children.Add(new SerializableNodeChild()
                {
                    guid = nodeChild.guid
                });
            }
            return serializableNode;
        }
        
        public static SerializableChoiceNode ConvertToSerializableChoiceNode(ChoiceNode choiceNode)
        {
            SerializableChoiceNode serializableNode = new SerializableChoiceNode
            {
                guid = choiceNode.guid,
                position = new Vector2Serializable( choiceNode.position),
                message =  choiceNode.message,
                actorGuid =  choiceNode.actor? choiceNode.actor.guid: ""
            };

            if (choiceNode.group != null)
            {
                serializableNode.group = ConvertToSerializableGroupNode(choiceNode.group);
            }

            serializableNode.choices = new List<SerializableChoice>();
            foreach (var choice in choiceNode.choices)
            {
                SerializableChoice newSerializableChoice = new SerializableChoice()
                {
                    guid = choice.guid,
                    choiceMessage = choice.choiceMessage,
                };
                
                newSerializableChoice.children = new List<SerializableNodeChild>();
                foreach (var child in choice.children)
                {
                    newSerializableChoice.children.Add(new SerializableNodeChild()
                    {
                        guid = child.guid
                    });
                }

                serializableNode.choices.Add(newSerializableChoice);
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
                    DialogNode dialogNode = nodeView.node as DialogNode;
                    if (dialogNode)
                    {
                        cutCopyData.dialogNodesToCopy.Add(ConvertToSerializableDialogNode(dialogNode));
                        continue;
                    }
                    
                    ChoiceNode choiceNode = nodeView.node as ChoiceNode;
                    if (choiceNode)
                    {
                        cutCopyData.choiceNodesToCopy.Add(ConvertToSerializableChoiceNode(choiceNode));
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
        
        public static List<T> MoveItemAtIndexToFront<T>(this List<T> list, int index)
        {
            T item = list[index];
            for (int i = index; i > 0; i--)
                list[i] = list[i - 1];
            list[0] = item;
            return list;
        }
    }
}

