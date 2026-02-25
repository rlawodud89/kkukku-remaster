using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class InventoryManager : MonoBehaviour
{
    [Header("슬롯 8개 연결")]
    public ItemSlot[] slots = new ItemSlot[8];

    [Header("아이템 리스트 (데이터)")]
    public List<FurnitureItem> furnitureList = new List<FurnitureItem>();
    public List<FloorItem> floorList = new List<FloorItem>();
    public List<WallpaperItem> wallpaperList = new List<WallpaperItem>();

    public Tilemap floorTilemap; // 바닥을 깔 타일맵
    public Tilemap wallTilemap;  // 벽지를 깔 타일맵

    [Header("현재 장착 중인 아이템 추적")]
    public string currentFloorName = "";       // 현재 깔려있는 바닥 이름
    public string currentWallpaperName = "";   // 현재 발려있는 벽지 이름

    private int currentPage = 0;
    private int itemsPerPage = 8;

    // 현재 보고 있는 카테고리 (0: 가구, 1: 타일, 2: 벽지)
    private int currentCategory = 0;

    public void OnClickInteriorButton()
    {
        // 1. 현재 장착 중인 바닥 타일 이름 가져오기
        FloorItem currentFloor = ServiceLocator.Get<GameData>().Interior.GetCurrentFloorTile(TilePositionType.SHOP_FLOOR);
        if (currentFloor != null)
        {
            currentFloorName = currentFloor.itemName;
        }

        // 2. 현재 장착 중인 벽지 타일 이름 가져오기
        WallpaperItem currentWallpaper = ServiceLocator.Get<GameData>().Interior.GetCurrentWallTile(TilePositionType.SHOP_WALL);
        if (currentWallpaper != null)
        {
            currentWallpaperName = currentWallpaper.itemName;
        }

        furnitureList = ServiceLocator.Get<GameData>().Inventory.GetShopInteriorItemInventory();
        floorList = ServiceLocator.Get<GameData>().Inventory.GetFloorTileItemInventory();
        wallpaperList = ServiceLocator.Get<GameData>().Inventory.GetWallTileItemInventory();
    }


    // --- 카테고리(탭) 버튼을 눌렀을 때 실행될 함수들 ---
    public void OnClickFurnitureTab()
    {
        currentCategory = 0; currentPage = 0; RefreshUI();
    }
    public void OnClickTileTab()
    {
        currentCategory = 1; currentPage = 0; RefreshUI();
    }
    public void OnClickWallpaperTab()
    {
        currentCategory = 2; currentPage = 0; RefreshUI();
    }

    // --- 화면 그리기 로직 ---
    public void RefreshUI()
    {
        int startIndex = currentPage * itemsPerPage;

        for (int i = 0; i < slots.Length; i++)
        {
            int itemIndex = startIndex + i;

            if (currentCategory == 0) // 가구 탭
            {
                if (itemIndex < furnitureList.Count)
                {
                    var item = furnitureList[itemIndex];
                    // 가구는 isEquipped 자리에 false 전달 (개수 0일 때 잠기는 건 ItemSlot이 알아서 함)
                    slots[i].UpdateSlot(item.itemImage, item.itemName, 0, item.quantity, true, false);
                }
                else { slots[i].UpdateSlot(null, "", 0, 0, false, false); }
            }
            else if (currentCategory == 1) // 타일 탭
            {
                if (itemIndex < floorList.Count)
                {
                    var item = floorList[itemIndex];
                    // ✨ 현재 깔려있는 바닥 이름과 일치하는지 확인
                    bool isEquipped = (item.itemName == currentFloorName);
                    slots[i].UpdateSlot(item.itemImage, item.itemName, 1, 0, false, isEquipped);
                }
                else { slots[i].UpdateSlot(null, "", 1, 0, false, false); }
            }
            else if (currentCategory == 2) // 벽지 탭
            {
                if (itemIndex < wallpaperList.Count)
                {
                    var item = wallpaperList[itemIndex];
                    // ✨ 현재 발려있는 벽지 이름과 일치하는지 확인
                    bool isEquipped = (item.itemName == currentWallpaperName);
                    slots[i].UpdateSlot(item.itemImage, item.itemName, 2, 0, false, isEquipped);
                }
                else { slots[i].UpdateSlot(null, "", 2, 0, false, false); }
            }
        }
    }


    public void PlaceTileOnMap(string targetName, int category, Vector3 mousePosition)
    {
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(mousePosition);
        worldPoint.z = 0;

        // 마우스를 놓은 위치를 타일맵 좌표로 변환
        Vector3Int dropFloorPos = floorTilemap.WorldToCell(worldPoint);
        Vector3Int dropWallPos = wallTilemap.WorldToCell(worldPoint);

        if (category == 1) // ================= 바닥 타일 탭일 때 =================
        {
            FloorItem itemToPlace = floorList.Find(x => x.itemName == targetName);
            if (itemToPlace != null)
            {
                if (floorTilemap.HasTile(dropFloorPos))
                {
                    BoundsInt bounds = floorTilemap.cellBounds;
                    foreach (Vector3Int pos in bounds.allPositionsWithin)
                    {
                        if (floorTilemap.HasTile(pos))
                        {
                            floorTilemap.SetTile(pos, itemToPlace.tileBase);
                        }
                    }

                    // 1. UI 및 현재 상태 업데이트
                    currentFloorName = targetName;
                    RefreshUI();

                    // ✨ 2. 팀원 API 연동: 바닥 타일 변경 데이터 DB 저장
                    ServiceLocator.Get<GameData>().Interior.SetTileInterior(TilePositionType.SHOP_FLOOR, targetName);

                    Debug.Log($"<color=green>[인테리어]</color> [{targetName}] 바닥 적용 및 DB 저장 완료!");
                }
            }
        }
        else if (category == 2) // ================= 벽지 탭일 때 =================
        {
            WallpaperItem itemToPlace = wallpaperList.Find(x => x.itemName == targetName);
            if (itemToPlace != null && itemToPlace.wallTiles.Length >= 3)
            {
                if (wallTilemap.HasTile(dropWallPos))
                {
                    BoundsInt bounds = wallTilemap.cellBounds;

                    // 가장 오른쪽 X 좌표(maxX) 찾기
                    int maxX = int.MinValue;
                    foreach (Vector3Int pos in bounds.allPositionsWithin)
                    {
                        if (wallTilemap.HasTile(pos) && pos.x > maxX) maxX = pos.x;
                    }

                    // 조건에 맞춰 벽지 바르기 (문 비우기 포함)
                    foreach (Vector3Int pos in bounds.allPositionsWithin)
                    {
                        if (wallTilemap.HasTile(pos) && !wallTilemap.HasTile(pos - new Vector3Int(0, 1, 0)))
                        {
                            bool isSecondFromRight = (pos.x == maxX - 1);

                            // [1층] 문 공간 비우거나 하단 타일
                            if (isSecondFromRight) wallTilemap.SetTile(pos, null);
                            else wallTilemap.SetTile(pos, itemToPlace.wallTiles[0]);

                            // [2층] 문 윗부분(상단 타일) 또는 중간 타일
                            Vector3Int middlePos = pos + new Vector3Int(0, 1, 0);
                            if (isSecondFromRight) wallTilemap.SetTile(middlePos, itemToPlace.wallTiles[1]);
                            else wallTilemap.SetTile(middlePos, itemToPlace.wallTiles[0]);

                            // [3층] 맨 위 하단 타일
                            Vector3Int topPos = pos + new Vector3Int(0, 2, 0);
                            wallTilemap.SetTile(topPos, itemToPlace.wallTiles[2]);
                        }
                    }

                    // 1. UI 및 현재 상태 업데이트
                    currentWallpaperName = targetName;
                    RefreshUI();

                    // ✨ 2. 팀원 API 연동: 벽지 타일 변경 데이터 DB 저장
                    ServiceLocator.Get<GameData>().Interior.SetTileInterior(TilePositionType.SHOP_WALL, targetName);

                    Debug.Log($"<color=green>[인테리어]</color> [{targetName}] 벽지 적용 및 DB 저장 완료!");
                }
                else
                {
                    Debug.Log("벽지 영역이 아닌 곳에 놓아서 취소되었습니다.");
                }
            }
        }
    }


    // --- 페이지 넘기기 함수 ---
    public void OnClickNextPage()
    {
        // 현재 카테고리에 맞춰 최대 페이지 계산
        int maxCount = currentCategory == 0 ? furnitureList.Count : (currentCategory == 1 ? floorList.Count : wallpaperList.Count);
        int maxPage = Mathf.Max(0, (maxCount - 1) / itemsPerPage);

        if (currentPage < maxPage)
        {
            currentPage++;
            RefreshUI();
        }
    }

    public void OnClickPrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            RefreshUI();
        }
    }
}
