using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RecipeManager : MonoBehaviour
{
    // 현재 플레이어가 얻은 레시피 ID 리스트
    private List<string> myRecipeIDs = new List<string>();
    public Transform  contentPanel;

    private static RecipeManager _instance;
    
    public static RecipeManager Instance
    {
        get
        {
            // 씬에 생성된 싱글톤이 없으면 자동 생성
            if (_instance == null)
            {
                var obj = new GameObject("RecipeManager");
                _instance = obj.AddComponent<RecipeManager>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as RecipeManager;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // 테스트
        //UnlockRecipe("빨간 체크 리본 이불");
        LoadRecipesFromDB();
    }

    // 레시피 해금 함수
    public void UnlockRecipe(string recipeID)
    {
        // 중복 해금 방지
        if (myRecipeIDs.Contains(recipeID))
        {
            Debug.Log($"이미 해금된 레시피입니다: {recipeID}");
            return;
        }

        // 리스트에 추가
        myRecipeIDs.Add(recipeID);

        // DB에 저장
        List<string> listToSave = new List<string> { recipeID };
        ServiceLocator.Get<GameData>().BlanketCraft.AddBlanketRecipes(listToSave);
        
        // 필요하다면 UI 갱신 호출 (예: RecipeUI.Instance.UpdateUI())
    }

    public void LoadRecipesFromDB()
    {
        // DB에서 기존 해금 목록 긁어오기
        List<BlanketItemSO> savedIDs = ServiceLocator.Get<GameData>().BlanketCraft.GetCurrentRecipes();
        
        if (savedIDs != null)
        {
            myRecipeIDs = savedIDs.Select(item => item.itemName).ToList();
            Debug.Log($"DB에서 {myRecipeIDs.Count}개의 레시피를 불러왔습니다.");
        }

        UpdateUI();
    }

    public void UpdateUI()
    {
        BlanketButtonUI[] blanketButtonUIScripts= contentPanel.GetComponentsInChildren<BlanketButtonUI>();

        foreach(var button in blanketButtonUIScripts)
        {
            bool isUnlocked = myRecipeIDs.Contains(button.blanketName.text);
            if (isUnlocked)
            {
                button.Setup();
            }
        }
    }
}
