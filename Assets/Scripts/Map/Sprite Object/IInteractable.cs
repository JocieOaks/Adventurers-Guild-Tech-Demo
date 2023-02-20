using System.Collections.Generic;

/// <summary>
/// Interface for <see cref="SpriteObject"/>s that can be interacted with by <see cref="Pawn"/>s.
/// </summary>
public interface IInteractable : ISpriteObject
{
    /// <value>The <see cref="List{T}"/> of all <see cref="RoomNode"/>s from which a <see cref="Pawn"/> can interact with this <see cref="IInteractable"/>.</value>
    public IEnumerable<RoomNode> InteractionPoints { get; }

    /// <summary>
    /// Sets all <see cref="InteractionPoints"/> to be reserved, so that a <see cref="Pawn"/> cannot block them.
    /// </summary>
    public void ReserveInteractionPoints();
}
