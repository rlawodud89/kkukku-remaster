using System;

public static class TutorialEventBus
{
    private static Action<TutorialID> onEvent;

    public static void Raise(TutorialID id)
    {
        onEvent?.Invoke(id);
    }

    public static void Subscribe(Action<TutorialID> listener)
    {
        onEvent += listener;
    }

    public static void Unsubscribe(Action<TutorialID> listener)
    {
        onEvent -= listener;
    }

    public static void Clear()
    {
        onEvent = null;
    }
}