using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface for <see cref="SpriteObject"/>s that can be interacted with by <see cref="Pawn"/>s.
/// </summary>
public interface IInteractable : IWorldPosition
{
    public IEnumerable<RoomNode> InteractionPoints { get; }

    public void Reserve();
}
