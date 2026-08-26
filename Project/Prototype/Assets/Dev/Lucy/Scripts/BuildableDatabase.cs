using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildableDatabase", menuName = "Database/BuildableDatabase")]
public class BuildableDatabase : ScriptableObject
{
    [Serializable] public struct Buildable
    {
        public string name;
        public GameObject building;
        public Mesh previewMesh;
        public Vector2Int tileFootPrint;
        public Vector3 placementOffset;

    }

    [SerializeField] private List<Buildable> _buildables = new();

    public Buildable this[int i] => _buildables[i];
    public int Count => _buildables.Count;
}
