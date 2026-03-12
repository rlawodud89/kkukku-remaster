using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UserAggregate : IAggregate
{
    // === 런타임 데이터 ===

    private User user;
    private Dictionary<ToolType, ToolUsed> toolUsed; // Key: toolType

    // === SO 데이터 ===

    private Dictionary<string, ToolItemSO> toolSOs;

    // === 변경 사항 저장소 ===

    private HashSet<ToolType> updatedToolUsed = new();

    // === 기타 데이터 ===

    private Dictionary<int, int> interiorLevelInventoryCount = new();
    private Dictionary<int, (int width, int height, Vector3 startPos)> shopLevelSize = new();


    // === 저장 시스템 사용 메서드 ===

    public bool IsDirty { get; private set; }

    private void MarkDirty()
    {
        IsDirty = true;
        ServiceLocator.Get<DirtyDataRegistry>().RegisterDirty(this);
    }

    public void ClearDirty()
    {
        IsDirty = false;

        updatedToolUsed.Clear();
    }

    public IEnumerable<SavePayload> ToSavePayloads()
    {
        if (!IsDirty)
            yield break;

        List<SavePayload> savePayloads = new List<SavePayload>();

        // 사용자 정보 UPDATE
        yield return new SavePayload
        {
            Operation = SaveOperation.UPDATE,
            Table = "User",
            Values = new Dictionary<string, object>
            {
                { "shopName", user.shopName },
                { "level", user.level },
                { "energy", user.energy },
                { "gold", user.gold },
                { "moonrock", user.moonrock },
                { "playTime", user.playTime },
                { "endScene", user.endScene },
                { "isOpen", user.isOpen },
                { "itemShopLevel", user.itemShopLevel },
                { "interiorInventoryLevel", user.interiorInventoryLevel },
                { "shopLevel", user.shopLevel },
                { "bgmVol", user.bgmVol },
                { "sfxVol", user.sfxVol },
                { "startState", user.startState },
                { "isWatchEnding", user.isWatchEnding },
            },
            Conditions = new Dictionary<string, object>
            {
                { "id", 1 }
            }
        };

        // 변경된 장착 도구 UPDATE
        foreach (var toolType in updatedToolUsed)
        {
            yield return new SavePayload
            {
                Operation = SaveOperation.UPDATE,
                Table = "ToolUsed",
                Values = new Dictionary<string, object>
                {
                    { "toolName", toolUsed[toolType].toolName }
                },
                Conditions = new Dictionary<string, object>
                {
                    { "toolType", toolType }
                }
            };
        }

    }

    public void LoadUserAggregate(User user, IEnumerable<ToolUsed> toolUsed, Dictionary<string, ToolItemSO> toolSOs)
    {
        this.user = user;
        this.toolUsed = toolUsed.ToDictionary(tu => tu.toolType);

        this.toolSOs = toolSOs;

        interiorLevelInventoryCount.Add(1, 20);
        interiorLevelInventoryCount.Add(2, 30);
        interiorLevelInventoryCount.Add(3, 40);

        shopLevelSize.Add(1,
            (10, 6, new Vector3 { x = -5, y = 1 })
            );
        shopLevelSize.Add(2,
            (14, 7, new Vector3 { x = -7, y = 1 })
            );
        shopLevelSize.Add(3,
            (16, 8, new Vector3 { x = -8, y = 2 })
            );
    }


    // === 게임 플레이 메서드 ===


    public (string shopName, int level, float energy) GetUserData()
    {
        return (user.shopName, user.level, user.energy);
    }

    public void SetUserData(string shopName, int level, float energy)
    {
        user.shopName = shopName;
        user.level = level;
        user.energy = energy;

        MarkDirty();
    }

    public List<int> GetVolumeData()
    {
        List<int> volumeData = new List<int>();
        volumeData.Add(user.bgmVol);
        volumeData.Add(user.sfxVol);

        return volumeData;
    }

    public void SetVolumeData(int bgmVol, int sfxVol)
    {
        user.bgmVol = bgmVol;
        user.sfxVol = sfxVol;

        MarkDirty();
    }

    public int GetCurrentGold()
    {
        return user.gold;
    }

    public int GetCurrentMoonrock()
    {
        return user.moonrock;
    }

    public void ChangeGold(int amount)
    {
        user.gold += amount;
        MarkDirty();
    }

    public void ChangeMoonrock(int amount)
    {
        user.moonrock += amount;
        MarkDirty();
    }

    public int GetItemShopLevel()
    {
        return user.itemShopLevel;
    }

    public (int level, int invenCount) GetInteriorInventoryLevel()
    {
        return (user.interiorInventoryLevel, interiorLevelInventoryCount[user.interiorInventoryLevel]);
    }

    public (int level, int width, int height) GetShopLevel()
    {
        return (user.shopLevel,
            shopLevelSize[user.shopLevel].width,
            shopLevelSize[user.shopLevel].height
            );
    }


    public void ChangeItemShopLevel(int amount)
    {
        if (user.itemShopLevel + amount <= 0) return;

        user.itemShopLevel += amount;
        MarkDirty();
    }

    public void ChangeInteriorInventoryLevel(int amount)
    {
        if (user.interiorInventoryLevel + amount <= 0) return;

        user.interiorInventoryLevel += amount;
        MarkDirty();
    }

    public void ChangeShopLevel(int amount)
    {
        if (user.shopLevel + amount <= 0) return;

        user.shopLevel += amount;
        MarkDirty();
    }

    public bool GetIsOpen()
    {
        return user.isOpen;
    }

    public void SetIsOpen(bool isOpen)
    {
        user.isOpen = isOpen;
        MarkDirty();
    }

    public float GetPlayTime()
    {
        return user.playTime;
    }

    public void SetPlayTime(float playTime)
    {
        user.playTime = playTime;
        MarkDirty();
    }

    public string GetEndSceneName()
    {
        return user.endScene;
    }

    public void SetEndScene(string endSceneName)
    {
        user.endScene = endSceneName;
        MarkDirty();
    }

    public ToolItemSO GetCurrentUsedTool(ToolType toolType)
    {
        return toolSOs[toolUsed[toolType].toolName];
    }

    public void SetCurrentUsedTool(ToolType toolType, string toolName)
    {
        toolUsed[toolType].toolName = toolName;
        updatedToolUsed.Add(toolType);

        MarkDirty();
    }

    public void GetCurrentShopGridSize(out int width, out int height)
    {
        width = shopLevelSize[user.shopLevel].width;
        height = shopLevelSize[user.shopLevel].height;
    }

    public void GetCurrentShopGridSize(out int width, out int height, out Vector3 startPos)
    {
        width = shopLevelSize[user.shopLevel].width;
        height = shopLevelSize[user.shopLevel].height;
        startPos = shopLevelSize[user.shopLevel].startPos;
    }

    public StartStateType GetStartState()
    {
        return user.startState;
    }

    public void SetStartState(StartStateType startState)
    {
        user.startState = startState;
        MarkDirty();
    }

    public bool GetIsWatchEnding()
    {
        return user.isWatchEnding;
    }

    public void SetIsWatchEnding(bool isWatchEnding)
    {
        user.isWatchEnding = isWatchEnding;
        MarkDirty();
    }
}
