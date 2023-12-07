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
        public List<Actor> actors;
        
        #if LOCALIZATION_EXIST
            public string tableCollectionName;
            public Locale defaultLocale;
            public bool localizationActivated;
        #endif

        public DSNode StartConversation(ConversationTree conversationTree)
        {
            return conversationTree.StartConversation(this);
        }
        
        public DialogSystemDatabase Clone()
        {
            DialogSystemDatabase tree = Instantiate(this);
            tree.title = "(Cloned) "+ title;
            tree.description = description;
            tree.conversationGroups = new List<ConversationGroup>();
            conversationGroups.ForEach((group) =>
            {
                tree.conversationGroups.Add(group.Clone());
            });
            
            tree.actors = new List<Actor>();
            actors.ForEach((actor) =>
            {
                Actor clonedActor = actor.Clone();
                tree.actors.Add(clonedActor);
            });
            return tree;
        }
    }
}

