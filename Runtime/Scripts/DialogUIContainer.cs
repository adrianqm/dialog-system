using System;
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

        public void SetNode(DSDialog node)
        {
            if (actorImage && showActorImage)
            {
                actorImage.gameObject.SetActive(true);
                actorImage.sprite = node.Actor.actorImage;
            }

            message.text = node.Message;
        }
    }
}
