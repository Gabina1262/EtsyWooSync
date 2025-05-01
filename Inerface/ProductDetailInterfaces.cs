namespace EtsyWooSync.Inerface
{

    public interface IHasCategories
    {
        List<string> Categories { get; }
    }

    public interface IHasTags
    {
        List<string> Tags { get; }
    }

    public interface IHasAttributes
    {
        Dictionary<string, List<string>> Attributes { get; }
    }
}
