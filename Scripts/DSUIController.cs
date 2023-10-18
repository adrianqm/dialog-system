using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AQM.Tools
{
    public class DSUIController : MonoBehaviour
    {
        [SerializeField] private DialogUIContainer dialogUIContainer;
        [SerializeField] private ChoiceUIContainer choiceUIContainer;
    
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
            
            if (dialogUIContainer)
            {
                dialogUIContainer.gameObject.SetActive(true);
                dialogUIContainer.SetNode(node);
            }
        }

        private void ShowChoiceNode (ChoiceNode choiceNode)
        {
            HideDialogContainer();
            
            if (choiceUIContainer)
            {
                choiceUIContainer.gameObject.SetActive(true);
                choiceUIContainer.SetNode(choiceNode);
            }
        }

        private void HideConversationUI()
        {
            HideDialogContainer();
            HideChoiceContainer();
        }

        private void HideDialogContainer()
        {
            if(dialogUIContainer) dialogUIContainer.gameObject.SetActive(false);
        }

        private void HideChoiceContainer()
        {
            if(choiceUIContainer) choiceUIContainer.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            DialogSystemController.onShowNewDialog -= ShowDialogNode;
            DialogSystemController.onShowNewChoice -= ShowChoiceNode;
            DialogSystemController.onConversationEnded -= HideConversationUI;
        }
    }
}

