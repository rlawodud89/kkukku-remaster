using System.Collections.Generic;
using UnityEngine;

public static class AnchorRegistry
{
    private static Dictionary<TutorialAnchorID, TutorialAnchor> anchors
        = new Dictionary<TutorialAnchorID, TutorialAnchor>();

    public static void Register(TutorialAnchorID id, TutorialAnchor anchor)
    {
        if (id == TutorialAnchorID.None)
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

    public static void Unregister(TutorialAnchorID id, TutorialAnchor anchor)
    {
        if (anchors.ContainsKey(id) && anchors[id] == anchor)
        {
            anchors.Remove(id);
        }
    }

    public static TutorialAnchor GetAnchor(TutorialAnchorID id)
    {
        if (anchors.TryGetValue(id, out var anchor))
        {
            return anchor;
        }

        Debug.LogWarning($"Anchor not found: {id}");
        return null;
    }

    public static bool HasAnchor(TutorialAnchorID id)
    {
        return anchors.ContainsKey(id);
    }

    public static void Clear()
    {
        anchors.Clear();
    }
}
