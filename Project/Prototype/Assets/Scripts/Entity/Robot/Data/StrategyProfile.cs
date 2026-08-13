using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StrategyProfile", menuName = "Entity/Robot/Strategy Profile")]
public class StrategyProfile : ScriptableObject
{
    [Header("Stance Distance Modifiers")]
    public float offensiveDistanceMultiplier = 0.5f;
    public float neutralDistanceMultiplier = 1.0f;
    public float defensiveDistanceMultiplier = 2.0f;

    [Header("Offensive Playbook")]
    public List<FormationData> offensiveFormations = new List<FormationData>();

    [Header("Neutral Playbook")]
    public List<FormationData> neutralFormations = new List<FormationData>();

    [Header("Defensive Playbook")]
    public List<FormationData> defensiveFormations = new List<FormationData>();
}