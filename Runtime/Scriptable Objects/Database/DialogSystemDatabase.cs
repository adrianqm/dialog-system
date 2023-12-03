using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
#if LOCALIZATION_EXIST
using UnityEngine.Localization;
#endif

namespace AQM.Tools
{
    [CreateAssetMenu(fileName = "New Dialog System Database", menuName = "AQM/Tools/Dialog System/Dialog System Database", order = 1)]
    public class DialogSystemDatabase : ScriptableObject
    {
        public string title;
        public string description;
        [HideInInspector] public string guid;
        public List<ConversationGroup> conversationGroups;
        public List<ConversationTree> conversations;
        public List<Actor> actors;
        
        #if LOCALIZATION_EXIST
            public string tableCollectionName;
            public Locale defaultLocale;
            public bool localizationActivated;
        #endif

        public DSNode StartConversation(ConversationTree conversationTree)
        {
            return !conversations.Contains(conversationTree) ? null : conversationTree.StartConversation(this);
        }
        
        public DialogSystemDatabase Clone()
        {
            DialogSystemDatabase tree = Instantiate(this);
            tree.title = "(Cloned) "+ title;
            tree.description = description;
            tree.conversations = new List<ConversationTree>();
            conversations.ForEach((conversation) =>
            {
                ConversationTree clonedConversation = conversation.Clone();
                tree.conversations.Add(clonedConversation);
            });
            tree.actors = new List<Actor>();
            actors.ForEach((actor) =>
            {
                Actor clonedActor = actor.Clone();
                tree.actors.Add(clonedActor);
            });
            return tree;
        }
        
    #if  UNITY_EDITOR
        
        public void CreateConversation()
        {
            
            ConversationTree newConversation = ScriptableObject.CreateInstance(typeof(ConversationTree)) as ConversationTree;
            if (newConversation)
            {
                newConversation.name = "ConversationTree";
                newConversation.guid = GUID.Generate().ToString();
                newConversation.title = "Default Title";
                newConversation.description = "Default Desc";
                newConversation.hideFlags = HideFlags.HideInHierarchy;
                
                //Undo.RecordObject(this, "Actors Tree (CreateActor)");
                conversations.Add(newConversation);
                
                if (!Application.isPlaying)
                {
                    AssetDatabase.AddObjectToAsset( newConversation,this);
                }
                //Undo.RegisterCreatedObjectUndo(newActor, "Actors Tree (CreateActor)");
                AssetDatabase.SaveAssets();
            }
        }
        
        public void DeteleConversation(ConversationTree conversation)
        {
            conversations.Remove(conversation);
            Undo.DestroyObjectImmediate(conversation);
            AssetDatabase.SaveAssets();
        }
    #endif
    }
}

