using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditorForks;
#if LOCALIZATION_EXIST
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
#endif

namespace AQM.Tools
{
    public class DSData : ScriptableSingleton<DSData>
    {
        public DialogSystemDatabase database;
        public bool debugMode;
        #if LOCALIZATION_EXIST
            public StringTableCollection tableCollection;
            public StringTable defaultStringTable;
        #endif
    }
}
