using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The <see cref="IDividerNode"/> interface is for <see cref="INode"/>s that separate two <see cref="RoomNode"/>s.
/// </summary>
public interface IDividerNode : INode
{
    /// <value>The first <see cref="RoomNode"/> connected to the <see cref="IDividerNode"/>.</value>
    RoomNode FirstNode { get; }

    /// <value>The second <see cref="RoomNode"/> connected to the <see cref="IDividerNode"/>.</value>
    RoomNode SecondNode { get; }
    
    /// <value>The <see cref="MapAlignment"/> of the <see cref="IDividerNode"/>.</value>
    MapAlignment Alignment { get; }

    /// <summary>
    /// Tests if the <see cref="IDividerNode"/> borders a given <see cref="Room"/>.
    /// </summary>
    /// <param name="room">The <see cref="Room"/> being tested.</param>
    /// <returns>Returns true if the <c>room</c> is on one side of the <see cref="IDividerNode"/> or the other.</returns>
    public bool AdjacentToRoom(Room room)
    {
        return FirstNode.Room == room || SecondNode.Room == room;
    }
}
