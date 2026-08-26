using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPipeBuilder : MonoBehaviour
{
    [SerializeField] private float interactRange = 5f;

    [Header("Pipe Visuals")]
    [SerializeField] private Material pipeMaterial;
    [SerializeField] private float pipeThickness = 0.2f;

    [Header("Player Connections")]
    [SerializeField] private Transform pipeHoldPoint;

    private WaterComponent startNode;
    private GameObject temporaryPipe;
    private PlayerViews playerViews;

    private void Awake()
    {
        playerViews = GetComponent<PlayerViews>();

        if (pipeHoldPoint == null)
        {
            pipeHoldPoint = transform;
        }
    }

    public void OnInteractWater(InputValue value)
    {
        if (!value.isPressed) return;

        WaterComponent nearbyNode = FindNearestWaterNode();

        if (startNode == null)
        {
            // Start connecting pipes
            if (nearbyNode != null)
            {
                startNode = nearbyNode;

                temporaryPipe = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(temporaryPipe.GetComponent<Collider>());

                temporaryPipe.AddComponent<WaterView>();

                if (pipeMaterial != null) temporaryPipe.GetComponent<Renderer>().material = pipeMaterial;

                // Get the player in water view mode
                if (playerViews != null)
                {
                    playerViews.ForceWaterView(true);
                }
            }
        }
        else
        {
            // Finish connecting pipes
            if (nearbyNode != null && nearbyNode != startNode)
            {
                startNode.ConnectedNodes.Add(nearbyNode);
                nearbyNode.ConnectedNodes.Add(startNode);

                StretchCubeBetweenPoints(temporaryPipe.transform, startNode.WaterReceiver.position, nearbyNode.WaterReceiver.position);

                startNode = null;
                temporaryPipe = null;
            }
            else
            {
                CancelPiping();
            }
        }
    }

    public void OnEscape(InputValue value)
    {
        if (!value.isPressed) return;

        if (startNode != null)
        {
            CancelPiping();
        }
    }

    private void Update()
    {
        // Update shows only the temporary pipe
        if (startNode != null && temporaryPipe != null)
        {
            StretchCubeBetweenPoints(temporaryPipe.transform, startNode.WaterReceiver.position, pipeHoldPoint.position);
        }
    }

    private void CancelPiping()
    {
        startNode = null;
        if (temporaryPipe != null) Destroy(temporaryPipe);
    }

    private void StretchCubeBetweenPoints(Transform pipeTransform, Vector3 start, Vector3 end)
    {
        Vector3 direction = end - start;
        float distance = direction.magnitude;

        pipeTransform.position = start + (direction / 2f);

        if (direction != Vector3.zero)
        {
            pipeTransform.rotation = Quaternion.LookRotation(direction);
        }

        pipeTransform.localScale = new Vector3(pipeThickness, pipeThickness, distance);
    }

    private WaterComponent FindNearestWaterNode()
    {
        WaterComponent[] allNodes = FindObjectsByType<WaterComponent>();
        WaterComponent nearest = null;
        float minDistance = interactRange;

        foreach (var node in allNodes)
        {
            float dist = Vector3.Distance(transform.position, node.WaterReceiver.position);
            if (dist <= minDistance)
            {
                minDistance = dist;
                nearest = node;
            }
        }

        return nearest;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
#endif
}
