using UnityEngine;

namespace AQM.Tools
{
    public class DSUIController : MonoBehaviour
    {
        [SerializeField] private GameObject dialogUIGo;
        [SerializeField] private GameObject choiceUIGo;
        private DialogUIContainer _dialogUIContainer;
        private ChoiceUIContainer _choiceUIContainer;
        private CanvasGroup _dialogCanvasGroup;
        private CanvasGroup  _choiceCanvasGroup;
    
        private void Awake()
        {
            _dialogUIContainer = dialogUIGo.GetComponent<DialogUIContainer>();
            _choiceUIContainer = choiceUIGo.GetComponent<ChoiceUIContainer>();
            _dialogCanvasGroup = dialogUIGo.GetComponent<CanvasGroup>();
            _choiceCanvasGroup = choiceUIGo.GetComponent<CanvasGroup>();
            DialogSystemController.onShowNewDialog += ShowDialogNode;
            DialogSystemController.onShowNewChoice += ShowChoiceNode;
            DialogSystemController.onConversationEnded += HideConversationUI;
            
            HideDialogContainer();
            HideChoiceContainer();
        }

        private void ShowDialogNode(DSDialog node)
        {
            HideChoiceContainer();

            if (!_dialogUIContainer) return;
            _dialogUIContainer.SetNode(node);
            _dialogCanvasGroup.alpha = 1f;
        }

        private void ShowChoiceNode (DSChoice choiceNode)
        {
            HideDialogContainer();

            if (!_choiceUIContainer) return;
            _choiceUIContainer.SetNode(choiceNode);
            _choiceCanvasGroup.alpha = 1f;
            _choiceCanvasGroup.interactable = true;
        }

        private void HideConversationUI()
        {
            HideDialogContainer();
            HideChoiceContainer();
        }

        private void HideDialogContainer()
        {
            if(_dialogUIContainer) _dialogCanvasGroup.alpha = 0f;
        }

        private void HideChoiceContainer()
        {
            if (_choiceUIContainer)
            {
                _choiceCanvasGroup.alpha = 0f;
                _choiceCanvasGroup.interactable = false;
            }
        }

        private void OnDestroy()
        {
            DialogSystemController.onShowNewDialog -= ShowDialogNode;
            DialogSystemController.onShowNewChoice -= ShowChoiceNode;
            DialogSystemController.onConversationEnded -= HideConversationUI;
        }
    }
}

