using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
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
        public static Action onConversationEnded;
        public static Action<DialogSystemDatabase> onDatabaseCloned;
        
        [SerializeField] private DialogSystemDatabase dialogSystemDatabase;

        public DialogSystemDatabase DialogSystemDatabase => dialogSystemDatabase;

        private ConversationTree _currentConversation;
        private DSChoice _currentChoiceNode;
        private InputActions.UIActions _playerInputs;

        private void Awake()
        {
            DDEvents.onStartConversation += StartConversation;
        }

        private IEnumerator Start()
        {
            yield return LocalizationSettings.InitializationOperation;
            dialogSystemDatabase = dialogSystemDatabase.Clone();
            onDatabaseCloned?.Invoke(dialogSystemDatabase);
        }
        
        public List<KeyValuePair<ConversationTree, string>> GetConversationPairs()
        {
            var pairs = new List<KeyValuePair<ConversationTree, string>>();
            if (dialogSystemDatabase != null)
            {
                foreach (ConversationTree conversation in dialogSystemDatabase.conversations)
                {
                    pairs.Add(new KeyValuePair<ConversationTree, string>(conversation, $"{conversation.title}"));
                }
            }

            return pairs;
        }
        public string GetConversationPath(ConversationTree tree)
        {
            return tree.title;
        }
        
        private void StartConversation(ConversationTree conversationTree)
        {
            InputManager.Instance.ToogleActionMap(InputManager.Instance.playerInputActions.UI);
            _playerInputs = InputManager.Instance.playerInputActions.UI;
            _currentConversation = conversationTree;
            _currentChoiceNode = null;
            _currentConversation.onEndConversation += EndConversation;
            
            #if LOCALIZATION_EXIST
                LocalizationSettings.SelectedLocaleChanged += ChangedLocale;
            #endif

            //DSNode nextNode = dialogSystemDatabase.StartConversation(_currentConversation);
            //HandleNextNode(nextNode);
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
                        onShowNewChoice?.Invoke(choiceNode);
                        break;
                }
            }
        }
    #endif
        

        private void NextMessage(InputAction.CallbackContext context)
        {
            _playerInputs.Click.started -= NextMessage;
            _playerInputs.Submit.started -= NextMessage;
            GetNextNode();
        }

        private void HandleNextNode(DSNode nextNode)
        {
            if (nextNode is DSChoice choiceNode)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                
                _currentChoiceNode = choiceNode;
                _currentChoiceNode.onChoiceSelected += OnChoiceSelected;
                onShowNewChoice?.Invoke(_currentChoiceNode);
            }
            else if(nextNode is DSDialog dialogNode)
            {
                _playerInputs.Click.started += NextMessage;
                _playerInputs.Submit.started += NextMessage;
                onShowNewDialog?.Invoke(dialogNode);
            }
        }
        
        private void OnChoiceSelected(int option)
        {
            if (_currentChoiceNode != null)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                _currentChoiceNode.onChoiceSelected -= OnChoiceSelected;
                GetNextNode(option);
            }
        }
        
        private void GetNextNode(int option = -1)
        {
            DSNode nextNode = _currentConversation.GetNextNode(option);
            HandleNextNode(nextNode);
        }

        private void EndConversation()
        {
            InputManager.Instance.ToogleActionMap(InputManager.Instance.playerInputActions.Player);
            _playerInputs.Click.started -= NextMessage;
            _playerInputs.Submit.started -= NextMessage;
            _currentConversation.onEndConversation -= EndConversation;
            _currentConversation = null;
            onConversationEnded?.Invoke();
            
    #if LOCALIZATION_EXIST
            LocalizationSettings.SelectedLocaleChanged -= ChangedLocale;
    #endif
        }

        private void OnDestroy()
        {
            DDEvents.onStartConversation -= StartConversation;
    #if LOCALIZATION_EXIST
            LocalizationSettings.SelectedLocaleChanged -= ChangedLocale;
    #endif
        }
    }
}

