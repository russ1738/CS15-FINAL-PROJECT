using UnityEngine;

public class DialogueNpc : MonoBehaviour, IInteractable
{
    public string speakerName = "Professor";
    public string[] lines =
    {
        "The server room is locked. You will need the USB key.",
        "A power switch near the gap builds a bridge.",
        "Collect coins along the way. Good luck!"
    };

    public string Prompt => "Press E to talk to " + speakerName;

    public void Interact(PlayerController player)
    {
        GameManager.Instance.StartDialogue(speakerName, lines);
    }
}
