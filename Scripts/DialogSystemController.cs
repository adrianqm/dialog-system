using System;
using System.Collections;
using System.Collections.Generic;
using AQM.Tools;
using UnityEngine;
using UnityEngine.InputSystem;
#if LOCALIZATION_EXIST
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
#endif
using PlayerInputManager = AQM.Tools.PlayerInputManager;

public class DialogSystemController : Singleton<DialogSystemController>
{
    
    public static Action<DialogNode> onShowNewDialog;
    public static Action<ChoiceNode> onShowNewChoice;
    public static Action onConversationEnded;
    public static Action<DialogSystemDatabase> onDatabaseCloned;
    
    [SerializeField] private DialogSystemDatabase dialogSystemDatabase;

    public DialogSystemDatabase DialogSystemDatabase => dialogSystemDatabase;

    private ConversationTree _currentConversation;
    private ChoiceNode _currentChoiceNode;
    private PlayerInputActions.UIActions _playerInputs;

    private void Awake()
    {
        ConversationRunner.onStartConversation += StartConversation;
    }

    private void Start()
    {
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
        PlayerInputManager.Instance.ToogleActionMap(PlayerInputManager.Instance.playerInputActions.UI);
        PlayerGeneralEvents.onPlayerStoppedEnter.Invoke();
        
        _playerInputs = PlayerInputManager.Instance.playerInputActions.UI;
        
        _currentConversation = conversationTree;
        _currentChoiceNode = null;
        _currentConversation.onEndConversation += EndConversation;
        
        #if LOCALIZATION_EXIST
            LocalizationSettings.SelectedLocaleChanged += ChangedLocale;
        #endif
        
        Node nextNode = _currentConversation.StartConversation();
        HandleNextNode(nextNode);
    }
    
#if LOCALIZATION_EXIST
    private void ChangedLocale(Locale locale){
        print("Changed locale to " + locale.name);
        if (!_currentConversation) return;
        Node currentNode = _currentConversation.GetCurrentNode();
        if (currentNode)
        {
            DialogNode dialogNode = currentNode as DialogNode;
            if (dialogNode)
            {
                onShowNewDialog?.Invoke(dialogNode);
            }
        
            ChoiceNode choiceNode = currentNode as ChoiceNode;
            if (choiceNode)
            {
                onShowNewChoice?.Invoke(_currentChoiceNode);
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

    private void HandleNextNode(Node nextNode)
    {
        DialogNode dialogNode = nextNode as DialogNode;
        if (dialogNode)
        {
            _playerInputs.Click.started += NextMessage;
            _playerInputs.Submit.started += NextMessage;
            onShowNewDialog?.Invoke(dialogNode);
        }
        
        ChoiceNode choiceNode = nextNode as ChoiceNode;
        if (choiceNode)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            
            _currentChoiceNode = choiceNode;
            _currentChoiceNode.onChoiceSelected += OnChoiceSelected;
            onShowNewChoice?.Invoke(_currentChoiceNode);
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
        Node nextNode = _currentConversation.GetNextNode(option);
        HandleNextNode(nextNode);
    }

    private void EndConversation()
    {
        PlayerInputManager.Instance.ToogleActionMap(PlayerInputManager.Instance.playerInputActions.Player);
        PlayerGeneralEvents.onPlayerStoppedExit.Invoke();
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
        ConversationRunner.onStartConversation -= StartConversation;
    }
}
