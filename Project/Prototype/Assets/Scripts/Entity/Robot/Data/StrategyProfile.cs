using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StrategyProfile", menuName = "Entity/Robot/Strategy Profile")]
public class StrategyProfile : ScriptableObject
{
    [Header("Stance Distance Modifiers")]
    [SerializeField] private float offensiveDistanceMultiplier = 0.5f;
    [SerializeField] private float neutralDistanceMultiplier = 1.0f;
    [SerializeField] private float defensiveDistanceMultiplier = 2.0f;

    [Header("Offensive Playbook")]
    [SerializeField] private List<FormationData> offensiveFormations = new List<FormationData>();

    [Header("Neutral Playbook")]
    [SerializeField] private List<FormationData> neutralFormations = new List<FormationData>();

    [Header("Defensive Playbook")]
    [SerializeField] private List<FormationData> defensiveFormations = new List<FormationData>();

    // Getters and Setters
    public float OffensiveDistanceMultiplier { get => offensiveDistanceMultiplier; set => offensiveDistanceMultiplier = value; }
    public float NeutralDistanceMultiplier { get => neutralDistanceMultiplier; set => neutralDistanceMultiplier = value; }
    public float DefensiveDistanceMultiplier { get => defensiveDistanceMultiplier; set => defensiveDistanceMultiplier = value; }
    public List<FormationData> OffensiveFormations { get => offensiveFormations; set => offensiveFormations = value; }
    public List<FormationData> NeutralFormations { get => neutralFormations; set => neutralFormations = value; }
    public List<FormationData> DefensiveFormations { get => defensiveFormations; set => defensiveFormations = value; }
}