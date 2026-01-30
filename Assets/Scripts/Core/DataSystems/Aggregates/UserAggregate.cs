using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UserAggregate : IAggregate
{
    // === 런타임 데이터 ===

    private User user;
    private Dictionary<ToolType, ToolUsed> toolUsed;

    // === SO 데이터 ===

    private Dictionary<string, ToolItemSO> toolSOs;

    // === 변경 사항 저장소 ===

    private HashSet<ToolType> updateToolTypes = new();


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

        updateToolTypes.Clear();
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
            }
        };

        // 변경된 장착 도구 UPDATE
        foreach (var toolType in updateToolTypes)
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



}
