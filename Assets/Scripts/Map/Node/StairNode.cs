using UnityEngine;
public class StairNode : RoomNode
{
    public Direction Direction { get; }

    Stair stairSprite;

    public override Vector3Int SurfacePosition => WorldPosition + Vector3Int.forward;

    public StairNode(Vector3Int position, Direction direction) : base(Map.Instance[position])
    {
        Direction = direction;

        if (RoomPosition.z == Room.Height - 1)
        {
            new Landing(this, direction);
        }

        stairSprite = new Stair(direction, WorldPosition, RoomPosition.z, this);
    }

    public StairNode(Stair sprite, RoomNode node, int z, Direction direction) : base(node)
    {
        Direction = direction;
        stairSprite = sprite;
        SetZ(z - Room.Origin.z);
        if (RoomPosition.z == Room.Height - 1)
        {
            new Landing(this, direction);
        }
    }

    public void Destroy()
    {
        SetZ(Room.Origin.z);
        new RoomNode(this);
    }

    public override T GetNodeAs<T>(Direction direction, bool traversible = true)
    {
        if (typeof(T) == typeof(RoomNode) && traversible && direction != ~Direction)
        {
            if (GetNode(direction) is T node)
            {
                if (direction == Direction)
                {
                    if (node.WorldPosition.z == WorldPosition.z + 1)
                    {
                        return node;
                    }
                }
                else if (node is StairNode && node.WorldPosition.z == WorldPosition.z)
                {
                    return node;
                }
            }
        }
        else
        {
            return base.GetNodeAs<T>(direction, traversible);
        }
        return default(T);
    }
}
