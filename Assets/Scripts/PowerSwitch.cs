using UnityEngine;

public class PowerSwitch : MonoBehaviour, IInteractable
{
    public GameObject bridge;
    bool used;

    public string Prompt => used ? "Bridge power is on" : "Press E to flip the power switch";

    public void Interact(PlayerController player)
    {
        if (used)
        {
            GameManager.Instance.StartDialogue("Switch", new[] { "The bridge is already powered." });
            return;
        }

        used = true;
        if (bridge != null)
        {
            bridge.SetActive(true);
        }

        GameManager.Instance.MarkBridgeBuilt();
        GameManager.Instance.StartDialogue("Switch", new[] { "Power restored. A bridge snapped into place across the gap." });
        GetComponent<SpriteRenderer>().color = new Color(0.2f, 0.9f, 0.4f);
    }
}
