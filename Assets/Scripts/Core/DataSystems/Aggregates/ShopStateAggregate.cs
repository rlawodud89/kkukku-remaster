using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopStateAggregate : IAggregate
{
    public bool IsDirty { get; private set; }

    private void MarkDirty()
    {
        IsDirty = true;
        ServiceLocator.Get<DirtyDataRegistry>().RegisterDirty(this);
    }

    public void ClearDirty()
    {
        IsDirty = false;


    }

    public IEnumerable<SavePayload> ToSavePayloads()
    {

    }
}
