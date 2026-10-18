using UnityEngine;

public class Shopkeeper : MonoBehaviour, IInteractable
{
    public string Prompt => "Press E to open the coin shop";

    public void Interact(PlayerController player)
    {
        GameManager.Instance?.OpenShop();
    }
}
