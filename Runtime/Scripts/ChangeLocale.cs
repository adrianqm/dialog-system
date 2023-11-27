using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;

namespace AQM.Tools
{
    public class ChangeLocale : MonoBehaviour
    {
#if LOCALIZATION_EXIST

        public CanvasGroup group;
        public TextMeshProUGUI tmp;
        [SerializeField] private InputActionReference changeLocale;


        private List<String> options = new();
        private int selected;
        
        IEnumerator Start()
        {
            yield return LocalizationSettings.InitializationOperation;
            for (int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; ++i)
            {
                var locale = LocalizationSettings.AvailableLocales.Locales[i];
                if (LocalizationSettings.SelectedLocale == locale)
                    selected = i;
                options.Add(locale.name);
            }
            tmp.text = options[selected];
            group.alpha = 1;
        }

        private void Update()
        {
            if (!changeLocale.action.WasPressedThisFrame()) return;
            selected = (selected + 1) % options.Count;
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[selected];
            tmp.text = options[selected];
        }

        private void OnEnable()
        {
            changeLocale.action.Enable();
        }

        private void OnDisable()
        {
            changeLocale.action.Disable();
        }


#endif
    }  
}

