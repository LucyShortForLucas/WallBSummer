using UnityEngine;

[CreateAssetMenu(fileName = "FormationData", menuName = "Entity/Robot/Formation Data")]
public class FormationData : ScriptableObject
{
    public enum FormationShape { Spear, Wall, Box }

    [Header("Shape")]
    [SerializeField] private FormationShape shape = FormationShape.Wall;

    [Header("Command Distances")]
    [SerializeField] private float safeCommandDistance = 15f;
    [SerializeField] private float breakFormationDistance = 6f;
    [SerializeField] private float recallDistance = 25f;

    [Header("Structural Math")]
    [SerializeField] private float spacing = 2.5f;
    [SerializeField] private float forwardOffset = 5f;

    // Getters and Setters
    public FormationShape Shape { get => shape; }
    public float SafeCommandDistance { get => safeCommandDistance; }
    public float BreakFormationDistance { get => breakFormationDistance; }
    public float RecallDistance { get => recallDistance; }
    public float Spacing { get => spacing; }
    public float ForwardOffset { get => forwardOffset; }

}