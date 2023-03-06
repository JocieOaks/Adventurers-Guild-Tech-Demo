using System.Collections.Generic;

/// <summary>
/// Interface for <see cref="SpriteObject"/>s that can be interacted with by <see cref="AdventurerPawn"/>s.
/// </summary>
public interface IInteractable : ISpriteObject
{
    /// <value>The <see cref="List{T}"/> of all <see cref="RoomNode"/>s from which a <see cref="AdventurerPawn"/> can interact with this <see cref="IInteractable"/>.</value>
    public IEnumerable<RoomNode> InteractionPoints { get; }

    /// <summary>
    /// Sets all <see cref="InteractionPoints"/> to be reserved, so that a <see cref="AdventurerPawn"/> cannot block them.
    /// </summary>
    public void ReserveInteractionPoints();
}

/// <summary>
/// The <see cref="IPlayerInteractable"/> interface is the interface for <see cref="IInteractable"/>s that can also be interacted with by the player.
/// </summary>
public interface IPlayerInteractable : IInteractable
{
    /// <summary>
    /// Performs whatever interaction is intended for when the player begins to interact with <see cref="IPlayerInteractable"/>.
    /// </summary>
    public void StartPlayerInteraction(PlayerPawn pawn);

    /// <summary>
    /// Performs whatever interaction is intended for when the player stops interacting with <see cref="IPlayerInteractable"/>.
    /// </summary>
    /// <param name="pawn"></param>
    public void EndPlayerInteraction(PlayerPawn pawn);

}