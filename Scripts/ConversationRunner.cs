
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace AQM.Tools
{
    public class ConversationRunner : InteractableBase
    {
        [FormerlySerializedAs("dialogTree")] public ConversationTree conversationTree;
        private PlayerInputActions.UIActions _playerInputs;
        
        private void Start()
        {
            conversationTree = conversationTree.Clone();
        }

        public override void Interact()
        {
            PlayerInputManager.Instance.ToogleActionMap(PlayerInputManager.Instance.playerInputActions.UI);
            _playerInputs = PlayerInputManager.Instance.playerInputActions.UI;
            _playerInputs.Click.performed += NextMessage;
            conversationTree.StartConversation();
            conversationTree.OnEndConversation += EndConversation;
            PlayerGeneralEvents.onPlayerStoppedEnter.Invoke();
        }

        private void NextMessage(InputAction.CallbackContext context)
        {
            conversationTree.NextMessage();
        }

        private void EndConversation()
        {
            _playerInputs.Click.performed -= NextMessage;
            PlayerInputManager.Instance.ToogleActionMap(PlayerInputManager.Instance.playerInputActions.Player);
            conversationTree.OnEndConversation -= EndConversation;
            PlayerGeneralEvents.onPlayerStoppedExit.Invoke();
        }
    }
}
 