public static class ToolPerformerFactory
{
    public static IToolPerformer Create(ToolType type)
    {
        return type switch
        {
            ToolType.GATHERING => new GatheringToolPerformer(),
            ToolType.FISHING => new FishingToolPerformer(),
            _ => null
        };
    }
}