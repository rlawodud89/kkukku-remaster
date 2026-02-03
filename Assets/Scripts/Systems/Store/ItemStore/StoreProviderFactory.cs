public static class StoreProviderFactory
{
    public static IStoreItemProvider Create(StoreType type)
    {
        return type switch
        {
            StoreType.YRAN_MATERIAL => new YarnStoreProvider(),
            StoreType.COTTON_MATERIAL => new CottonStoreProvider(),
            StoreType.MOONPIECE_MATERIAL => new MoonpieceStoreProvider(),
            StoreType.WORKER => new WorkerStoreProvider(),
            StoreType.SHOP_INTERIOR => new ShopInteriorStoreProvider(),
            StoreType.ROOM_INTERIOR => new RoomInteriorStoreProvider(),
            StoreType.TILE_INTERIOR => new TileInteriorStoreProvider(),
            StoreType.GATHERING_TOOL => new GatheringToolStoreProvider(),
            StoreType.FISHING_TOOL => new FishingToolStoreProvider(),
            _ => null
        };
    }
}
