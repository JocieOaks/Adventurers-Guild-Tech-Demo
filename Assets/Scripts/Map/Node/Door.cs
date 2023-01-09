using System.Collections.Generic;
using UnityEngine;

public class Door : ConnectionNode
{
    //bool _locked = false;
    //bool _open = true;
    MapAlignment _alignment;
    public WallNode Wall { get; set; }

    List<Pawn> _canUnlockDoor;

    public Door(RoomNode connection1, RoomNode connection2, bool traversible, Vector3Int worldPosition, MapAlignment alignment) : base(connection1, connection2, traversible, worldPosition)
    {
        _alignment = alignment;
        if (alignment == MapAlignment.XEdge)
        {
            Wall = connection1.GetNodeAs<WallNode>(Direction.North);
            connection1.SetNode(Direction.North,this);
            connection2.SetNode(Direction.South,this);
        }
        else
        {
            Wall = connection1.GetNodeAs<WallNode>(Direction.East);
            connection1.SetNode(Direction.East,this);
            connection2.SetNode(Direction.West,this);
        }
    }

    public override void Disconnect()
    {
        if(_alignment == MapAlignment.XEdge)
        {
            _connection1.SetNode(Direction.North,Wall);
            _connection2.SetNode(Direction.South,Wall);
        }
        else
        {
            _connection1.SetNode(Direction.East,Wall);
            _connection2.SetNode(Direction.West,Wall);
        }
    }
}
