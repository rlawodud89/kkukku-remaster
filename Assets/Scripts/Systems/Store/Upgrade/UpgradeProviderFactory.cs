public static class UpgradeProviderFactory
{
    public static IUpgradeProvider Create(UpgradeType type)
    {
        return type switch
        {
            UpgradeType.ITEM_SHOP => new ItemShopProvider(),
            UpgradeType.INTERIOR_INVENTORY => new InteriorInventoryProvider(),
            UpgradeType.SHOP_EXPANSION => new ShopExpansionProvider(),
            _ => null
        };
    }
}
