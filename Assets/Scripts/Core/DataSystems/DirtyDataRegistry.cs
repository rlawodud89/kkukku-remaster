using System.Collections.Generic;

public class DirtyDataRegistry
{
    private HashSet<IAggregate> dirtySet = new();

    public void RegisterDirty(IAggregate data)
    {
        dirtySet.Add(data);
    }

    public bool HasDirtyData() => dirtySet.Count > 0;

    public IEnumerable<IAggregate> GetAll() => dirtySet;

    public void Clear() => dirtySet.Clear();
}
