using UnityEngine;

/// <summary>
/// Interface for objects that exist within the game <see cref="Map"/>.
/// </summary>
public interface IWorldPosition
{
    /// <value>Gives the position of the object in <see cref="Map"/> coordinates.</value>
    Vector3Int WorldPosition { get; }

    /// <value>Gives the <see cref="Room"/> the object currently occupies. Can be <c>null</c> for objects that border two rooms, i.e. <see cref="WallNode"/> and <see cref="ConnectionNode"/>.</value>
    Room Room { get; }

    /// <value>Gives the <see cref="INode"/> the object currently occupies. <see cref="INode"/> objects return themselves. Everything else returns <see cref="RoomNode"/>.</value>
    INode Node { get; }

    /// <summary>
    /// Method for <see cref="Room.Navigate(IWorldPosition, IWorldPosition)"/>. Determines if the given node is an acceptable endpoint when navigating to this <see cref="IWorldPosition"/>.
    /// </summary>
    /// <param name="node">The potential endpoint.</param>
    /// <returns>Returns true if the given <see cref="RoomNode"/> is an acceptable endpoint for navigation.</returns>
    bool HasNavigatedTo(RoomNode node);
}
