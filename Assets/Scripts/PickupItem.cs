using UnityEngine;

public enum PickupKind
{
    Coin,
    UsbKey
}

public class PickupItem : MonoBehaviour, IInteractable
{
    public PickupKind kind = PickupKind.Coin;
    public string keyTakenLine = "Got the key. Head to the door.";

    public string Prompt => kind == PickupKind.UsbKey
        ? "Press E to take the key"
        : "Press E to collect a coin";

    public void Interact(PlayerController player)
    {
        if (kind == PickupKind.UsbKey)
        {
            GameManager.Instance.GiveUsbKey();
            GameManager.Instance.StartDialogue("You", new[] { keyTakenLine });
        }
        else
        {
            GameManager.Instance.AddCoin();
        }

        Destroy(gameObject);
    }

    void Update()
    {
        transform.Rotate(0f, 0f, 40f * Time.deltaTime);
    }
}
