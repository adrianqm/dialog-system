using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AQM.Tools
{
    public class DialogUIContainer : MonoBehaviour
    {
        [SerializeField] private bool showActorImage;
        [SerializeField] private Image actorImage;
        [SerializeField] private TextMeshProUGUI message;

        public void SetNode(DialogNode node)
        {
            if (actorImage && showActorImage)
            {
                actorImage.gameObject.SetActive(true);
                actorImage.sprite = node.actor.actorImage;
            }

            message.text = node.message;
        }
    }
}
