// Assets/Scripts/Core/ScriptableObjectRuntimeSaveUtil.cs
using System.Reflection;
using UnityEngine;

public static class ScriptableObjectRuntimeSaveUtil
{
    private static readonly string[] IdMemberNames = { "charmID", "itemID", "ID", "id", "uniqueID", "uniqueId" };
    private static readonly string[] UnlockMemberNames = { "isUnlocked", "unlocked", "IsUnlocked", "Unlocked" };
    
    // NEW: Handle equipped variables
    private static readonly string[] EquipMemberNames = { "isEquipped", "equipped", "IsEquipped", "Equipped" };

    public static string GetId(ScriptableObject so)
    {
        if (so == null) return string.Empty;
        object value = GetMemberValue(so, IdMemberNames);
        if (value != null) return value.ToString();
        return so.name;
    }

    public static bool GetUnlocked(ScriptableObject so)
    {
        if (so == null) return false;
        object value = GetMemberValue(so, UnlockMemberNames);
        return value is bool boolValue && boolValue;
    }

    public static void SetUnlocked(ScriptableObject so, bool value)
    {
        if (so == null) return;
        SetMemberValue(so, UnlockMemberNames, value);
    }

    // --- NEW METHODS FOR EQUIPPED STATE ---
    public static bool GetEquipped(ScriptableObject so)
    {
        if (so == null) return false;
        object value = GetMemberValue(so, EquipMemberNames);
        return value is bool boolValue && boolValue;
    }

    public static void SetEquipped(ScriptableObject so, bool value)
    {
        if (so == null) return;
        SetMemberValue(so, EquipMemberNames, value);
    }

    // Internal reflection logic
    private static object GetMemberValue(object target, string[] candidateNames)
    {
        System.Type type = target.GetType();
        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        for (int i = 0; i < candidateNames.Length; i++)
        {
            FieldInfo field = type.GetField(candidateNames[i], flags);
            if (field != null) return field.GetValue(target);

            PropertyInfo property = type.GetProperty(candidateNames[i], flags);
            if (property != null && property.CanRead) return property.GetValue(target);
        }
        return null;
    }

    private static bool SetMemberValue(object target, string[] candidateNames, bool value)
    {
        System.Type type = target.GetType();
        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        for (int i = 0; i < candidateNames.Length; i++)
        {
            FieldInfo field = type.GetField(candidateNames[i], flags);
            if (field != null && field.FieldType == typeof(bool)) { field.SetValue(target, value); return true; }

            PropertyInfo property = type.GetProperty(candidateNames[i], flags);
            if (property != null && property.CanWrite && property.PropertyType == typeof(bool)) { property.SetValue(target, value); return true; }
        }
        return false;
    }
}