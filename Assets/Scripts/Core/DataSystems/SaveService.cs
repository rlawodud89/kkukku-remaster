using UnityEngine;

public class SaveService
{
    private readonly SaveRepository repository;
    private readonly DirtyDataRegistry dirtyRegistry;

    private bool isAutoSaveEnabled = false;
    private float saveInterval = 180f;  // 3분(180초)에 한번씩 저장
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
        if (!isAutoSaveEnabled) return;

        timer += deltaTime;
        if (timer >= saveInterval)
        {
            Flush();
            timer = 0f;
        }
    }

    private void Flush()
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

    public void SaveNow()
    {
        Flush();
        timer = 0f;
    }

    public void SetAutoSave(bool enabled)
    {
        isAutoSaveEnabled = enabled;
        timer = 0f;
    }

    public void ResetTimer()
    {
        timer = 0f;
    }

    public float GetCurrentTimer()
    {
        return timer;
    }
}
