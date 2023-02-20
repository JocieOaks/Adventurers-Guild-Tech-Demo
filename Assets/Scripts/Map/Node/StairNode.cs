using UnityEngine;

/// <summary>
/// The <see cref="StairNode"/> is a child of <see cref="RoomNode"/> that has a directed slant, an sits at two different z positions.
/// </summary>
public class StairNode : RoomNode, IDirected
{
    readonly StairSprite stairSprite;

    /// <summary>
    /// Initializes a new instance of the <see cref="StairNode"/> class that does not already have an associated <see cref="StairSprite"/>.
    /// </summary>
    /// <param name="position">The <see cref="IWorldPosition.WorldPosition"/> of the <see cref="StairNode"/>.</param>
    /// <param name="direction">The <see cref="global::Direction"/> the <see cref="StairNode"/> is facing.</param>
    public StairNode(Vector3Int position, Direction direction) : base(Map.Instance[position])
    {
        Direction = direction;

        if (RoomPosition.z == Room.Height - 1)
        {
            new LandingConnector(this, direction);
        }

        stairSprite = new StairSprite(direction, WorldPosition, RoomPosition.z, this);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StairNode"/> class that already has an associated <see cref="StairSprite"/>, and is replacing a pre-existing <see cref="RoomNode"/>.
    /// </summary>
    /// <param name="sprite">The <see cref="StairSprite"/> associated with this <see cref="StairNode"/>.</param>
    /// <param name="node">The <see cref="RoomNode"/> this <see cref="StairNode"/> is taking the place of.</param>
    /// <param name="z">The vertical position, relative to <see cref="Room.Origin"/> that the <see cref="StairNode"/> will be placed.</param>
    /// <param name="direction">The <see cref="global::Direction"/> the <see cref="StairNode"/> is facing.</param>
    public StairNode(StairSprite sprite, RoomNode node, int z, Direction direction) : base(node)
    {
        Direction = direction;
        stairSprite = sprite;
        SetZ(z - Room.Origin.z);
        if (RoomPosition.z == Room.Height - 1)
        {
            new LandingConnector(this, direction);
        }
    }

    /// <inheritdoc/>
    public Direction Direction { get; }

    /// <inheritdoc/>
    public override Vector3Int SurfacePosition => WorldPosition + Vector3Int.forward;

    /// <inheritdoc/>
    public void Destroy()
    {
        SetZ(Room.Origin.z);
        RoomNode copy = new(this);
        if(copy.TryGetNodeAs(Direction, out LandingConnector landing))
            landing.Disconnect();
    }

    /// <inheritdoc/>
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
