using System;

public static class TutorialEventBus
{
    public static Action<TutorialID> OnEvent;

    public static void Raise(TutorialID ID)
    {
        OnEvent?.Invoke(ID);
    }
}