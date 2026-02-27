using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;

public class BlanketCraftController : MonoBehaviour
{
    public static BlanketCraftController Instance;

    [Header("---- UI 슬롯 연결 ----")]
    public BlanketCraftingSlot[] slots;
    public Image blanketImage; // 이불 아이콘 (있으면 연결)
    public TextMeshProUGUI blanketNameText; // 이불 이름 텍스트 (있으면 연결)
    public GameObject EmployeePanel;
    public bool isCrafting = false;
    public static event Action OnEmployeeWorkStarted;
    private EmployeeController currentEmployee; // 현재 작업 중인 직원 참조
    private BlanketItemSO RecipeData;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // 레시피 UI 아이템이 클릭되었을 때 호출되는 함수
    public void ApplyRecipeToSlots(BlanketItemSO recipeData)
    {
        ClearAllSlots();

        blanketNameText.text = recipeData.itemName;
        blanketImage.sprite = recipeData.image;

        RecipeData = recipeData;    

        if (recipeData.recipe == null) return;

        foreach (var pair in recipeData.recipe)
        {
            // 재료 이름, 필요 개수, 현재 보유 개수 가져오기
            string ingredientName = pair.itemName;
            int requiredCount = pair.count;
            Sprite icon = StorageUIController.Instance.GetIconSprite(StorageUIController.StorageType.Material, ingredientName); 
            int haveCount = InteriorManager.Instance.GetTotalItemCountInRoom(StorageUIController.StorageType.Material, ingredientName);
            FillEmptySlot(ingredientName, icon, requiredCount, haveCount);
        }
    }


    // 빈 슬롯 찾아서 데이터 넣는 헬퍼 함수
    private void FillEmptySlot(string name, Sprite icon, int requireCount, int haveCount)
    {
        foreach (var slot in slots)
        {
            if (slot.IsEmpty)
            {
                // 아까 만든 SetRecipeItem 함수 사용
                slot.SetRecipeItem(name, icon, requireCount, haveCount);
                return; // 하나 넣었으면 종료 (다음 재료는 다음 슬롯에)
            }
        }
        Debug.LogWarning("슬롯이 꽉 찼습니다!");
    }

    // 슬롯 전체 초기화
    public void ClearAllSlots()
    {
        foreach (var slot in slots)
        {
            slot.Clear();
        }
    }

    public void setCurrentEmployee(EmployeeController emp)
    {
        currentEmployee = emp;
    }

    public void OnClickCraftButton()
    {
        bool isCraftingPossible = true;

        if (isCrafting) return;

        // 1. 검사 단계: 재료가 충분한지 확인
        foreach (var slot in slots)
        {
            if (!slot.IsSufficient) 
            {
                Debug.Log($"재료 부족: {slot.ItemName}");
                isCraftingPossible = false;
                return; 
            }
        }

        // 2. 검사 단계: 직원이 선택되었는지 확인
        if (currentEmployee == null)
        {
            Debug.LogError("선택된 직원이 없습니다!");
            return;
        }

        // 3. 검사 단계: 직원이 체력이 있는지 확인 (EmployeeController의 IsWorking 검사 등)
        if (currentEmployee.currentStamina < currentEmployee.staminaCostPerWork)
        {
            Debug.LogWarning("직원의 체력이 부족합니다!");
            return;
        }

        // ★ 4. 안전장치: 이불장에 넣을 빈자리가 최소 1개라도 있는지 미리 확인!
        bool hasEmptySpace = InteriorManager.Instance.HasAnyEmptySpace(StorageUIController.StorageType.Blanket);
        if (!hasEmptySpace)
        {
            Debug.LogWarning("이불장에 빈자리가 없습니다! 이불을 먼저 팔아주세요.");
            // TODO: 유저에게 토스트 메시지 띄우기
            return;
        }

        // 모든 조건이 완벽하게 충족되었다면!
        if (isCraftingPossible)
        {
            isCrafting = true; 
            
            // =========================================================
            // ★ 핵심 변경점: 작업 시작과 동시에 인벤토리(가구)에서 재료 즉시 소모!
            // =========================================================
            foreach (var slot in slots)
            {
                if (slot.IsEmpty) continue;
                InteriorManager.Instance.ConsumeMaterialFromAnyStorage(slot.ItemName, slot.CurrentSlotQty); 
            }
            Debug.Log("제작 시작! 재료가 즉시 소모되었습니다.");

            // 직원에게 일을 시키면서 "끝나면 FinishCrafting 함수를 실행해줘!" 라고 넘깁니다.
            currentEmployee.AssignWork(() => FinishCrafting(RecipeData));
            
            StorageUIController.Instance.CloseAllPanels(); // 작업 시작하자마자 UI 닫기
        }
    }

    public void FinishCrafting(BlanketItemSO resultItem)
    {
        string finalItemName = resultItem.itemName;

        // 위에서 이미 빈자리가 있는지 검사했으므로, 여기선 무조건 성공합니다!
        bool isAdded = InteriorManager.Instance.TryAddToAnyStorage(
            StorageUIController.StorageType.Blanket, 
            finalItemName, 
            1
        );

        if (isAdded)
        {
            Debug.Log($"수납 완료! 획득한 이불: {finalItemName}");

            // ★ 재료 소모 로직은 OnClickCraftButton으로 이동했으므로 여기서는 깔끔하게 삭제!
            
            // (UI가 이미 닫혔으므로 ApplyRecipeToSlots는 굳이 안 불러도 됩니다)
        }
        else
        {
            // 이론상 발생하지 않지만, 작업 중 2초 사이에 다른 로직이 이불장을 꽉 채워버린 경우
            Debug.LogError("치명적 오류: 넣을 공간이 없어서 제작된 이불이 증발했습니다!");
        }

        isCrafting = false; // UI 잠금 해제
    }

}   
