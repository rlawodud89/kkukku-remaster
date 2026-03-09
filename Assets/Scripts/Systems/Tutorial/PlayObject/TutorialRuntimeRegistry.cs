using System.Collections.Generic;
using UnityEngine;

public static class TutorialRuntimeRegistry
{
    private static Dictionary<TutorialID, Object> objects
        = new();

    public static void Register(TutorialID id, Object obj)
    {
        if (id == TutorialID.None)
            return;

        objects[id] = obj; // 덮어쓰기 허용
    }

    public static void Unregister(TutorialID id, Object obj)
    {
        if (objects.TryGetValue(id, out var current) &&
            current == obj)
        {
            objects.Remove(id);
        }
    }

    public static T Get<T>(TutorialID id) where T : Object
    {
        if (objects.TryGetValue(id, out var obj))
            return obj as T;

        return null;
    }

    public static bool Has(TutorialID id)
    {
        return objects.ContainsKey(id);
    }

    public static void Clear()
    {
        objects.Clear();
    }
}