using UnityEngine;

public class LockedDoor : MonoBehaviour, IInteractable
{
    public string lockedHint = "It needs a key from this level.";
    public string unlockedPrompt = "Press E to go through the door";

    public string Prompt => GameManager.Instance != null && GameManager.Instance.HasUsbKey
        ? unlockedPrompt
        : "Press E to try the locked door";

    public void Interact(PlayerController player)
    {
        if (!GameManager.Instance.HasUsbKey)
        {
            GameManager.Instance.StartDialogue("Door", new[] { lockedHint });
            return;
        }

        GameManager.Instance.AdvanceLevel();
    }
}
