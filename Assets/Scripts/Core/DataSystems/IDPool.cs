using System.Collections.Generic;
using System.Linq;

public class IDPool
{
    private int nextID;
    private readonly Queue<int> reusableIDs = new();

    public IDPool(int startID = 1)
    {
        nextID = startID;
    }

    public int Generate()
    {
        if (reusableIDs.Count > 0)
            return reusableIDs.Dequeue();

        return nextID++;
    }

    public void Release(int ID)
    {
        reusableIDs.Enqueue(ID);
    }

    public void InitializeFromExisting(IEnumerable<int> existingIDs)
    {
        nextID = existingIDs.DefaultIfEmpty(0).Max() + 1;
    }
}