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


    private Sprite failureSprite;
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

        failureSprite = ServiceLocator.Get<GameData>().Inventory.GetBlanketItemSO("엉성한 이불").image;
        var allBlankets = ServiceLocator.Get<GameData>().BlanketCraft.GetAllRecipes();


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

        foreach (var slot in slots)
        {
            if (slot.IsEmpty) continue;
            isAnySlotFilled = true;
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

        if (isValidRecipe)
        {
            // 내가 가지고 있는 레시피인지,
            if (OwnrecipeDict.ContainsKey(inputKey))
            {
                ShowNotice($"이미 보유한 레시피입니다 : {foundRecipe.itemName}");
                return;
            }
            
            StartCoroutine(CraftingSequence(foundRecipe, inputKey));
        }
        else
        {
            StartCoroutine(CraftingSequence(null, null));
        }
    }

    // [수정됨] 키 값을 받아와서 제작 완료 시 내 사전에 등록
    IEnumerator CraftingSequence(BlanketItemSO resultItem, string recipeKey)
    {
        isCrafting = true; 

        if (CraftingAnimation_Panel != null)
            CraftingAnimation_Panel.SetActive(true);


        int targetItemID = slots[0].InventoryID; 

        // 결과물이 있다면 (성공)
        if (resultItem != null)
        {
            Debug.Log($"제작 성공: {resultItem.itemName}");

            // DB에 새로운 레시피 추가
            List<string> unlockList = new List<string>() { resultItem.itemName };
            ServiceLocator.Get<GameData>().BlanketCraft.AddBlanketRecipes(unlockList);
            ServiceLocator.Get<GameData>().Inventory.AdjustBlanketCount(targetItemID, resultItem.itemName, 1);

            // OwnrecipeDict에 추가
            if (!string.IsNullOrEmpty(recipeKey) && !OwnrecipeDict.ContainsKey(recipeKey))
            {
                OwnrecipeDict.Add(recipeKey, resultItem);
            }

            Debug.Log($"[시스템] 새로운 레시피 '{resultItem.itemName}' 획득 및 등록 완료!");
        }
        else
        {
            Debug.Log("제작 실패");
            ServiceLocator.Get<GameData>().Inventory.AdjustBlanketCount(targetItemID, "엉성한 이불", 1);
        }
        yield return new WaitForSeconds(craftingTime);

        if (CraftingAnimation_Panel != null)
            CraftingAnimation_Panel.SetActive(false);


        ConsumeMaterialsInSlots();

        // 실패해도 재료 사라짐
        ClearAllSlots();
        
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

            // 슬롯에 5개가 올라와 있으면, -5를 해서 인벤토리에서 뺌
            // AdjustMaterialCount(ID, 이름, -개수)
            ServiceLocator.Get<GameData>().Inventory.AdjustMaterialCount(
                slot.InventoryID, 
                slot.ItemName, 
                -slot.CurrentSlotQty // 음수로 넣어야 빠짐
            );
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
                // (선택사항) 한 슬롯 최대 개수 제한이 있다면 여기서 체크
                slot.AddItem(inventoryID, name, icon, HaveQty); 
                return;
            }
        }
        // 2. 빈 슬롯에 추가
        foreach (var slot in slots)
        {
            if (slot.IsEmpty)
            {
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