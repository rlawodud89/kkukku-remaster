using UnityEngine;

public class TutorialAnchor : MonoBehaviour
{
    public TutorialID anchorID;

    private void OnEnable()
    {
        AnchorRegistry.Register(anchorID, this);
    }

    private void OnDisable()
    {
        AnchorRegistry.Unregister(anchorID, this);
    }
}
