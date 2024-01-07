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
        
        public ConversationTree FindConversation(string conversationGuid)
        {
            foreach (var group in conversationGroups)
            {
                ConversationTree conversation = group.FindConversation(conversationGuid);
                if (conversation != null)
                {
                    return conversation;
                }
            }
            return null;
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
        
        public void DeleteDatabase()
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.SaveAssets();
        }
        
        public ConversationGroup CreateConversationGroup(string title, ConversationGroup fromGroup = null)
        {
            
            ConversationGroup newGroup = ScriptableObject.CreateInstance(typeof(ConversationGroup)) as ConversationGroup;
            if (newGroup)
            {
                newGroup.name = "ConversationGroup";
                newGroup.guid = GUID.Generate().ToString();
                newGroup.title = title;
                newGroup.groups ??= new List<ConversationGroup>();
                newGroup.conversations ??= new List<ConversationTree>();
                //newGroup.hideFlags = HideFlags.HideInHierarchy;
                if (!fromGroup || conversationGroups == null)
                {
                    conversationGroups??= new List<ConversationGroup>();
                    conversationGroups.Add(newGroup);
                }else if (fromGroup)
                {
                    fromGroup.groups.Add(newGroup);
                }
                
                if (!Application.isPlaying)
                {
                    AssetDatabase.AddObjectToAsset(newGroup,this);
                }
                AssetDatabase.SaveAssets();
            }
            return newGroup;
        }
        
        public void DeleteConversationGroup(ConversationGroup group, ConversationGroup fromGroup = null, Dictionary<string,int> keyMap = null)
        {
            RemoveConversationGroups(group, fromGroup, keyMap);
            AssetDatabase.SaveAssets();
        }
        
        private void RemoveConversationGroups(ConversationGroup group, ConversationGroup fromGroup = null, Dictionary<string,int> keyMap = null)
        {
            RemoveConversationGroupsInGroup(group, keyMap);
            RemoveConversationsInGroup(group);

            if (fromGroup == null) conversationGroups.Remove(group);
            else fromGroup.groups.Remove(group);
            if (keyMap != null) keyMap.Remove(group.guid);
            Undo.DestroyObjectImmediate(group);
        }

        private void RemoveConversationGroupsInGroup(ConversationGroup group,Dictionary<string,int> keyMap = null)
        {
            foreach (var conversationGroup in group.groups)
            {
                RemoveConversationGroupsInGroup(conversationGroup);
                if (keyMap != null) keyMap.Remove(conversationGroup.guid);
                Undo.DestroyObjectImmediate(conversationGroup);
            }
            group.groups.Clear();

            RemoveConversationsInGroup(group);
        }
        private void RemoveConversationsInGroup(ConversationGroup group)
        {
            foreach (var groupConversation in group.conversations)
            {
                Undo.DestroyObjectImmediate(groupConversation);
            }
            group.conversations.Clear();
        }
        
        public void CreateActor()
        {
            Actor newActor = ScriptableObject.CreateInstance(typeof(Actor)) as Actor;
            if (!newActor) return;
            
            newActor.name = "Actor";
            newActor.guid = GUID.Generate().ToString();
            newActor.fullName = "defaultName";
            newActor.description = "defaultDesc";
            newActor.hideFlags = HideFlags.HideInHierarchy;
            
            actors.Add(newActor);
                
            if (!Application.isPlaying)
            {
                AssetDatabase.AddObjectToAsset(newActor, this);
            }
            AssetDatabase.SaveAssets();
        }
        
        public void DeleteActor(Actor actor)
        {
            actors.Remove(actor);
            Undo.DestroyObjectImmediate(actor);
            AssetDatabase.SaveAssets();
        }
        
    }
}

