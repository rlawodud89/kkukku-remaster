using System.Collections.Generic;
using UnityEngine;

public class UserAggregate : IAggregate
{
    private User user;


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
    }


    public void LoadUser(User user)
    {
        this.user = user;
    }


    // === 게임 플레이 메서드 ===

    public void ChangeGold(int delta)
    {
        user.gold += delta;
        MarkDirty();
    }
}
