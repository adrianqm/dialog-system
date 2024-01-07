using UnityEditor;
using UnityEngine.UIElements;

public static class UIToolkitLoader
{
    public static VisualTreeAsset LoadUXML(string editorWindowPath, string uxmlName)
    {
        string path = $"{editorWindowPath}/Views/{uxmlName}/{uxmlName}.uxml";
        return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
    }
    
    public static StyleSheet LoadStyleSheet(string editorWindowPath, string styleSheetName)
    {
        string path = $"{editorWindowPath}/Views/{styleSheetName}/{styleSheetName}.uss";
        return AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
    }
}