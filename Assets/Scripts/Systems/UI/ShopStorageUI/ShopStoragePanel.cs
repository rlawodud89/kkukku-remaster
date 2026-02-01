using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopStoragePanel : MonoBehaviour
{

    // Start is called before the first frame update
    public GameObject leftItemPrefab;
    public GameObject rightItemPrefab;
    public Transform leftContent;   // 왼쪽 스크롤뷰의 Content
    public Transform rightContent;  // 오른쪽 스크롤뷰의 Content

    // 기존 리스트를 추적하기 위한 리스트 (나중에 지우기 위함)
    private List<GameObject> leftSpawnedItems = new List<GameObject>();
    private List<GameObject> rightSpawnedItems = new List<GameObject>();

    // 1. 클릭 시 호출되어 데이터를 채우는 함수
    public void OpenStorage(int id)
    {
        gameObject.SetActive(true); // 패널 켜기

        // 이전 데이터 삭제
        ClearList();

        if(ShopStorageDataManager.Instance.GetTableClass(id, out TableClass blanketList))
        {
            for(int i = 0; i < blanketList.itemName.Count; i++)
            {
                GameObject go = Instantiate(leftItemPrefab, leftContent);
                leftSpawnedItems.Add(go);

                var item = go.GetComponent<BlanketItem>();

                item.SetupItem(id, i, blanketList.itemName[i], blanketList.count[i], blanketList.itemImage[i]);
                item.OnItemSelected = OnLeftItemSelected;
            }
        }


        // 오른쪽(재고함) 리스트도 여기서 채움. 이건 다른 이불장과 동일한 정보를 가져옴. 씬 로드 시 불러온 데이터 사용
        // 먼저 데이터 가져오기
        int j = 0;
        foreach (StorageClass s in ShopStorageDataManager.Instance.storageClasses)
        {
            GameObject go = Instantiate(rightItemPrefab, leftContent);
            rightSpawnedItems.Add(go);

            var item = go.GetComponent<BlanketItem>();

            item.SetupBlanketItem(id, j, s.count, s.max);
            item.OnItemSelected = OnRightItemSelected;
            j++;
        }

    }

    // 리스트 초기화 함수
    private void ClearList()
    {
        foreach (var item in leftSpawnedItems)
        {
            Destroy(item);
        }
        leftSpawnedItems.Clear();

        foreach (var item in rightSpawnedItems)
        {
            Destroy(item);
        }
        rightSpawnedItems.Clear();

        // 선택 정보도 초기화
        selectedLeft = null;
        selectedRight = null;
        RefreshButtonState();
    }

    [Header("Selection")]
    private BlanketItem selectedLeft;
    private BlanketItem selectedRight;

    [Header("UI Elements")]
    public Button sendButton;
    public TMP_InputField quantityText;
    

    int currentTransferCount = 1;

    // 왼쪽 아이템 선택 시 호출
    void OnLeftItemSelected(BlanketItem item)
    {
        if (selectedLeft != null) selectedLeft.SetHighlight(false);
        selectedLeft = item;
        selectedLeft.SetHighlight(true);
        RefreshButtonState();
    }

    // 오른쪽 재고함 선택 시 호출
    void OnRightItemSelected(BlanketItem item)
    {
        if (selectedRight != null) selectedRight.SetHighlight(false);
        selectedRight = item;
        selectedRight.SetHighlight(true);
        RefreshButtonState();
    }

    // 버튼 활성화 여부 결정
    void RefreshButtonState()
    {
        bool isBothSelected = (selectedLeft != null && selectedRight != null);

        // 예외사항: 재고함이 꽉 찼는지 체크 (예: 현재 사용중인 슬롯 수 확인)
        bool isRightNotFull = CheckIfSpaceAvailable(selectedRight);

        sendButton.interactable = isBothSelected && isRightNotFull;
    }

    bool CheckIfSpaceAvailable(BlanketItem targetSlot)
    {
        if (targetSlot == null) return false;
        // 재고함 아이템의 '사용중 슬롯'이 '최대 슬롯'보다 작은지 확인하는 로직
        return targetSlot.currentAmount < targetSlot.max;
    }

    // +, - 버튼에 연결할 함수들
    public void ChangeQuantity(int amount)
    {
        if (selectedLeft == null) return;

        // 선택된 왼쪽 아이템의 실제 수량 범위를 벗어나지 않게 클램프
        currentTransferCount = Mathf.Clamp(currentTransferCount + amount, 1, selectedLeft.currentAmount);
        quantityText.text = currentTransferCount.ToString();
    }

    // 보내기 버튼 클릭 시
    public void ExecuteTransfer()
    {
        if (selectedLeft == null || selectedRight == null) return;

        // 1. 데이터 업데이트
        selectedLeft.currentAmount -= currentTransferCount;
        selectedRight.currentAmount += currentTransferCount;

        // 데이터 매니저 데이터 업데이트
        ShopStorageDataManager.Instance.UpdateTableData(selectedLeft.parentID, selectedLeft.dataIndex, -currentTransferCount);
        ShopStorageDataManager.Instance.UpdateStorageData(selectedRight.parentID, currentTransferCount);

        // 2. UI 갱신 (아이템 프리팹 내부의 텍스트를 새로고침하는 함수가 아이템 스크립트에 있어야 함)
        selectedLeft.RefreshUI(true);
        selectedRight.RefreshUI(false);

        // 3. 만약 수량이 0이 되면 아이템 파괴
        if (selectedLeft.currentAmount <= 0)
        {
            leftSpawnedItems.Remove(selectedLeft.gameObject);
            Destroy(selectedLeft.gameObject);
            selectedLeft = null;
        }

        // 4. 전송 후 수량 초기화 및 버튼 상태 업데이트
        currentTransferCount = 1;
        quantityText.text = "1";

        Debug.Log($"{currentTransferCount}개의 이불을 전송했습니다.");
        RefreshButtonState();
    }

    //X버튼 연결
    public void ClosePanel()
    {
        // 1. 선택된 정보 초기화 (다음에 열 때 깨끗하게)
        if (selectedLeft != null) selectedLeft.SetHighlight(false);
        if (selectedRight != null) selectedRight.SetHighlight(false);

        selectedLeft = null;
        selectedRight = null;
        currentTransferCount = 1;
        quantityText.text = "1";

        // 2. 생성된 아이템들 삭제 (메모리 관리)
        ClearList();

        // 3. 패널 비활성화
        gameObject.SetActive(false);
    }

    // 인스펙터에서 InputField의 On End Edit 또는 On Value Changed에 연결할 함수
    public void OnInputQuantityChanged(string input)
    {
        if (selectedLeft == null) return;

        // 1. 입력된 문자열을 숫자로 변환 시도
        if (int.TryParse(input, out int result))
        {
            // 2. 보유량 범위를 벗어나지 않게 다시 조정
            currentTransferCount = Mathf.Clamp(result, 1, selectedLeft.currentAmount);
        }
        else
        {
            // 3. 숫자가 아니면 1로 초기화
            currentTransferCount = 1;
        }

        // 4. 입력 칸을 최종 결정된 숫자로 강제 업데이트
        quantityText.text = currentTransferCount.ToString();
    }
}
