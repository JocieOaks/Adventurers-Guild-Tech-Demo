using UnityEngine;

/// <summary>
/// Interface for objects that exist within the game <see cref="Map"/>.
/// </summary>
public interface IWorldPosition
{
    /// <value>The position of an <see cref="IWorldPosition"/> within a tile. Very few objects have an alignment that is not <see cref="MapAlignment.Center"/>.</value>
    MapAlignment Alignment { get; }

    /// <value>The 3D dimensions of the <see cref="IWorldPosition"/> in terms of <see cref="Map"/> coordinates. 
    Vector3Int Dimensions { get; }

    /// <value>Determines where the lower corner of <see cref="Dimensions"/> is. For most objects it is at <see cref="WorldPosition"/>.</value>
    Vector3Int NearestCornerPosition { get; }

    /// <value>Gives the <see cref="INode"/> the object currently occupies. <see cref="INode"/> objects return themselves. Everything else returns <see cref="RoomNode"/>.</value>
    INode Node { get; }

    /// <value>Gives the <see cref="global::Room"/> the object currently occupies. Can be <c>null</c> for objects that border two rooms, i.e. <see cref="WallBlocker"/> and <see cref="ConnectingNode"/>.</value>
    Room Room { get; }

    /// <value>Gives the position of the object in <see cref="Map"/> coordinates.</value>
    Vector3Int WorldPosition { get; }
    /// <summary>
    /// Method for <see cref="Room.Navigate(IWorldPosition, IWorldPosition)"/>. Determines if the given node is an acceptable endpoint when navigating to this <see cref="IWorldPosition"/>.
    /// </summary>
    /// <param name="node">The potential endpoint.</param>
    /// <returns>Returns true if the given <see cref="RoomNode"/> is an acceptable endpoint for navigation.</returns>
    bool HasNavigatedTo(RoomNode node);
}
