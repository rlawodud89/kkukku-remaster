using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;  
using TMPro;
using System.Linq; // 정렬(OrderBy)을 위해 필수
using System.Text;
using System.Collections;
using Unity.VisualScripting.FullSerializer;
using System.Diagnostics.Contracts;
using System.Data.Common;
using Unity.VisualScripting;

public class RecipeCraftController : MonoBehaviour
{
    public static RecipeCraftController Instance;

    [Header("---- UI 슬롯 연결 ----")]
    public RecipeCraftingSlot[] slots;
    public GameObject CraftPopupPanel;
    public GameObject resultPopupPanel;
    public Image resultItemImage;
    public TextMeshProUGUI resultItemNameText;
    public TextMeshProUGUI NoticeText;
    public Sprite successSprite;


    [Header("---- 애니메이션 ----")]
    public GameObject CraftingAnimation_Panel;
    public float craftingTime = 2.0f;

    [Header("기록 이력 UI")]
    public GameObject historyPrefab; 
    public Transform historyContentParent;

    private Sprite[] historySprites;
    private int[] historyNums;

    public Sprite failureSprite;
    private string failureMessage = "레시피 제작에 실패했습니다... 엉성한 이불을 획득하였습니다.";

    // 전체 레시피를 빠르게 찾기 위한 사전 (Key: 조합문자열, Value: 결과물)
    private Dictionary<string, BlanketItemSO> recipeDict = new Dictionary<string, BlanketItemSO>();
    private Dictionary<string, BlanketItemSO> OwnrecipeDict = new Dictionary<string, BlanketItemSO>();
    private Coroutine currentNoticeRoutine;
    public bool isCrafting = false;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        InitializeRecipeTable();
    }

    // 1. 게임 시작 시 DB 데이터를 Dictionary로 변환 (Key 생성)
    private void InitializeRecipeTable()
    {

        var allBlankets = ServiceLocator.Get<GameData>().BlanketCraft.GetAllRecipes();


        foreach (var slot in slots)
        {
            slot.iconImage.gameObject.SetActive(false);
        }

        foreach (var blanket in allBlankets)
        {
            if (blanket.recipe == null || blanket.recipe.Count == 0)
            {
                continue; // 엉성한 이불용 if문
            }

            string key = GenerateKeyFromList(blanket.recipe);

            if (!recipeDict.ContainsKey(key))
            {
                recipeDict.Add(key, blanket);
            }
            else
            {
                Debug.LogWarning($"[시스템] 중복된 레시피 키 발견: {blanket.itemName} (키: {key})");
            }
        }
        Debug.Log($"[시스템] 레시피 {recipeDict.Count}개 해싱 완료!");


        // 자신의 레시피만 별도 관리
        var ownBlankets = ServiceLocator.Get<GameData>().BlanketCraft.GetCurrentRecipes();

        foreach (var blanket in ownBlankets)
        {
            if (blanket.recipe == null || blanket.recipe.Count == 0)
            {
                continue; // 엉성한 이불용 if문
            }

            string key = GenerateKeyFromList(blanket.recipe);

            if (!OwnrecipeDict.ContainsKey(key))
            {
                OwnrecipeDict.Add(key, blanket);
            }
            else
            {
                Debug.LogWarning($"[시스템] 중복된 레시피 키 발견: {blanket.itemName} (키: {key})");
            }
        }

    }



   public void OnClickCraftYesBtn()
    {
        if (isCrafting) return;

        // 1. 재료 수집
        Dictionary<string, int> currentInput = new Dictionary<string, int>();
        bool isAnySlotFilled = false;

        int index = 0;


        historySprites = new Sprite[slots.Length];
        historyNums = new int[slots.Length];
        foreach (var slot in slots)
        {
            if (slot.IsEmpty) continue;
            
            isAnySlotFilled = true;

            historySprites[index] = slot.iconImage.sprite; // 기록 이력용 이미지 저장
            historyNums[index] = slot.CurrentSlotQty;
            index++;

            if (currentInput.ContainsKey(slot.ItemName))
                currentInput[slot.ItemName] += slot.CurrentSlotQty;
            else
                currentInput.Add(slot.ItemName, slot.CurrentSlotQty);
        }

        if (!isAnySlotFilled)
        {
            ShowNotice("재료를 올려주세요!");
            return;
        }

        // 2. 키 생성
        string inputKey = GenerateKeyFromInput(currentInput);
        BlanketItemSO foundRecipe = null;

        // 유효한 레시피인지
        bool isValidRecipe = recipeDict.TryGetValue(inputKey, out foundRecipe);

        if (isValidRecipe && OwnrecipeDict.ContainsKey(inputKey))
        {
            ShowNotice($"이미 보유한 레시피입니다 : {foundRecipe.itemName}");
            return;
        }

        string resultName = isValidRecipe ? foundRecipe.itemName : "엉성한 이불";

        if (historyPrefab != null && historyContentParent != null)
        {
            GameObject historyObj = Instantiate(historyPrefab, historyContentParent);
            RecipeHistorySlotUI historyUI = historyObj.GetComponent<RecipeHistorySlotUI>();
            
            if (historyUI != null)
            {
                // 결정된 resultName을 넘겨줍니다.
                historyUI.SetHistoryData(historySprites, historyNums, resultName); 
            }
        }

        // 💡 4. [제작 시작] 
        if (isValidRecipe)
        {
            StartCoroutine(CraftingSequence(foundRecipe, inputKey));
        }
        else
        {
            StartCoroutine(CraftingSequence(null, null));
        }
    }

    private WR_StorageController GetStorageController(int targetID)
    {
        // (만약 InteriorManager에서 배치된 가구 리스트를 관리 중이라면 그걸 활용하는 게 베스트입니다!)
        // 지금은 가장 직관적인 FindObjectsOfType 방식으로 보여드릴게요.
        
        WR_StorageController[] allBoxes = FindObjectsOfType<WR_StorageController>();
        foreach (var box in allBoxes)
        {
            if (box.myStorageID == targetID)
            {
                return box; // 찾았다!
            }
        }
        
        Debug.LogError($"[Crafting] {targetID}번 상자를 씬에서 찾을 수 없습니다!");
        return null;
    }


    // [최종 수정본] 키 값을 받아와서 제작 완료 시 내 사전에 등록
    IEnumerator CraftingSequence(BlanketItemSO resultItem, string recipeKey)
    {
        isCrafting = true; 

        // 1. 애니메이션 시작
        if (CraftingAnimation_Panel != null)
            CraftingAnimation_Panel.SetActive(true);

        // 2. 애니메이션 연출 대기 (기다리는 동안에는 데이터를 건드리지 않음!)
        yield return new WaitForSeconds(craftingTime);

        if (CraftingAnimation_Panel != null)
            CraftingAnimation_Panel.SetActive(false);

        // =========================================================
        // 3. 실제 데이터 처리 (재료 소모 & 결과물 지급)
        // =========================================================
        
        // 일단 재료부터 확실하게 뺍니다.
        ConsumeMaterialsInSlots();
        ClearAllSlots();

        // 결과물 이름 결정 (성공하면 원래 이름, 실패하면 엉성한 이불)
        string finalItemName = (resultItem != null) ? resultItem.itemName : "엉성한 이불";
        
        // 이불장에 수납 시도
        bool isAdded = RoomInteriorManager.Instance.TryAddToAnyStorage(
            StorageUIController.StorageType.Blanket, 
            finalItemName, 
            1
        );

        // 수납 성공 여부에 따른 처리
        if (isAdded)
        {
            Debug.Log($"수납 완료! 획득한 이불: {finalItemName}");

            // 진짜 레시피(resultItem)를 성공적으로 만들었을 때만 DB에 레시피 등록
            if (resultItem != null) 
            {
                List<string> unlockList = new List<string>() { finalItemName };
                
                RecipeManager.Instance.UnlockRecipe(finalItemName);

                // 내 사전(OwnrecipeDict)에 추가
                if (!string.IsNullOrEmpty(recipeKey) && !OwnrecipeDict.ContainsKey(recipeKey))
                {
                    OwnrecipeDict.Add(recipeKey, resultItem);
                }
                Debug.Log($"[시스템] 새로운 레시피 '{finalItemName}' 획득 및 등록 완료!");
            }
        }
        else
        {
            ShowNotice("모든 이불장이 꽉 찼습니다! 이불을 먼저 팔아주세요.");
            
            // 주의: 이불장이 꽉 차서 아이템을 못 얻었으므로, 
            // 기획에 따라 여기서 레시피 등록도 안 되게 막는 것이 자연스러울 수 있습니다!
        }

        // =========================================================
        // 4. UI 갱신 및 팝업 띄우기
        // =========================================================
        
        if (StorageUIController.Instance != null && StorageUIController.Instance.IsPopupOpen)
        {
            StorageUIController.Instance.RefreshCurrentPopup();
        }

        ShowResultPopup(resultItem);

        isCrafting = false;
    }

    private string GenerateKeyFromList(List<RecipePair> recipe)
    {
        // 이름순 정렬
        var sortedList = recipe.OrderBy(x => x.itemName).ToList();

        StringBuilder sb = new StringBuilder();
        foreach (var pair in sortedList)
        {
            sb.Append(pair.itemName);
            sb.Append("_");
            sb.Append(pair.count);
            sb.Append("_");
        }
        return sb.ToString();
    }

    private string GenerateKeyFromInput(Dictionary<string, int> input)
    {
        // Key(이름)순 정렬
        var sortedKeys = input.Keys.OrderBy(k => k).ToList();

        StringBuilder sb = new StringBuilder();
        foreach (var key in sortedKeys)
        {
            sb.Append(key);       // 이름
            sb.Append("_");
            sb.Append(input[key]); // 개수
            sb.Append("_");
        }
        return sb.ToString();
    }

   private void ConsumeMaterialsInSlots()
    {
        foreach (var slot in slots)
        {
            // 빈 슬롯은 패스
            if (slot.IsEmpty) continue;

            // ★ 단일 ID 대신, 슬롯이 들고 있는 '장부(sourceBoxes)'를 쫙 펼쳐서 각각 차감합니다.
            foreach (var kvp in slot.sourceBoxes)
            {
                int boxID = kvp.Key;          // 재료를 꺼내왔던 상자 ID
                int deductAmount = kvp.Value; // 해당 상자에서 꺼내온 개수

                // 1. 인벤토리(DB)에서 정확히 그 상자의 재료를 차감
                ServiceLocator.Get<GameData>().Inventory.AdjustMaterialCount(
                    boxID, 
                    slot.ItemName, 
                    -deductAmount // 음수로 넣어야 빠짐
                );

                // 2. 맵에 배치된 가구의 현재 용량(totalItemCount) 상태도 최신화!
                if (RoomInteriorManager.Instance != null)
                {
                    var targetBox = RoomInteriorManager.Instance.GetStorageBoxByID(boxID);
                    if (targetBox != null)
                    {
                        targetBox.UpdateTotalItemCount();
                    }
                }
            }
        }
    }
    // =========================================================

    // display 용 / 재료 눌렀을 때 슬롯에 들어가게
    public void AddIngredient(int inventoryID, string name, Sprite icon, int HaveQty)
    {
        

        // 1. 이미 같은 재료가 있는 슬롯에 추가
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty && slot.ItemName == name)
            {
                slot.iconImage.gameObject.SetActive(true);
                slot.AddItem(inventoryID, name, icon, HaveQty); 
                return;
            }
        }
        // 2. 빈 슬롯에 추가
        foreach (var slot in slots)
        {
            if (slot.IsEmpty)
            {
                slot.iconImage.gameObject.SetActive(true);
                slot.AddItem(inventoryID, name, icon, HaveQty);
                return;
            }
        }
        ShowNotice("슬롯이 가득 찼습니다!");
    }

    // '제작하기 버튼' 눌렀을 때
    public void OnClickCraftButton()
    {
        CraftPopupPanel.SetActive(true);
    }


    // ===============UI==================
    public void ShowNotice(string message)
    {
        if (NoticeText == null) return;

        if (currentNoticeRoutine != null)
        {
            StopCoroutine(currentNoticeRoutine);
        }

        NoticeText.text = message;

        currentNoticeRoutine = StartCoroutine(DisableNoticeRoutine());
    }
    IEnumerator DisableNoticeRoutine()
    {
        yield return new WaitForSeconds(2.0f); // 2초 대기

        NoticeText.text = ""; // 텍스트 비우기
        
        currentNoticeRoutine = null;
    }


    private void ShowResultPopup(BlanketItemSO item)
    {
        if (resultPopupPanel == null) return;

        if (item != null)
        {
            // === 성공 케이스 ===
            if (resultItemImage != null) resultItemImage.sprite = successSprite;
            if (resultItemNameText != null) resultItemNameText.text = item.itemName+ " 레시피 제작 성공 !";

            // (선택) 성공 효과음 재생
        }
        else
        {
            // === 실패 케이스 (item == null) ===
            if (resultItemImage != null) 
                resultItemImage.sprite = failureSprite;
            
            if (resultItemNameText != null) 
                resultItemNameText.text = failureMessage;
            
            // (선택) 실패 효과음 재생
        }
        
        CraftPopupPanel.SetActive(false);
        resultPopupPanel.SetActive(true);
    }


    private void ClearAllSlots()
    {
        foreach(var slot in slots)
        {
            slot.Clear(); // 슬롯 스크립트에 초기화 함수가 있다고 가정
        }
    }
}