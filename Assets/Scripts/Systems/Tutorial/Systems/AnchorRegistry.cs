using System.Collections.Generic;
using UnityEngine;

public static class AnchorRegistry
{
    private static Dictionary<TutorialID, TutorialAnchor> anchors
        = new Dictionary<TutorialID, TutorialAnchor>();

    public static void Register(TutorialID id, TutorialAnchor anchor)
    {
        if (id == TutorialID.None)
        {
            Debug.LogWarning("Invalid Anchor ID: None");
            return;
        }

        if (anchors.ContainsKey(id))
        {
            Debug.LogWarning($"Duplicate Anchor ID detected: {id}");
            return;
        }

        anchors.Add(id, anchor);
    }

    public static void Unregister(TutorialID id, TutorialAnchor anchor)
    {
        if (anchors.ContainsKey(id) && anchors[id] == anchor)
        {
            anchors.Remove(id);
        }
    }

    public static TutorialAnchor GetAnchor(TutorialID id)
    {
        if (anchors.TryGetValue(id, out var anchor))
        {
            return anchor;
        }

        Debug.LogWarning($"Anchor not found: {id}");
        return null;
    }

    public static bool HasAnchor(TutorialID id)
    {
        return anchors.ContainsKey(id);
    }

    public static void Clear()
    {
        anchors.Clear();
    }
}
