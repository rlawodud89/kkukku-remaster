//using System.Collections.Generic;

//public class QuestAggregate : IAggregate
//{
//    private Dictionary<string, QuestSO> quests = new();

//    private HashSet<string> inserted = new();
//    private HashSet<string> updated = new();
//    private HashSet<string> deleted = new();

//    public bool IsDirty { get; private set; }

//    private void MarkDirty()
//    {
//        IsDirty = true;
//        ServiceLocator.Get<DirtyDataRegistry>().RegisterDirty(this);
//    }

//    public void ClearDirty() => IsDirty = false;

//    public IEnumerable<SavePayload> ToSavePayloads()
//    {

//    }
//}
