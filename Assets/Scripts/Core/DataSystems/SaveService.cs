using UnityEngine;

public class SaveService
{
    private readonly SaveRepository repository;
    private readonly DirtyDataRegistry dirtyRegistry;

    private float saveInterval = 30f;
    private float timer;

    public SaveService(
        SaveRepository repository,
        DirtyDataRegistry dirtyRegistry)
    {
        this.repository = repository;
        this.dirtyRegistry = dirtyRegistry;
    }

    public void Tick(float deltaTime)
    {
        timer += deltaTime;
        if (timer >= saveInterval)
        {
            Flush();
            timer = 0f;
        }
    }

    public void Flush()
    {
        if (!dirtyRegistry.HasDirtyData()) return;

        repository.BeginTransaction();

        try
        {
            foreach (var data in dirtyRegistry.GetAll())
                repository.Save(data);

            repository.Commit();
            dirtyRegistry.Clear();
        }
        catch
        {
            repository.Rollback();
            throw;
        }
    }
}
