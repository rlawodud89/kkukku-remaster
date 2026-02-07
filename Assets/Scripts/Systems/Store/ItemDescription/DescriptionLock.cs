using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DescriptionLock
{
    public static PanelItemImg currentOwner { get; private set; }

    public static void Reset()
    {
        currentOwner = null;
    }


    public static bool TryAcquire(PanelItemImg requester)
    {
        if (currentOwner != null) return false;

        currentOwner = requester;
        return true;
    }

    public static void Release(PanelItemImg requester)
    {
        if (currentOwner == requester)
            currentOwner = null;
    }

    public static bool IsOwner(PanelItemImg requester)
    {
        return currentOwner == requester;
    }

}

