using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomStoragePanel : MonoBehaviour
{
    [Header("Prefabs & Contents")]
    public GameObject leftItemPrefab;
    public GameObject rightItemPrefab;
    public Transform leftContent;   // 왼쪽: 재고함 내의 이불들
    public Transform rightContent;  // 오른쪽: 가게의 판매대 목록

    private List<GameObject> leftSpawnedItems = new List<GameObject>();
    private List<GameObject> rightSpawnedItems = new List<GameObject>();

    private int currentOpenInventoryID = -1;

    [Header("Selection")]
    private RoomBlanketItem selectedLeft;
    private RoomBlanketItem selectedRight;

    [Header("UI Elements")]
    public Button sendButton;
    public TMP_InputField quantityText;

    private int currentTransferCount = 1;

    // 1. 패널 열기 및 데이터 로드
    public void OpenStorage(int inventoryID)
    {
        currentOpenInventoryID = inventoryID;
        gameObject.SetActive(true);
        ClearList();

        // [왼쪽 세팅] 문서 API: GetBlanketsInBox 사용
        var itemsInBox = ServiceLocator.Get<GameData>().Inventory.GetBlanketsInBox(inventoryID);
        if (itemsInBox != null)
        {
            for (int i = 0; i < itemsInBox.Count; i++)
            {
                // 변수명은 실제 BlanketInventory 클래스 멤버명에 맞게 조정하세요.
                if (itemsInBox[i].count <= 0) continue;

                GameObject go = Instantiate(leftItemPrefab, leftContent);
                leftSpawnedItems.Add(go);

                var item = go.GetComponent<RoomBlanketItem>();

                var itemSO = ServiceLocator.Get<GameData>().Inventory.GetBlanketItemSO(itemsInBox[i].itemName);
                Sprite blanketSprite = (itemSO != null) ? itemSO.image : null;

                // 가져온 이미지를 여기에 쏙 넣어줍니다.
                item.SetupItem(inventoryID, i, itemsInBox[i].itemName, itemsInBox[i].count, blanketSprite);
                item.OnItemSelected = OnLeftItemSelected;
            }
        }

        // [오른쪽 세팅] 문서 API: GetCurrentShopTables 사용

        var maxCountData = ServiceLocator.Get<GameData>().Interior.GetCurrentTableMaxCountList();
        Dictionary<int, int> tableMaxCapacity = new Dictionary<int, int>();
        if (maxCountData != null)
        {
            foreach (var data in maxCountData)
            {
                tableMaxCapacity[data.tableID] = data.maxCount;
            }
        }

        // [오른쪽 세팅]
        var tableList = ServiceLocator.Get<GameData>().ShopState.GetCurrentShopTables();
        if (tableList != null)
        {
            for (int i = 0; i < tableList.Count; i++)
            {
                GameObject go = Instantiate(rightItemPrefab, rightContent);
                rightSpawnedItems.Add(go);

                var item = go.GetComponent<RoomBlanketItem>();

                int totalAmountOnTable = 0;
                if (tableList[i].count != null)
                {
                    foreach (int amount in tableList[i].count)
                    {
                        totalAmountOnTable += amount;
                    }
                }

                // 💡 해당 테이블의 ID를 키로 써서 Max 값을 가져옵니다. (없으면 0)
                int maxCount = tableMaxCapacity.ContainsKey(tableList[i].tableID) ? tableMaxCapacity[tableList[i].tableID] : 0;

                // Max값 넘겨주기
                item.SetupTableItem(tableList[i].tableID, i, totalAmountOnTable, maxCount);
                item.OnItemSelected = OnRightItemSelected;
            }
        }
    }

    // 왼쪽 재고 선택
    void OnLeftItemSelected(RoomBlanketItem item)
    {
        if (selectedLeft != null) selectedLeft.SetHighlight(false);
        selectedLeft = item;
        selectedLeft.SetHighlight(true);
        RefreshButtonState();
        ValidateTransferCount();

        TutorialEventBus.Raise(TutorialID.ClickBlanketInBox);
    }

    // 오른쪽 판매대 선택
    void OnRightItemSelected(RoomBlanketItem item)
    {
        if (selectedRight != null) selectedRight.SetHighlight(false);
        selectedRight = item;
        selectedRight.SetHighlight(true);
        RefreshButtonState();
        ValidateTransferCount();

        TutorialEventBus.Raise(TutorialID.ClickTableInBox);
    }
    private void ValidateTransferCount()
    {
        if (selectedLeft == null) return;

        int maxAllowed = selectedLeft.currentAmount; // 기본적으로 내가 가진 개수까지만

        // 오른쪽 테이블이 선택되었다면, '빈 공간'도 고려해서 둘 중 작은 값으로 제한
        if (selectedRight != null)
        {
            int availableSpace = selectedRight.max - selectedRight.currentAmount;
            if (availableSpace < maxAllowed)
            {
                maxAllowed = availableSpace; // 빈 공간이 내가 가진 것보다 적으면, 빈 공간만큼만!
            }
        }

        // 제한된 값 안에서 현재 전송 수량 고정
        if (maxAllowed <= 0)
        {
            currentTransferCount = 0; // 꽉 찼으면 0
        }
        else
        {
            currentTransferCount = Mathf.Clamp(currentTransferCount, 1, maxAllowed);
        }

        quantityText.text = currentTransferCount.ToString();
        RefreshButtonState();
    }
    void RefreshButtonState()
    {
        if (selectedLeft == null || selectedRight == null)
        {
            if (sendButton != null) sendButton.interactable = false;
            return;
        }
        bool hasEnoughSpace = (selectedRight.currentAmount + currentTransferCount) <= selectedRight.max;
        // 양쪽 모두 선택되어야 보내기 버튼 활성화
        // (만약 테이블 쪽에 '최대 보관 가능 개수' 제한 로직이 있다면 여기서 CheckIfSpaceAvailable 활용)
        sendButton.interactable = (selectedLeft != null && selectedRight != null);
    }

    public void ChangeQuantity(int amount)
    {
        if (selectedLeft == null) return;
        currentTransferCount += amount;
        ValidateTransferCount();
    }

    // 💡 2. 가장 중요한 데이터 이동 전송 로직
    public void ExecuteTransfer()
    {
        if (quantityText != null && int.TryParse(quantityText.text, out int manualResult))
        {
            currentTransferCount = manualResult;
            ValidateTransferCount(); // 수량 제한(Max 등) 로직을 한 번 더 타서 안전하게 고정
        }

        if (selectedLeft == null || selectedRight == null || currentTransferCount <= 0) return;

        // [DB 업데이트] 문서에 적힌 API 호출
        // 1. 재고함에서 빼기 (-)
        ServiceLocator.Get<GameData>().Inventory.AdjustBlanketCount(selectedLeft.parentID, selectedLeft.itemName, -currentTransferCount);

        // 2. 가게 판매대에 넣기 (+)
        ServiceLocator.Get<GameData>().ShopState.AdjustShopTableBlanketCount(selectedRight.parentID, selectedLeft.itemName, currentTransferCount);

        // [UI 갱신]
        selectedLeft.currentAmount -= currentTransferCount;
        selectedLeft.RefreshUI(true);
        selectedRight.currentAmount += currentTransferCount;
        selectedRight.RefreshUI(false);

        // 왼쪽 수량이 0이 되면 항목 삭제
        if (selectedLeft.currentAmount <= 0)
        {
            leftSpawnedItems.Remove(selectedLeft.gameObject);
            Destroy(selectedLeft.gameObject);
            selectedLeft = null;
        }

        Debug.Log($"[{selectedRight.itemName}]로 {selectedLeft?.itemName} {currentTransferCount}개 전송 완료");

        // 상태 초기화
        currentTransferCount = 1;
        quantityText.text = "1";
        ValidateTransferCount();
        RefreshButtonState();

        TutorialEventBus.Raise(TutorialID.ClickPassInBox);
    }

    // 패널 닫기 및 초기화
    public void ClosePanel()
    {
        ClearList();
        UIEventManager.ShowMainUI();
        gameObject.SetActive(false);

        TutorialEventBus.Raise(TutorialID.ExitBlanketBox);
    }


    private void ClearList()
    {
        if (selectedLeft != null) selectedLeft.SetHighlight(false);
        if (selectedRight != null) selectedRight.SetHighlight(false);

        foreach (var item in leftSpawnedItems) Destroy(item);
        leftSpawnedItems.Clear();

        foreach (var item in rightSpawnedItems) Destroy(item);
        rightSpawnedItems.Clear();

        selectedLeft = null;
        selectedRight = null;
        currentTransferCount = 1;
        quantityText.text = "1";

        // 💡 추가: 리스트를 비우고 선택(selected)이 null이 되었으니, 버튼도 비활성화시킵니다.
        RefreshButtonState();
    }

    public void OnInputQuantityChanged(string input)
    {
        if (selectedLeft == null) return;

        if (int.TryParse(input, out int result))
        {
            currentTransferCount = Mathf.Clamp(result, 1, selectedLeft.currentAmount);
        }
        else
        {
            currentTransferCount = 1;
        }
        ValidateTransferCount();
    }
}