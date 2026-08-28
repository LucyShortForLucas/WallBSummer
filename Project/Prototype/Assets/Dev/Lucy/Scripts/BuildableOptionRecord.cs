#nullable enable
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildableOptionRecord : MonoBehaviour
{
    public event Action<BuildableDatabase.Buildable>? Build;

    [SerializeReference] Image? _icon;
    [SerializeReference] TextMeshProUGUI? _title;
    [SerializeReference] TextMeshProUGUI? _descr;
    [SerializeReference] GameObject? _costRoot;
    [SerializeReference] GameObject? _costPrefab;
    [SerializeReference] Button? _button;

    private BuildableDatabase.Buildable _buildable;

    private Func<BuildableDatabase.Buildable, bool>? _canBuildPredicate;

    public void Populate(BuildableDatabase.Buildable buildable, Func<BuildableDatabase.Buildable, bool> predicate)
    {
        if (_icon != null) _icon.sprite = buildable.icon;
        if (_title != null) _title.text = buildable.name;
        if (_descr != null) _descr.text = buildable.description;

        if (_costRoot == null)
            return;

        _buildable = buildable;
        _canBuildPredicate = predicate;

        foreach (var cost in buildable.resourceCost) 
        {
            var go = Instantiate(_costPrefab, _costRoot.transform);
            if (go == null)
                continue;
            go.GetComponent<BuildableCostRecord>().Populate(cost);
        }

        Refresh();
    }

    public void Refresh()
    {
        if (_canBuildPredicate != null && _button != null)
            _button.interactable = _canBuildPredicate(_buildable);
    }
    private void OnClick() {
        Build?.Invoke(_buildable);
    }

    private void OnEnable()
    {
        if (_button)
            _button.onClick.AddListener(OnClick);

        Refresh();
    }

    private void OnDisable()
    {
        if (_button)
            _button.onClick.RemoveAllListeners();
    }
}
