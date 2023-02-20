using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class <see cref="DoorConnector"/> is a <see cref="ConnectingNode"/> that connects two <see cref="Room"/>s via a doorway.
/// </summary>
public class DoorConnector : ConnectingNode
{
    readonly bool _locked = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="DoorConnector"/> class.
    /// </summary>
    /// <param name="connection1">The first <see cref="RoomNode"/> that the <see cref="DoorConnector"/> connects.</param>
    /// <param name="connection2">The second <see cref="RoomNode"/> that the <see cref="DoorConnector"/> connects.</param>
    /// <param name="worldPosition">The position of the <see cref="DoorConnector"/> in <see cref="Map"/> coordinates.</param>
    /// <param name="alignment">The <see cref="MapAlignment"/> of the door.</param>
    public DoorConnector(RoomNode connection1, RoomNode connection2, Vector3Int worldPosition) : base(connection1, connection2, worldPosition)
    {
        if (Alignment == MapAlignment.XEdge)
        {
            WallNode = connection1.GetNodeAs<WallBlocker>(Direction.North);
            connection1.SetNode(Direction.North, this);
            connection2.SetNode(Direction.South, this);
        }
        else
        {
            WallNode = connection1.GetNodeAs<WallBlocker>(Direction.East);
            connection1.SetNode(Direction.East, this);
            connection2.SetNode(Direction.West, this);
        }
    }

    /// <inheritdoc/>
    public override bool Obstructed => _locked;

    /// <value>The <see cref="WallBlocker"/> that this <see cref="DoorConnector"/> is replacing.</value>
    public WallBlocker WallNode { get;}
    /// <inheritdoc/>
    public override void Disconnect()
    {
        if(Alignment == MapAlignment.XEdge)
        {
            FirstNode.SetNode(Direction.North,WallNode);
            SecondNode.SetNode(Direction.South,WallNode);
        }
        else
        {
            FirstNode.SetNode(Direction.East,WallNode);
            SecondNode.SetNode(Direction.West,WallNode);
        }

        if (IsWithinSingleRoom)
        {
            Room.RemoveConnection(this);
        }
        else
        {
            FirstNode.Room.RemoveConnection(this);
            SecondNode.Room.RemoveConnection(this);
        }
    }
}
