/// <summary>
/// Anything the player can use with E implements this interface.
/// CS15 idea: one contract, many object types (NPC, item, switch, door).
/// </summary>
public interface IInteractable
{
    string Prompt { get; }
    void Interact(PlayerController player);
}
