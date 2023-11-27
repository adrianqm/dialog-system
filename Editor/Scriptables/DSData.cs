using UnityEditor;
using UnityEditor.Localization;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

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
