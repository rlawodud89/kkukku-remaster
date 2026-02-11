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

        // TODO: 모바일 환경에 맞춘 dbPath로 변경 필요
        var connection = new SQLiteConnection(testdbPath);
        var repository = new SaveRepository(connection);
        var dirtyRegistry = new DirtyDataRegistry();

        // TODO: 새로운 유저인 경우, 초기 데이터 DB 생성
        // repository.MakeDefaultDB();

        var gameData = repository.LoadAll();

        saveService = new SaveService(repository, dirtyRegistry);

        ServiceLocator.Register(saveService);
        ServiceLocator.Register(repository);
        ServiceLocator.Register(dirtyRegistry);
        ServiceLocator.Register(gameData);

        // TODO: 새로운 유저인 경우, 판매 아이템 데이터 생성
        // gameData.Store.ResetAllStoreItemList();
    }

    void Update()
    {
        saveService.Tick(Time.deltaTime);
    }
}
