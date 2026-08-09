using UnityEngine;

[CreateAssetMenu(fileName = "Resource", menuName = "Resources/New Resource")]
public class ResourceData : ScriptableObject
{
    public string resourceName;
    public Sprite icon;
}