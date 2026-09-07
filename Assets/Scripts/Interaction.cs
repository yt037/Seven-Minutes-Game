// Interaction.cs
// Anything the player can aim at and press E on.
// Prompt is the text shown in the crosshair label. Return null to show nothing
// (the carpet does this until the player knows there is a key under it) while
// still allowing Interact to work.
public interface Interaction
{
    string Prompt { get; }
    void Interact(Player player);
}
