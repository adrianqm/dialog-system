using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
#if LOCALIZATION_EXIST
using UnityEditor.Localization;
using UnityEngine.Localization.Tables;
using UnityEngine.Localization.Settings;
#endif

namespace AQM.Tools
{
    public static class LocalizationUtils
    {
        #if LOCALIZATION_EXIST
        public static string GetLocalizedString(StringTable table, string entryName)
        {
            // Get the table entry. The entry contains the localized string and Metadata
            var entry = table.GetEntry(entryName);
            if (entry == null)
            {
                entry = table.AddEntry(entryName, "");
                RefreshStringTableCollection(DSData.instance.tableCollection,table);
            }
            // We can pass in optional arguments for Smart Format or String.Format here.
            string value = entry.GetLocalizedString();
            return value??""; 
        }
        
        public static string GetDefaultLocaleLocalizedString(string entryName)
        {
            StringTable defaultStringTable = DSData.instance.defaultStringTable;
            if (!defaultStringTable) return "";
            return GetLocalizedString(defaultStringTable, entryName);
        }
        public static void SetDefaultLocaleEntry(string entryName, string value)
        {
            StringTableCollection collection = DSData.instance.tableCollection;
            StringTable defaultStringTable = DSData.instance.defaultStringTable;
            if (!collection || !defaultStringTable) return;
            Undo.RecordObject(collection.SharedData, "Add new key");
            //collection.SharedData.AddKey(entryName);
            defaultStringTable.AddEntry(entryName, value);
            RefreshStringTableCollection(collection,defaultStringTable);
        }
        
        public static void AddCopyKeyToCollection(string newKey, string copyKey)
        {
            StringTableCollection collection = DSData.instance.tableCollection;
            if (!collection) return;
            Undo.RecordObject(collection.SharedData, "Add new key");
            collection.SharedData.AddKey(newKey);
            foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
            {
                StringTable table = LocalizationSettings.StringDatabase.GetTable(collection.name, locale);
                string translation = GetLocalizedString(table, copyKey);
                table.AddEntry(newKey, translation);
            }
        }
        
        public static void RemoveKeysFromCollection(List<string> entryNames)
        {
            StringTableCollection collection = DSData.instance.tableCollection;
            if (!collection) return;
            var objects = new Object[collection.Tables.Count + 1];
            for (int i = 0; i < collection.Tables.Count; ++i)
            {
                objects[i] = collection.Tables[i].asset;
            }
            objects[collection.Tables.Count] = collection.SharedData;
            Undo.RecordObjects(objects, "Remove key from collection");
            foreach (var key in entryNames)
            {
                collection.RemoveEntry(key);
            }
            foreach (var o in objects)
                EditorUtility.SetDirty(o);
        }
        
        public static void RemoveKeyFromCollection(string entryName)
        {
            StringTableCollection collection = DSData.instance.tableCollection;
            if (!collection) return;
            var objects = new Object[collection.Tables.Count + 1];
            for (int i = 0; i < collection.Tables.Count; ++i)
            {
                objects[i] = collection.Tables[i].asset;
            }
            objects[collection.Tables.Count] = collection.SharedData;
            Undo.RecordObjects(objects, "Remove key from collection");
            collection.RemoveEntry(entryName);
            foreach (var o in objects)
                EditorUtility.SetDirty(o);
        }

        public static void RefreshStringTableCollection(StringTableCollection collection, StringTable table = null)
        {
            EditorUtility.SetDirty(collection);
            EditorUtility.SetDirty(collection.SharedData);
            if (table)
            { 
                EditorUtility.SetDirty(table);
                EditorUtility.SetDirty(table.SharedData);
            }
            if(DSData.instance.debugMode) AssetDatabase.SaveAssets();
        }
        #endif
    }
}

