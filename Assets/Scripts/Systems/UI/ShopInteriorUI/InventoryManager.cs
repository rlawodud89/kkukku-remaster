using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class InventoryManager : MonoBehaviour
{
    [Header("슬롯 8개 연결")]
    public ItemSlot[] slots = new ItemSlot[8];

    [Header("아이템 리스트 (데이터)")]
    public List<FurnitureItem> furnitureList = new List<FurnitureItem>();
    public List<FloorItem> tileList = new List<FloorItem>();
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
                if (itemIndex < tileList.Count)
                {
                    var item = tileList[itemIndex];
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

        if (category == 1) // 바닥 타일 탭일 때
        {
            FloorItem itemToPlace = tileList.Find(x => x.itemName == targetName);
            if (itemToPlace != null)
            {
                Vector3Int dropCellPos = floorTilemap.WorldToCell(worldPoint);

                if (floorTilemap.HasTile(dropCellPos))
                {
                    BoundsInt bounds = floorTilemap.cellBounds;
                    foreach (Vector3Int pos in bounds.allPositionsWithin)
                    {
                        if (floorTilemap.HasTile(pos))
                        {
                            floorTilemap.SetTile(pos, itemToPlace.tileBase);
                        }
                    }

                    // ✨ 핵심: 타일을 다 깔고 나서 현재 장착 중인 이름 업데이트 후 UI 다시 그리기!
                    currentFloorName = targetName;
                    RefreshUI();

                    Debug.Log($"[{targetName}] 바닥 전체 교체 완료!");
                }
            }
        }
        else if (category == 2) // 벽지
        {
            // 나중에 구현할 때 currentWallpaperName = targetName; 과 RefreshUI() 추가
        }
    }



    // --- 페이지 넘기기 함수 ---
    public void OnClickNextPage()
    {
        // 현재 카테고리에 맞춰 최대 페이지 계산
        int maxCount = currentCategory == 0 ? furnitureList.Count : (currentCategory == 1 ? tileList.Count : wallpaperList.Count);
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
