#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildableDatabase", menuName = "Database/BuildableDatabase")]
public class BuildableDatabase : ScriptableObject, IEnumerable<BuildableDatabase.Buildable>
{
    // TEMPORARY helper enum for resources <--- THIS SHOULD BE MADE INTO A CENTRAL RESOURCE ENUM LATER
    public enum Resource
    {
        ScrapMetal = 1,
        MechanicalScrap,
        ElectronicScrap,
        Seeds,
        Food
    }

    // ---- Database entry
    [Serializable] public struct Buildable
    {
        [Serializable] public struct ResourceCost
        {
            public Resource resource;
            public int cost;
        }

        public string name;
        [TextArea] public string description;
        public Sprite icon;

        [Header("Object refs")]
        public GameObject building;
        public Mesh previewMesh;

        [Header("Placement config")]
        public Vector2Int tileFootPrint;
        public Vector3 placementOffset;
        [Header("Prerequisites")]
        public bool requiresFertileSoil;
        public List<ResourceCost> resourceCost;
    }

    // ---- Database API
    [SerializeField] private List<Buildable> _buildables = new();
    [SerializeReference] private List<BuildableDatabase> _children = new();
    public Buildable this[int i] => _buildables[i];
    public int Count => _buildables.Count;

    public static BuildableDatabase Concatenate(BuildableDatabase first, BuildableDatabase second)
    {
        var result = Instantiate(first);
        result._buildables.Concat(second._buildables);
        return result;
    }

    // ---- Enumerator interface implementation
    public IEnumerator<Buildable> GetEnumerator()
    {
        return _buildables.GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
