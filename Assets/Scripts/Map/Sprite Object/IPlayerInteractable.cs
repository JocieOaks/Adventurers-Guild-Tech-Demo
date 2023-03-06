
/// <summary>
/// The <see cref="IPlayerInteractable"/> interface is the interface for <see cref="IInteractable"/>s that can also be interacted with by the player.
/// </summary>
public interface IPlayerInteractable : IInteractable
{
    /// <summary>
    /// Performs whatever interaction is intended for when the player begins to interact with <see cref="IPlayerInteractable"/>.
    /// </summary>
    public void StartPlayerInteraction();

    /// <summary>
    /// Performs whatever interaction is intended for when the player stops interacting with <see cref="IPlayerInteractable"/>.
    /// </summary>
    public void EndPlayerInteraction();

}