using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DescriptionLock
{
    public static CountableItemPanel currentCountableOwner { get; private set; }
    public static CountlessItemPanel currentCountlessOwner { get; private set; }

    public static bool TryAcquire(CountableItemPanel requester)
    {
        if (currentCountableOwner != null) return false;

        currentCountableOwner = requester;
        return true;
    }

    public static bool TryAcquire(CountlessItemPanel requester)
    {
        if (currentCountlessOwner != null) return false;

        currentCountlessOwner = requester;
        return true;
    }

    public static void Release(CountableItemPanel requester)
    {
        if (currentCountableOwner == requester)
            currentCountableOwner = null;
    }

    public static void Release(CountlessItemPanel requester)
    {
        if (currentCountlessOwner == requester)
            currentCountlessOwner = null;
    }

    public static bool IsOwner(CountableItemPanel requester)
    {
        return currentCountableOwner == requester;
    }

    public static bool IsOwner(CountlessItemPanel requester)
    {
        return currentCountlessOwner == requester;
    }
}

