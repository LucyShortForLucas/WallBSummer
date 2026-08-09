using UnityEngine;
using UnityEngine.UI;

public class InteractableUIController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private Transform rowContainer;
    [SerializeField] private GameObject rowPrefab;

    private void Awake()
    {
        mainPanel.SetActive(false);
    }

    public void OpenPanel(ResourceStorage playerStorage, ResourceStorage buildingStorage)
    {
        // Clear out old resource rows 
        foreach (Transform child in rowContainer)
        {
            Destroy(child.gameObject);
        }

        // Instantiate a new UI row
        foreach (ResourceData res in buildingStorage.GetAllTrackedResources())
        {
            GameObject rowObj = Instantiate(rowPrefab, rowContainer);
            ResourceUIRow rowScript = rowObj.GetComponent<ResourceUIRow>();

            rowScript.Setup(res, playerStorage, buildingStorage);
        }

        mainPanel.SetActive(true);
    }

    public void ClosePanel()
    {
        mainPanel.SetActive(false);
    }
}