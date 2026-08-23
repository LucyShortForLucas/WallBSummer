using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildableDatabase", menuName = "Database/BuildableDatabase")]
public class BuildableDatabase : ScriptableObject
{
    [Serializable] public struct Buildable
    {
        public string _name;
        public GameObject _building;
        public Vector2Int _tileFootPrint;
        public Vector3 _placementOffset;
    }

    [SerializeField] private List<Buildable> _buildables = new();
}
