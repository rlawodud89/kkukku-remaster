using SQLite4Unity3d;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    private SaveService saveService;
    private string dbPath;
    private string testdbPath;

    void Awake()
    {
        DontDestroyOnLoad(this);

        dbPath = Path.Combine(Application.persistentDataPath, "kkukkuDB.db");
        testdbPath = Path.Combine(Application.streamingAssetsPath, "kkukkuDB.db");

        // TODO: 모바일 환경에 맞춘 dbPath로 변경 필요
        bool isNewUser = !File.Exists(testdbPath);

        var connection = new SQLiteConnection(testdbPath);
        var repository = new SaveRepository(connection);
        var dirtyRegistry = new DirtyDataRegistry();

        // 신규 유저 처리
        if (isNewUser)
        {
            Debug.Log("신규 유저 - DB 생성");
            repository.MakeDefaultDB();
        }

        var gameData = repository.LoadAll();

        saveService = new SaveService(repository, dirtyRegistry);

        ServiceLocator.Register(saveService);
        ServiceLocator.Register(repository);
        ServiceLocator.Register(dirtyRegistry);
        ServiceLocator.Register(gameData);

        if (isNewUser)
        {
            Debug.Log("초기 데이터 세팅");
            gameData.Store.ResetAllStoreItemList();
            saveService.SaveNow();
        }
    }

    void Update()
    {
        saveService.Tick(Time.deltaTime);
    }
}
