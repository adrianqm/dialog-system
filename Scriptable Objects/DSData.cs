using UnityEditor;
#if LOCALIZATION_EXIST
using UnityEditor.Localization;
using UnityEngine.Localization;
#endif

public class DSData : ScriptableSingleton<DSData>
{
    public DialogSystemDatabase database;
    public bool debugMode;
}