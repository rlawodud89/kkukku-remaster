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
    private EmployeeController currentEmployee; // 현재 작업 중인 직원 참조
    private BlanketItemSO RecipeData;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (blanketImage != null) blanketImage.enabled = false;
    }
    // 레시피 UI 아이템이 클릭되었을 때 호출되는 함수
    public void ApplyRecipeToSlots(BlanketItemSO recipeData)
    {
        ClearAllSlots();

        blanketNameText.text = recipeData.itemName;
        blanketImage.sprite = recipeData.image;

        if (blanketImage != null) blanketImage.enabled = true;

        RecipeData = recipeData;

        if (recipeData.recipe == null) return;

        foreach (var pair in recipeData.recipe)
        {
            // 재료 이름, 필요 개수, 현재 보유 개수 가져오기
            string ingredientName = pair.itemName;
            int requiredCount = pair.count;
            Sprite icon = StorageUIController.Instance.GetIconSprite(StorageUIController.StorageType.Material, ingredientName);
            int haveCount = RoomInteriorManager.Instance.GetTotalItemCountInRoom(StorageUIController.StorageType.Material, ingredientName);
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

                slot.gameObject.SetActive(true);

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

            slot.gameObject.SetActive(false);
        }

        if (blanketImage != null) blanketImage.enabled = false;
        if (blanketNameText != null) blanketNameText.text = "";
    }

    public void setCurrentEmployee(EmployeeController emp)
    {
        currentEmployee = emp;
    }

    public void OnClickCraftButton()
    {

        // 1. 재료 검사 (외부 환경)
        foreach (var slot in slots)
        {
            if (!slot.IsSufficient)
            {
                Debug.Log($"재료 부족: {slot.ItemName}");
                return;
            }
        }

        // 2. 이불장 빈자리 검사 (외부 환경)
        bool hasEmptySpace = RoomInteriorManager.Instance.HasAnyEmptySpace(StorageUIController.StorageType.Blanket);
        if (!hasEmptySpace)
        {
            Debug.LogWarning("이불장에 빈자리가 없습니다! 이불을 먼저 팔아주세요.");
            // TODO: NoticeText 등 토스트 메시지 띄우기
            return;
        }

        // 3. 직원 할당 여부 확인
        if (currentEmployee == null)
        {
            Debug.LogError("선택된 직원이 없습니다!");
            return;
        }

        BlanketItemSO targetRecipe = RecipeData;
        
        bool isAccepted = currentEmployee.StartCrafting(RecipeData.itemName, () => FinishCrafting(targetRecipe));

        // 5. 직원이 일을 시작하겠다고 수락했다면?
        if (isAccepted)
        {
            // 그때 재료를 확실하게 소모합니다.
            foreach (var slot in slots)
            {
                if (slot.IsEmpty) continue;
                RoomInteriorManager.Instance.ConsumeMaterialFromAnyStorage(slot.ItemName, slot.CurrentSlotQty);
            }

            Debug.Log("제작 시작! 재료가 즉시 소모되었습니다.");
            StorageUIController.Instance.CloseAllPanels(); // UI 닫기
            UIEventManager.ShowMainUI();
        }
        else
        {
            // 체력이 없거나 다른 일 중이라서 직원이 거절함
            Debug.LogWarning("직원이 체력이 부족하거나 이미 일하는 중입니다!");
        }

        TutorialEventBus.Raise(TutorialID.MakeBlanket);
    }

    public void FinishCrafting(BlanketItemSO resultItem)
    {
        string finalItemName = resultItem.itemName;

        // 위에서 이미 빈자리가 있는지 검사했으므로, 여기선 무조건 성공합니다!
        bool isAdded = RoomInteriorManager.Instance.TryAddToAnyStorage(
            StorageUIController.StorageType.Blanket,
            finalItemName,
            1
        );

        if (isAdded)
        {
            Debug.Log($"수납 완료! 획득한 이불: {finalItemName}");
        }
        else
        {
            // 이론상 발생하지 않지만, 작업 중 2초 사이에 다른 로직이 이불장을 꽉 채워버린 경우
            Debug.LogError("치명적 오류: 넣을 공간이 없어서 제작된 이불이 증발했습니다!");
        }


        // 퀘스트 
        QuestManager.Instance.UpdateQuestProgressByID(1);
    }

}
