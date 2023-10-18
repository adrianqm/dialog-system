using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AQM.Tools
{
    public class ChoiceUIContainer : MonoBehaviour
    {
        [SerializeField] private bool showActorImage;
        [SerializeField] private Image actorImage;
        [SerializeField] private TextMeshProUGUI message;
        [SerializeField] private GameObject choiceContainer;
        [SerializeField] private GameObject buttonPrefab;

        private ChoiceNode _currentChoiceNode;
        
        public void SetNode (ChoiceNode node)
        {
            _currentChoiceNode = node;
            if (actorImage && showActorImage)
            {
                actorImage.gameObject.SetActive(true);
                actorImage.sprite = _currentChoiceNode.actor.actorImage;
            }

            message.text = _currentChoiceNode.message;

            InstantiateChoices();
        }

        private void InstantiateChoices()
        {
            DestroyAllChildren(choiceContainer.transform);
            GameObject firstSelectedButton = null;
            for (int i = 0; i < _currentChoiceNode.choices.Count; i++)
            {
                Choice choice = _currentChoiceNode.choices[i];
                
                //Cast it as a Button, not a game object
                GameObject newButtonGo = Instantiate(buttonPrefab);
                if (newButtonGo)
                {
                    newButtonGo.transform.SetParent(choiceContainer.transform,false);
                    Button button = newButtonGo.GetComponent<Button>();
                    newButtonGo.GetComponentInChildren<TextMeshProUGUI>().text = choice.choiceMessage;
                    var saveIndex = i;
                    var choiceNode = _currentChoiceNode;
                    button.onClick.AddListener(delegate () {  OnChoiceSelected(choiceNode, saveIndex);});

                    if (i != 0) continue;
                    firstSelectedButton = newButtonGo;
                }
            }

            if (firstSelectedButton) // Preselect button
            {
                StartCoroutine(SetDefaultButton(firstSelectedButton));
            }
        }

        private void OnChoiceSelected(ChoiceNode choiceNode, int index)
        {
            choiceNode.onChoiceSelected.Invoke(index);
        }
        
        private static void DestroyAllChildren(Transform t)
        {
            t.transform.Cast<Transform>().ToList().ForEach(c => Destroy(c.gameObject));
        }

        private IEnumerator SetDefaultButton(GameObject firstSelectedButton)
        {
            yield return null;
            if (!firstSelectedButton) yield break;
            var eventSystem = EventSystem.current;
            eventSystem.SetSelectedGameObject(firstSelectedButton, new BaseEventData(eventSystem));
        }
    }
}
