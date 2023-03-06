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

    /// <summary>
    /// Tests if the <see cref="IDividerNode"/> borders a given <see cref="Room"/>.
    /// </summary>
    /// <param name="room">The <see cref="Room"/> being tested.</param>
    /// <returns>Returns true if the <c>room</c> is on one side of the <see cref="IDividerNode"/> or the other.</returns>
    public bool AdjacentToRoom(Room room)
    {
        return FirstNode.Room == room || SecondNode.Room == room;
    }

    /// <summary>
    /// Gives the <see cref="RoomNode"/> that is on the other side of the <see cref="IDividerNode"/> from <see cref="RoomNode"/>.
    /// </summary>
    /// <param name="node"><see cref="RoomNode"/> that is being evaluated.</param>
    /// <returns>Returns the <see cref="RoomNode"/> that is on the opposite side of the <see cref="IDividerNode"/>.</returns>
    /// <exception cref="System.ArgumentException">Throws this error if the given <see cref="RoomNode"/> is not one of the two <see cref="RoomNode"/>s connected by the <see cref="ConnectingNode"/>.</exception>
    RoomNode GetOppositeRoomNode(RoomNode node)
    {
        if (node == FirstNode)
            return SecondNode;
        else if (node == SecondNode)
            return FirstNode;
        else
        {
            throw new System.ArgumentException();
        }
    }
}
