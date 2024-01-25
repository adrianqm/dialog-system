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
        public static Action<DSNode> onNextMessageShown;
        public static Action onConversationStarted;
        public static Action onConversationEnded;
        
        [SerializeField] private DialogSystemDatabase dialogSystemDatabase;
        
        [Header("Dialog Config")]
        [SerializeField] private List<InputActionReference> inputKeys;
        [SerializeField] private bool dialogTimer;
        [SerializeField] private float dialogTime = 2f;
        private Coroutine _dialogCo;
        
        [Header("Choice Config")]
        [SerializeField] private bool hasNoResponse;
        [SerializeField] private float noResponseTime = 2f;

        public DialogSystemDatabase DialogSystemDatabase => dialogSystemDatabase;

        private ConversationTree _currentConversation;
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
            if (_currentConversation != null)
            {
                dialogSystemDatabase.ForceEndOfConversation(_currentConversation);
                ResetEndData();
            }
            _currentConversation = conversationTree;
            _currentChoiceNode = null;
            DSNode nextNode = dialogSystemDatabase.StartConversation(_currentConversation);
            if (nextNode != null)
            {
                onConversationStarted?.Invoke();
                _currentConversation.onEndConversation -= EndConversation;
                _currentConversation.onEndConversation += EndConversation;
            
#if LOCALIZATION_EXIST
                LocalizationSettings.SelectedLocaleChanged += ChangedLocale;
#endif
                HandleNextNode(nextNode);
            }
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

        private void NextMessageCallback(InputAction.CallbackContext context)
        {
            NextMessage();
        }

        private void NextMessage()
        {
            DisableInputKeys();
            GetNextNode();
        }

        private void HandleNextNode(DSNode nextNode)
        {
            if(nextNode == null) return;
            if (nextNode.DelayTime > 0)
            {
                StartCoroutine(ShowDialogWithDelay(nextNode));
            }else ShowNode(nextNode);
        }

        private void ShowNode(DSNode node)
        {
            if (node is DSChoice choiceNode)
            {
                _currentChoiceNode = choiceNode;
                _currentChoiceNode.onChoiceSelected += OnChoiceSelected;
                
                if (hasNoResponse)onShowNewChoiceInTime?.Invoke(choiceNode,noResponseTime);
                else onShowNewChoice?.Invoke(choiceNode);
                onNextMessageShown?.Invoke(node);
            }
            else if(node is DSDialog dialogNode)
            {
                EnableInputKeys();
                onShowNewDialog?.Invoke(dialogNode);
                onNextMessageShown?.Invoke(node);
            }
        }

        private IEnumerator ShowDialogWithDelay(DSNode node)
        {
            yield return new WaitForSeconds(node.DelayTime);
            ShowNode(node);
        }
        
        private void OnChoiceSelected(int option)
        {
            if (_currentChoiceNode != null)
            {
                _currentChoiceNode.onChoiceSelected -= OnChoiceSelected;
                GetNextNode(option);
            }
        }
        
        public void GetNextNode(int option = -2)
        {
            if(_currentConversation == null) return;
            DSNode nextNode = _currentConversation.GetNextNode(option);
            HandleNextNode(nextNode);
        }

        private void EndConversation()
        {
            ResetEndData();
            onConversationEnded?.Invoke();
        }

        private void ResetEndData()
        {
            DisableInputKeys();
            _currentConversation.onEndConversation -= EndConversation;
            _currentConversation = null;
            
            
#if LOCALIZATION_EXIST
            LocalizationSettings.SelectedLocaleChanged -= ChangedLocale;
#endif
        }

        private void EnableInputKeys()
        {
            foreach (var key in inputKeys)
            {
                key.action.started += NextMessageCallback;
            }

            if (dialogTimer)
            {
                _dialogCo = StartCoroutine(NextMessageCoroutine());
            }
        }
        
        private void DisableInputKeys()
        {
            foreach (var key in inputKeys)
            {
                key.action.started -= NextMessageCallback;
            }
            
            if(_dialogCo != null) StopCoroutine(_dialogCo);
        }

        private IEnumerator NextMessageCoroutine()
        {
            yield return new WaitForSeconds(dialogTime);
            NextMessage();
        }

        private void OnEnable()
        {
            foreach (var key in inputKeys)
            {
                key.action.Enable();
            }
        }

        private void OnDisable()
        {
            foreach (var key in inputKeys)
            {
                key.action.Disable();
            }
        }

        private void OnDestroy()
        {
            DDEvents.onStartConversation -= StartConversation;
            DDEvents.onGetNextNode -= GetNextNode;
            if(_currentConversation) _currentConversation.onEndConversation -= EndConversation;
            if(_dialogCo != null) StopCoroutine(_dialogCo);
    #if LOCALIZATION_EXIST
            LocalizationSettings.SelectedLocaleChanged -= ChangedLocale;
    #endif
        }
    }
}

