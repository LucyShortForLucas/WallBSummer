using UnityEngine;

public class ResourceState
{
    public int current;
    public int max;
    public bool allowStoring;
    public bool allowTaking;

    public ResourceState(int maxCapacity = 100, bool canStore = true, bool canTake = true)
    {
        current = 0;
        max = maxCapacity;
        allowStoring = canStore;
        allowTaking = canTake;
    }
}