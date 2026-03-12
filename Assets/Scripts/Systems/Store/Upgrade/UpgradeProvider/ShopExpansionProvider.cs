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
        // 1. 업그레이드 전(과거)의 그리드 정보 DB에서 가져오기
        // ✏️ (주의) 유저님의 실제 그리드 정보 가져오는 함수로 변경해주세요!
        //var oldGrid = ServiceLocator.Get<GameData>().ShopInfo.GetShopGridData(currentLevel);

        // 2. 레벨업! (DB 반영)
        ServiceLocator.Get<GameData>().User.ChangeShopLevel(1);
        currentLevel++;

        // 3. 업그레이드 후(새로운)의 그리드 정보 DB에서 가져오기
        //var newGrid = ServiceLocator.Get<GameData>().ShopInfo.GetShopGridData(currentLevel);

        // 4. DB 데이터를 바탕으로 가구 위치 일괄 재계산 및 덮어쓰기!
        //UpdateFurnitureGridInDB(oldGrid.StartPos, oldGrid.Width, newGrid.StartPos, newGrid.Width);

        // 5. 완료 이벤트 호출
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

