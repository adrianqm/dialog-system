using System;
using System.Collections.Generic;
using AQM.Tools.Serializable;

[Serializable]
class CutCopySerializedData
{
    public List<SerializableDialogNode> dialogNodesToCopy = new();
    public List<SerializableChoiceNode> choiceNodesToCopy = new();
    public List<SerializableGroupNode> groupNodesToCopy = new();
}