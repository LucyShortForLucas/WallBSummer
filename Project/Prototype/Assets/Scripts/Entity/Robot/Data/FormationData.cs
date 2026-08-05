using UnityEngine;

[CreateAssetMenu(fileName = "FormationData", menuName = "Entity/Robot/Formation Data")]
public class FormationData : ScriptableObject
{
    public enum FormationShape { Spear, Wall, Box }

    [Header("Shape")]
    public FormationShape shape = FormationShape.Wall;

    [Header("Command Distances")]
    public float safeCommandDistance = 15f;
    public float breakFormationDistance = 6f;
    public float recallDistance = 25f;

    [Header("Structural Math")]
    public float spacing = 2.5f;
    public float forwardOffset = 5f;
}