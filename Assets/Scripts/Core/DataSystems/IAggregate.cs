using System.Collections.Generic;

public interface IAggregate
{
    public bool IsDirty { get; }
    public void ClearDirty();
    public IEnumerable<SavePayload> ToSavePayloads();
}
