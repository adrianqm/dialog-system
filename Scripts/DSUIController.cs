using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AQM.Tools
{
    public class DSUIController : MonoBehaviour
    {
        [SerializeField] private DialogUIContainer _dialogUIContainer;
        [SerializeField] private ChoiceUIContainer _choiceUIContainer;
    
        private void Awake()
        {
            DialogSystemController.onShowNewDialog += ShowDialogNode;
            DialogSystemController.onShowNewChoice += ShowChoiceNode;
            DialogSystemController.onConversationEnded += HideConversationUI;
            
            HideDialogContainer();
            HideChoiceContainer();
        }

        private void ShowDialogNode(DialogNode node)
        {
            HideChoiceContainer();
            
            if (_dialogUIContainer)
            {
                _dialogUIContainer.gameObject.SetActive(true);
                _dialogUIContainer.SetNode(node);
            }
        }

        private void ShowChoiceNode (ChoiceNode choiceNode)
        {
            HideDialogContainer();
            
            if (_choiceUIContainer)
            {
                _choiceUIContainer.gameObject.SetActive(true);
                _choiceUIContainer.SetNode(choiceNode);
            }
        }

        private void HideConversationUI()
        {
            HideDialogContainer();
        }

        private void HideDialogContainer()
        {
            if(_dialogUIContainer) _dialogUIContainer.gameObject.SetActive(false);
        }

        private void HideChoiceContainer()
        {
            if(_choiceUIContainer) _choiceUIContainer.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            DialogSystemController.onShowNewDialog -= ShowDialogNode;
            DialogSystemController.onShowNewChoice -= ShowChoiceNode;
            DialogSystemController.onConversationEnded -= HideConversationUI;
        }
    }
}

