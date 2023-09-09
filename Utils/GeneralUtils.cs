using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AQM.Tools.Serializable;
using UnityEngine;

public static class GeneralUtils
{
    public static SerializableDialogNode ConvertToSerializableNode(Node node)
    {
        DialogNode dialogNode = node as DialogNode;
        if (dialogNode != null)
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
            foreach (var nodeChild in node.children)
            {
                serializableNode.children.Add(new SerializableNodeChild()
                {
                    guid = nodeChild.guid
                });
            }
            return serializableNode;
        }

        return null;
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
