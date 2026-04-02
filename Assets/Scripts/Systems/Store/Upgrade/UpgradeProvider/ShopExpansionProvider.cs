using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopExpansionProvider : IUpgradeProvider
{
    public string levelName { get; private set; } = "가게 확장 레벨";
    public int currentLevel { get; private set; }
    public int maxLevel { get; private set; } = 3;
    public bool isGold { get; private set; } = true;

    private Dictionary<int, int> upgradePrice = new();

    public ShopExpansionProvider()
    {
        upgradePrice.Add(1, 8000);
        upgradePrice.Add(2, 10000);

        currentLevel = ServiceLocator.Get<GameData>().User.GetShopLevel().level;
    }

    public int GetUpgradePrice()
    {
        if (upgradePrice.ContainsKey(currentLevel)) return upgradePrice[currentLevel];
        return -1;
    }


    public void LevelUpgrade()
    {

        int oldwidth, oldheight;
        Vector3 oldstartPos = new();
        ServiceLocator.Get<GameData>().User.GetCurrentShopGridSize(out oldwidth, out oldheight, out oldstartPos);

        
        // 🔎 1번 로그: 업그레이드 전 크기 확인
        Debug.Log($"<color=orange>[확장 전]</color> 레벨: {currentLevel}, 크기: {oldwidth}x{oldheight}");

        ServiceLocator.Get<GameData>().User.ChangeShopLevel(1);
        currentLevel++;

        int newwidth, newheight;
        Vector3 newstartPos = new();
        ServiceLocator.Get<GameData>().User.GetCurrentShopGridSize(out newwidth, out newheight, out newstartPos);

        // 🔎 2번 로그: 업그레이드 후 크기 확인 (여기서 크기가 안 늘어났으면 DB 함수가 범인!)
        Debug.Log($"<color=green>[확장 후]</color> 레벨: {currentLevel}, 크기: {newwidth}x{newheight}");

        // 🔎 3번 로그: 재계산 함수로 넘어가는지 확인
        UpdateFurnitureGridInDB(Vector3Int.RoundToInt(oldstartPos), oldwidth, Vector3Int.RoundToInt(newstartPos), newwidth);

        UpgradeEvents.OnUpgradeLevelChanged?.Invoke(this);
    }

    // =========================================================
    // ★ 가구 위치 변환 및 DB 전송 로직
    // =========================================================
    private void UpdateFurnitureGridInDB(Vector3Int oldStartPos, int oldWidth, Vector3Int newStartPos, int newWidth)
    {
        var placedItems = ServiceLocator.Get<GameData>().Interior.GetCurrentShopInterior();
        Debug.Log($"<color=yellow>[상점 확장]</color> 총 {placedItems.Count}개 가구 위치 갱신 시작.");

        foreach (var placed in placedItems)
        {
            // 1) 예전 위치와 새 위치 데이터를 바탕으로 새로운 Index 번호 계산
            int newGridNumber = RecalculateIndex(placed.gridNumber, oldStartPos, oldWidth, newStartPos, newWidth);

            // 2) DB에 바뀐 위치 즉시 전송
            ServiceLocator.Get<GameData>().Interior.TransferShopInterior(placed.ID, newGridNumber);
        }

        Debug.Log("<color=cyan>[가구 이동 완료]</color> 모든 가구가 확장된 맵의 올바른 좌표로 갱신되었습니다!");
    }

    private void UpdateRoomFurnitureGridInDB(Vector3Int oldStartPos, int oldWidth, Vector3Int newStartPos, int newWidth)
    {
        // 💡 주의: 방 인테리어를 가져오는 함수명으로 맞춰주세요!
        var placedItems = ServiceLocator.Get<GameData>().Interior.GetCurrentRoomInterior();
        if (placedItems == null || placedItems.Count == 0) return;

        Debug.Log($"<color=yellow>[방 확장]</color> 총 {placedItems.Count}개 가구 위치 갱신 시작.");

        foreach (var placed in placedItems)
        {
            int newGridNumber = RecalculateIndex(placed.gridNumber, oldStartPos, oldWidth, newStartPos, newWidth);
            
            // 💡 주의: 방 인테리어 위치를 옮기는(저장하는) 함수명으로 맞춰주세요!
            ServiceLocator.Get<GameData>().Interior.TransferRoomInterior(placed.ID, newGridNumber);
        }
        Debug.Log("<color=cyan>[방 가구 이동 완료]</color> 올바른 좌표로 갱신되었습니다!");
    }
    
    // 예전 Index를 새로운 Index로 변환하는 핵심 수학 로직
    private int RecalculateIndex(int oldIndex, Vector3Int oldStartPos, int oldWidth, Vector3Int newStartPos, int newWidth)
    {
        // A. 예전 맵 기준의 절대 좌표(X, Y) 복구
        int oldOffsetX = oldIndex % oldWidth;
        int oldOffsetY = oldIndex / oldWidth;

        int cellX = oldStartPos.x + oldOffsetX;
        int cellY = oldStartPos.y - oldOffsetY; // Top-Left 기준

        // B. 새로운 맵 기준의 Offset으로 다시 변환
        int newOffsetX = cellX - newStartPos.x;
        int newOffsetY = newStartPos.y - cellY; // Top-Left 기준

        // C. 새로운 Index 번호 리턴
        return (newOffsetY * newWidth) + newOffsetX;
    }
}

