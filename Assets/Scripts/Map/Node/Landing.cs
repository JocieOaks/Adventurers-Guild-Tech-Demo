using UnityEngine;

public class Landing : ConnectionNode
{
    public Direction Direction { get; private set; }
    public Landing(RoomNode connection1, Direction direction) : base(connection1, null, true, Vector3Int.zero)
    {
        _connection2 = Map.Instance[connection1.WorldPosition + Vector3Int.forward + Map.DirToVector(direction)];

        _connection1.SetNode(direction, this);
        _connection2.SetNode(~direction, this);
        Direction = direction;

        switch (direction)
        {
            case Direction.North:
                WorldPosition = _connection2.WorldPosition;
                break;
            case Direction.South:
                WorldPosition = _connection1.WorldPosition;
                break;
            case Direction.East:
                WorldPosition = _connection2.WorldPosition;
                break;
            case Direction.West:
                WorldPosition = _connection1.WorldPosition;
                break;
        }
    }

    public override void RegisterRooms()
    {
        _connection1.Room.AddConnection(this);
        _connection2.Room.AddConnection(this);

        GameManager.MapChanging -= RegisterRooms;
    }

    public override void Disconnect()
    {
        _connection1.SetNode(Direction, _connection2);
        _connection2.SetNode(~Direction, _connection1);
    }

    public override RoomNode GetRoomNode(RoomNode entrance)
    {
        if (entrance == _connection1 || entrance.SurfacePosition == _connection1.SurfacePosition)
            return _connection2;
        else if (entrance == _connection2 || entrance.SurfacePosition == _connection1.SurfacePosition)
            return _connection1;
        else
        {
            throw new System.ArgumentException();
        }
    }
}