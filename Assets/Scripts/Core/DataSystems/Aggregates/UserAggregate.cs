using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UserAggregate : IAggregate
{
    private User user;
    private Dictionary<ToolType, ToolUsed> toolUsed;

    private HashSet<ToolType> updateToolTypes = new();

    public bool IsDirty { get; private set; }

    private void MarkDirty()
    {
        IsDirty = true;
        ServiceLocator.Get<DirtyDataRegistry>().RegisterDirty(this);
    }

    public void ClearDirty() => IsDirty = false;

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
                { "bgSound", user.bgSound },
                { "effectSound", user.effectSound },
            },
            Conditions = new Dictionary<string, object>
            {
                { "shopName", user.shopName }
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

        updateToolTypes.Clear();
    }

    public void LoadUserAggregate(User user, IEnumerable<ToolUsed> toolUsed)
    {
        this.user = user;
        this.toolUsed = toolUsed.ToDictionary(tu => tu.toolType);
    }


    // === 게임 플레이 메서드 ===

    public void ChangeGold(int amount)
    {
        user.gold += amount;
        MarkDirty();
    }
}
