using UnityEditor;
using UnityEngine;
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
    
    public static Sprite LoadSprite(string editorWindowPath, string spriteName)
    {
        string path = $"{editorWindowPath}/Assets/{spriteName}";
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
    
    public static Texture2D LoadTexture2D (string editorWindowPath, string spriteName)
    {
        string path = $"{editorWindowPath}/Assets/{spriteName}";
        return (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
    }
    
}