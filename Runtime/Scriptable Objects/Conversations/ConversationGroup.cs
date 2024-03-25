using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AQM.Tools
{
    public class ConversationGroup: ScriptableObject
    {
        public string guid;
        public string title;
        public List<ConversationGroup> groups;
        public List<ConversationTree> conversations;
        
        public ConversationTree FindConversation(string conversationGuid)
        {
            ConversationTree conversation = conversations.Find(c => c.guid == conversationGuid);
            if (conversation != null)
            {
                return conversation;
            }
            if (groups != null)
            {
                foreach (var group in groups)
                {
                    conversation = group.FindConversation(conversationGuid);
                    if (conversation != null)
                    {
                        return conversation;
                    }
                }
            }
            return null;
        }
        
#if UNITY_EDITOR
        
        public void CreateConversation(DialogSystemDatabase db)
        {
            
            ConversationTree newConversation = ScriptableObject.CreateInstance(typeof(ConversationTree)) as ConversationTree;
            if (newConversation)
            {
                newConversation.name = "ConversationTree";
                newConversation.guid = GUID.Generate().ToString();
                newConversation.title = "Default Title";
                newConversation.description = "Default Desc";
                //newConversation.hideFlags = HideFlags.HideInHierarchy;
                
                //Undo.RecordObject(this, "Actors Tree (CreateActor)");
                conversations.Add(newConversation);
                AssetDatabase.AddObjectToAsset( newConversation,db);
                
                //Undo.RegisterCreatedObjectUndo(newActor, "Actors Tree (CreateActor)");
                AssetDatabase.SaveAssets();
            }
        }
        
        public void DeleteConversation(ConversationTree conversation)
        {
            conversations.Remove(conversation);
            Undo.DestroyObjectImmediate(conversation);
            AssetDatabase.SaveAssets();
        }
#endif
    }
}