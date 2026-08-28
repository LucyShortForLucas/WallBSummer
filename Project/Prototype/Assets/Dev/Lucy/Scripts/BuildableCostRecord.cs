#nullable enable
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BuildableCostRecord : MonoBehaviour
{
    [SerializeReference] Image? _image;
    [SerializeReference] TextMeshProUGUI? _tmPro;
    [SerializeReference] GlobalResourceDatabase _resourceDatabase;

    public void Populate(BuildableDatabase.Buildable.ResourceCost cost)
    {
        if (_image != null && _resourceDatabase != null) 
            _image.sprite = _resourceDatabase.GetResource((int)cost.resource).icon;
        if (_tmPro != null) 
            _tmPro.text = cost.cost.ToString();
    }
}
