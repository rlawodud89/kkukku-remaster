using SQLite4Unity3d;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    private SaveService saveService;
    private static string testdbPath = Path.Combine(Application.streamingAssetsPath, "kkukkuDB.db");


    void Awake()
    {
        DontDestroyOnLoad(this);

        var connection = new SQLiteConnection(testdbPath);
        var repository = new SaveRepository(connection);
        var dirtyRegistry = new DirtyDataRegistry();
        var gameData = repository.LoadAll();

        saveService = new SaveService(repository, dirtyRegistry);

        ServiceLocator.Register(saveService);
        ServiceLocator.Register(repository);
        ServiceLocator.Register(dirtyRegistry);
        ServiceLocator.Register(gameData);
    }

    void Update()
    {
        saveService.Tick(Time.deltaTime);
    }
}
