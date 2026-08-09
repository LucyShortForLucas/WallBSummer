public interface IResourceContainer
{
    void AddResource(ResourceData resource, int amount);
    bool ConsumeResource(ResourceData resource, int amount);
    bool HasEnough(ResourceData resource, int amount);
    int GetAmount(ResourceData resource);
    int GetMaxAmount(ResourceData resource);
}