using UnityEngine;

public class InventoryInteractionsAudioEvents : MonoBehaviour
{
    [SerializeField] private AK.Wwise.Event _inventoryOpenEvent;
    [SerializeField] private AK.Wwise.Event _inventoryCloseEvent;
    private StorageInteractable _inventoryInteractableReference;
    private void Awake()
    {
        _inventoryInteractableReference = GetComponent<StorageInteractable>();
    }
    private void OnEnable()
    {
        _inventoryInteractableReference.OnOpenStorage += InventoryOpen;
        _inventoryInteractableReference.OnCloseStorage += InventoryClose;
    }
    private void OnDisable()
    {
        _inventoryInteractableReference.OnOpenStorage -= InventoryOpen;
        _inventoryInteractableReference.OnCloseStorage -= InventoryClose;
    }
    private void InventoryOpen()
    {
        _inventoryOpenEvent.Post(gameObject);
    }
    private void InventoryClose()
    {
        _inventoryCloseEvent.Post(gameObject);
    }
}
