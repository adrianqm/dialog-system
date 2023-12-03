using System.Collections;
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
        [SerializeField] private Slider _slider;

        private DSChoice _currentChoiceNode;
        private Coroutine _choiceCo;
        private bool _stopTimer = false;
        
        public void SetNode (DSChoice node, float choiceSeconds = 0f)
        {
            _currentChoiceNode = node;
            _slider.enabled = false;
            if (actorImage && showActorImage)
            {
                actorImage.gameObject.SetActive(true);
                actorImage.sprite = _currentChoiceNode.Actor.actorImage;
            }

            message.text = _currentChoiceNode.Message;

            InstantiateChoices();

            if (choiceSeconds > 0)
            {
                _slider.enabled = true;
                _slider.maxValue = choiceSeconds;
                _slider.value = choiceSeconds;
                _stopTimer = false;
                if(_choiceCo != null)  StopCoroutine(_choiceCo);
                _choiceCo = StartCoroutine(NoResponseCoroutine(choiceSeconds));
            }
        }

        private void InstantiateChoices()
        {
            DestroyAllChildren(choiceContainer.transform);
            GameObject firstSelectedButton = null;
            for (int i = 0; i < _currentChoiceNode.Choices.Count; i++)
            {
                string choice = _currentChoiceNode.Choices[i];
                
                //Cast it as a Button, not a game object
                GameObject newButtonGo = Instantiate(buttonPrefab);
                if (newButtonGo)
                {
                    newButtonGo.transform.SetParent(choiceContainer.transform,false);
                    Button button = newButtonGo.GetComponent<Button>();
                    newButtonGo.GetComponentInChildren<TextMeshProUGUI>().text = choice;
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
        
        private IEnumerator NoResponseCoroutine(float choiceTime)
        {
            while (_stopTimer == false)
            {
                choiceTime -= Time.deltaTime;
                yield return new WaitForSeconds(0.001f);
                if (choiceTime <= 0) _stopTimer = true;
                if (_stopTimer == false) _slider.value = choiceTime;
            }
            
            OnChoiceSelected(_currentChoiceNode, -1);
        }

        private void OnChoiceSelected(DSChoice choiceNode, int index)
        {
            if(_choiceCo != null) StopCoroutine(_choiceCo);
            choiceNode.onChoiceSelected.Invoke(index);
        }

        public void DestroyAllChildren(Transform t)
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
