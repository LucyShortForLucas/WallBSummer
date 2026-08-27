#nullable enable
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildableOptionRecord : MonoBehaviour
{
    [SerializeReference] Image? _icon;
    [SerializeReference] TextMeshProUGUI? _title;
    [SerializeReference] TextMeshProUGUI? _descr;
    [SerializeReference] GameObject? _costRoot;
    [SerializeReference] GameObject? _costPrefab;

    public void Populate(BuildableDatabase.Buildable buildable)
    {
        if (_icon != null) _icon.sprite = buildable.icon;
        if (_title != null) _title.text = buildable.name;
        if (_descr != null) _descr.text = buildable.description;

        if (_costRoot == null)
            return;

        foreach (var cost in buildable.resourceCost) 
        {
            var go = Instantiate(_costPrefab, _costRoot.transform);
            if (go == null)
                continue;
            go.GetComponent<BuildableCostRecord>().Populate(cost);
        }
    }
}
