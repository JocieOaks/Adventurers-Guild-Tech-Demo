using UnityEngine;

/// <summary>
/// Class <see cref="WallBlocker"/> is a <see cref="BlockingNode"/> that divides two <see cref="RoomNode"/>s via a <see cref="global::WallSprite"/>.
/// </summary>
public class WallBlocker : BlockingNode
{
    /// <summary>
    /// Initializes a new instace of the <see cref="WallBlocker"/> class for a <see cref="global::WallSprite"/> that has already been constructed.
    /// </summary>
    /// <param name="wallSprite">The <see cref="global::WallSprite"/> asscoiated with this <see cref="WallBlocker"/>.</param>
    /// <param name="worldPosition">The <see cref="IWorldPosition.WorldPosition"/> of the <see cref="WallBlocker"/>.</param>
    /// <param name="alignment">The <see cref="MapAlignment"/> of the <see cref="WallBlocker"/>.</param>
    public WallBlocker(WallSprite wallSprite, Vector3Int worldPosition, MapAlignment alignment) : base(worldPosition, alignment)
    {
        WallSprite = wallSprite;

        if (alignment == MapAlignment.XEdge)
        {
            Map.Instance[worldPosition.x, worldPosition.y, worldPosition.z].SetNode(Direction.South, this);
        }
        else
        {
            Map.Instance[worldPosition.x, worldPosition.y, worldPosition.z].SetNode(Direction.West, this);
        }
    }

    /// <summary>
    /// Initializes a new instace of the <see cref="WallBlocker"/> class and constructs a new <see cref="global::WallSprite"/> asscoiated with it..
    /// </summary>
    /// <param name="worldPosition">The <see cref="IWorldPosition.WorldPosition"/> of the <see cref="WallBlocker"/>.</param>
    /// <param name="alignment">The <see cref="MapAlignment"/> of the <see cref="WallBlocker"/>.</param>
    public WallBlocker(Vector3Int worldPosition, MapAlignment alignment) : base(worldPosition, alignment)
    {
        WallSprite = new WallSprite(worldPosition, alignment, 6, WallMaterial.Brick, this);
    }

    /// <inheritdoc/>
    public override Vector3Int Dimensions => WallSprite.Dimensions;

    /// <value>The <see cref="global::WallSprite"/> associated with this <see cref="WallBlocker"/>.</value>
    public WallSprite WallSprite { get; }

    /// <summary>
    /// Removes this wall from the <see cref="Map"/> connecting the two <see cref="RoomNode"/>s the <see cref="WallBlocker"/> was between.
    /// </summary>
    public void RemoveWall()
    {
        if(FirstNode.WorldPosition == WorldPosition) 
        {
            FirstNode.SetNode(Alignment == MapAlignment.XEdge ? Direction.South : Direction.West, SecondNode);
        }
        else
        {
            SecondNode.SetNode(Alignment == MapAlignment.XEdge ? Direction.South : Direction.West, FirstNode);
        }
    }
}