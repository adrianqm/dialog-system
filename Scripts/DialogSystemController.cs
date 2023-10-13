using System;
using System.Collections;
using System.Collections.Generic;
using AQM.Tools;
using UnityEngine;
using UnityEngine.InputSystem;
using PlayerInputManager = AQM.Tools.PlayerInputManager;

public class DialogSystemController : MonoBehaviour
{
    
    public static Action<DialogNode> onShowNewDialog;
    public static Action<ChoiceNode> onShowNewChoice;
    public static Action onConversationEnded;
    public static Action<DialogSystemDatabase> onDatabaseCloned;
    
    [SerializeField] private DialogSystemDatabase dialogSystemDatabase;

    public DialogSystemDatabase DialogSystemDatabase => dialogSystemDatabase;

    private ConversationTree _currentConversation;
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

    private void StartConversation(ConversationTree conversationTree)
    {
        PlayerInputManager.Instance.ToogleActionMap(PlayerInputManager.Instance.playerInputActions.UI);
        PlayerGeneralEvents.onPlayerStoppedEnter.Invoke();
        
        _playerInputs = PlayerInputManager.Instance.playerInputActions.UI;
        _playerInputs.Click.performed += NextMessage;
        
        _currentConversation = conversationTree;
        _currentConversation.OnEndConversation += EndConversation;
        
        Node nextNode = _currentConversation.StartConversation();
        HandleNextNode(nextNode);
    }

    private void NextMessage(InputAction.CallbackContext context)
    {
        Node nextNode = _currentConversation.GetNextNode();
        HandleNextNode(nextNode);
    }

    private void HandleNextNode(Node nextNode)
    {
        DialogNode dialogNode = nextNode as DialogNode;
        if (dialogNode)
        {
            onShowNewDialog?.Invoke(dialogNode);
        }
    }

    private void EndConversation()
    {
        PlayerInputManager.Instance.ToogleActionMap(PlayerInputManager.Instance.playerInputActions.Player);
        PlayerGeneralEvents.onPlayerStoppedExit.Invoke();
        _playerInputs.Click.performed -= NextMessage;
        _currentConversation.OnEndConversation -= EndConversation;
        onConversationEnded?.Invoke();
    }

    private void OnDestroy()
    {
        ConversationRunner.onStartConversation -= StartConversation;
    }
}
