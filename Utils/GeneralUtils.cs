using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AQM.Tools.Serializable;
using UnityEngine;

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
}
