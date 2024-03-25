using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
#if LOCALIZATION_EXIST
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
#endif

namespace AQM.Tools
{
    public class DialogSystemController : Singleton<DialogSystemController>
    {
        public static Action<DSDialog> onShowNewDialog;
        public static Action<DSChoice> onShowNewChoice;
        public static Action<DSChoice,float> onShowNewChoiceInTime;
        public static Action onConversationStarted;
        public static Action onConversationEnded;
        
        [SerializeField] private DialogSystemDatabase dialogSystemDatabase;
        public DialogSystemDatabase DialogSystemDatabase => dialogSystemDatabase;
        
        [Header("Dialog Config")]
        
        [SerializeField] private bool dialogTimer;
        [SerializeField] private float dialogTime = 2f;
        public bool DialogTimer => dialogTimer;
        public float DialogTime => dialogTime;
        
        [Header("Choice Config")]
        [SerializeField] private bool hasNoResponse;
        [SerializeField] private float noResponseTime = 2f;

        private ConversationStateMachine _currentConversation;
        private DSChoice _currentChoiceNode;

        private void Awake()
        {
            DDEvents.onStartConversation += StartConversation;
            DDEvents.onGetNextNode += GetNextNode;
        }

        #if LOCALIZATION_EXIST
        private IEnumerator Start()
        {
            if (dialogSystemDatabase && dialogSystemDatabase.tableCollectionName != "" && dialogSystemDatabase.defaultLocale)
            {
                var d = LocalizationSettings.StringDatabase.GetTableAsync(dialogSystemDatabase.tableCollectionName);
                if (!d.IsDone)
                    yield return d;
                string dump = LocalizationSettings.StringDatabase.GetLocalizedString(dialogSystemDatabase.tableCollectionName,"default");
            }
        }
        #endif
        
        
        public List<KeyValuePair<ConversationTree, string>> GetConversationPairs()
        {
            var pairs = new List<KeyValuePair<ConversationTree, string>>();
            if (dialogSystemDatabase != null)
            {
                foreach (ConversationGroup group in dialogSystemDatabase.conversationGroups)
                {
                    pairs.AddRange(GetPairInGroup(group, ""));
                }
            }

            return pairs;
        }

        public List<Actor> GetActors()
        {
            List<Actor> actors = new List<Actor>();
            if (dialogSystemDatabase)
            {
                actors =  dialogSystemDatabase.actors;
            }
            return actors;
        }

        private List<KeyValuePair<ConversationTree, string>> GetPairInGroup(ConversationGroup group, string path)
        {
            var pairs = new List<KeyValuePair<ConversationTree, string>>();
            path += group.title + '/';
            foreach (ConversationTree conversation in group.conversations)
            {
                pairs.Add(new KeyValuePair<ConversationTree, string>(conversation, $"{ path + conversation.title}"));
            }
            
            foreach (ConversationGroup inGroup in group.groups)
            {
                pairs.AddRange(GetPairInGroup(inGroup,path));
            }
            return pairs;
        }
        
        public string GetConversationPath(ConversationTree tree)
        {
            return tree.title;
        }
        
        private void StartConversation(ConversationTree conversationTree)
        {
            DestroyConversation();
            _currentConversation = gameObject.AddComponent<ConversationStateMachine>();
            _currentConversation.onConversationStarted += OnStartConversation;
            _currentConversation.onShowNode += OnShowNode;
            _currentConversation.onConversationEnded += OnEndConversation;
#if LOCALIZATION_EXIST
            LocalizationSettings.SelectedLocaleChanged += ChangedLocale;
#endif
            _currentConversation.Initialize(dialogSystemDatabase,conversationTree);
        }
        
#if LOCALIZATION_EXIST
        private void ChangedLocale(Locale locale){
            if (!_currentConversation) return;
            DSNode currentNode = _currentConversation.GetCurrentNode();
            if (currentNode != null)
            {
                switch (currentNode)
                {
                    case DSDialog dialogNode:
                        onShowNewDialog?.Invoke(dialogNode);
                        break;
                    case DSChoice choiceNode:
                        _currentChoiceNode.onChoiceSelected -= OnChoiceSelected;
                        _currentChoiceNode = choiceNode;
                        _currentChoiceNode.onChoiceSelected += OnChoiceSelected;
                        
                        if(hasNoResponse) onShowNewChoiceInTime?.Invoke(choiceNode,noResponseTime);
                        else onShowNewChoice?.Invoke(choiceNode);
                        break;
                }
            }
        }
    #endif

        private void GetNextNode(int option)
        {
            _currentConversation.GetNextStep(option);
        }
        
        private void OnChoiceSelected(int option)
        {
            if (_currentChoiceNode != null)
            {
                _currentChoiceNode.onChoiceSelected -= OnChoiceSelected;
                GetNextNode(option);
            }
        }

        private void OnStartConversation()
        {
            onConversationStarted?.Invoke();
        }

        private void OnShowNode(DSNode node)
        {
            if (node is DSChoice choiceNode)
            {
                _currentChoiceNode = choiceNode;
                _currentChoiceNode.onChoiceSelected += OnChoiceSelected;
                
                if (hasNoResponse) onShowNewChoiceInTime?.Invoke(choiceNode,noResponseTime);
                else onShowNewChoice?.Invoke(choiceNode);
            }
            else if(node is DSDialog dialogNode)
            {
                onShowNewDialog?.Invoke(dialogNode);
            }
        }
        
        private void OnEndConversation()
        {
            DestroyConversation();
            onConversationEnded?.Invoke();
        }

        private void DestroyConversation()
        {
            if (!_currentConversation) return;
            _currentConversation.onConversationStarted -= OnStartConversation;
            _currentConversation.onShowNode -= OnShowNode;
            _currentConversation.onConversationEnded -= OnEndConversation;
#if LOCALIZATION_EXIST
            LocalizationSettings.SelectedLocaleChanged -= ChangedLocale;
#endif
            Destroy(_currentConversation);
        }

        private void OnDestroy()
        {
            DDEvents.onStartConversation -= StartConversation;
            DDEvents.onGetNextNode -= GetNextNode;
            DestroyConversation();
        }
    }
}

