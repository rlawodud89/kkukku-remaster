using UnityEngine;

public class TutorialAnchor : MonoBehaviour
{
    public TutorialAnchorID anchorID;

    private void OnEnable()
    {
        AnchorRegistry.Register(anchorID, this);
    }

    private void OnDisable()
    {
        AnchorRegistry.Unregister(anchorID, this);
    }
}
