using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Header("슬롯 8개 연결")]
    public ItemSlot[] slots = new ItemSlot[8];

    [Header("아이템 리스트 (데이터)")]
    public List<FurnitureItem> furnitureList = new List<FurnitureItem>();
    public List<FloorItem> tileList = new List<FloorItem>();
    public List<WallpaperItem> wallpaperList = new List<WallpaperItem>();


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
                    // 가구는 quantity를 넣고, showCount를 true로 전달
                    slots[i].UpdateSlot(item.itemImage, item.itemName, item.quantity, true);
                }
                else { slots[i].UpdateSlot(null, "", 0, false); }
            }
            else if (currentCategory == 1) // 타일 탭
            {
                if (itemIndex < tileList.Count)
                {
                    var item = tileList[itemIndex];
                    // 타일은 showCount를 false로 전달하여 숫자 숨김
                    slots[i].UpdateSlot(item.itemImage, item.itemName, 0, false);
                }
                else { slots[i].UpdateSlot(null, "", 0, false); }
            }
            else if (currentCategory == 2) // 벽지 탭
            {
                if (itemIndex < wallpaperList.Count)
                {
                    var item = wallpaperList[itemIndex];
                    // 벽지도 showCount를 false로 전달
                    slots[i].UpdateSlot(item.itemImage, item.itemName, 0, false);
                }
                else { slots[i].UpdateSlot(null, "", 0, false); }
            }
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
