using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Linq;

/// <summary>
/// The <see cref="ChairSprite"/> class is a <see cref="SpriteObject"/> for chair furniture.
/// </summary>
[System.Serializable]
public class ChairSprite : SpriteObject, IOccupied, IDirected
{
    // Initialized the first time GetMaskPixels is called for each given direction., _pixelsEast, _pixelsNorth, _pixelsSouth, and _pixelsWest are the sprite mask for all Chairs.
    static bool[,] _pixelsEast;
    static bool[,] _pixelsNorth;
    static bool[,] _pixelsSouth;
    static bool[,] _pixelsWest;
    static readonly Sprite[] sprites = new Sprite[] { Graphics.Instance.ChairNorth, Graphics.Instance.ChairEast, Graphics.Instance.ChairSouth, Graphics.Instance.ChairWest };

    List<RoomNode> _interactionPoints;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChairSprite"/> class.
    /// </summary>
    /// <param name="direction">The <see cref="Direction"/> the <see cref="ChairSprite"/> is facing.</param>
    /// <param name="worldPosition">The <see cref="IWorldPosition.WorldPosition"/> of the  <see cref="ChairSprite"/>.</param>
    [JsonConstructor]
    public ChairSprite(Direction direction, Vector3Int worldPosition)
        : base(1,  sprites, direction, worldPosition, "Chair", ObjectDimensions, true)
    {
        Direction = direction;
        StanceSit.SittingObjects.Add(this);
    }

    /// <value>The 3D dimensions of a <see cref="ChairSprite"/> in terms of <see cref="Map"/> coordinates.</value>
    public static new Vector3Int ObjectDimensions { get; } = new Vector3Int(1, 1, 2);

    /// <inheritdoc/>
    public Direction Direction { get; private set; }

    ///<inheritdoc/>
    [JsonIgnore]
    public override IEnumerable<bool[,]> GetMaskPixels
    {
        get
        {
            switch (Direction)
            {
                case Direction.North:
                    if (_pixelsNorth == default)
                    {
                        BuildPixelArray(Graphics.Instance.ChairNorth, ref _pixelsNorth);
                    }
                    yield return _pixelsNorth;
                    yield break;
                case Direction.South:
                    if (_pixelsSouth == default)
                    {
                        BuildPixelArray(Graphics.Instance.ChairSouth, ref _pixelsSouth);
                    }
                    yield return _pixelsSouth;
                    yield break;
                case Direction.East:
                    if (_pixelsEast == default)
                    {
                        BuildPixelArray(Graphics.Instance.ChairEast, ref _pixelsEast);
                    }
                    yield return _pixelsEast;
                    yield break;

                default:
                    if (_pixelsWest == default)
                    {
                        BuildPixelArray(Graphics.Instance.ChairWest, ref _pixelsWest);
                    }
                    yield return _pixelsWest;
                    yield break;
            }
        }
    }

    [JsonIgnore]
    /// <inheritdoc/>
    public IEnumerable<RoomNode> InteractionPoints
    {
        get
        {
            if (_interactionPoints == null)
            {
                int minX = -2;
                int minY = -2;
                int maxX = 2;
                int maxY = 2;

                switch (Direction)
                {
                    case Direction.North:
                        minY = 0;
                        break;
                    case Direction.South:
                        maxY = 0;
                        break;
                    case Direction.East:
                        minX = 0;
                        break;
                    case Direction.West:
                        maxX = 0;
                        break;
                }

                _interactionPoints = new List<RoomNode>();
                for (int i = minX; i < maxX; i++)
                {
                    for (int j = minY; j < maxY; j++)
                    {
                        RoomNode roomNode = Map.Instance[WorldPosition + new Vector3Int(i, j)];
                        if (roomNode.Traversable)
                            _interactionPoints.Add(roomNode);
                    }
                }
            }
            return _interactionPoints;
        }
    }

    /// <inheritdoc/>
    [JsonIgnore]
    public Pawn Occupant { get; set; }

    /// <inheritdoc/>
    [JsonIgnore]
    public bool Occupied => Occupant != null;

    /// <inheritdoc/>
    [JsonProperty]
    protected override string ObjectType { get; } = "Chair";

    /// <summary>
    /// Checks if a new <see cref="ChairSprite"/> can be created at a given <see cref="Map"/> position.
    /// </summary>
    /// <param name="position"><see cref="Map"/> position to check.</param>
    /// <returns>Returns true a <see cref="ChairSprite"/> can be created at <c>position</c>.</returns>
    public static bool CheckObject(Vector3Int position)
    {
        return Map.Instance.CanPlaceObject(position, ObjectDimensions);
    }

    /// <summary>
    /// Initializes a new <see cref="ChairSprite"/> at the given <see cref="Map"/> position.
    /// </summary>
    /// <param name="position"><see cref="Map"/> position to create the new <see cref="ChairSprite"/>.</param>
    public static void CreateChair(Vector3Int position)
    {
        new ChairSprite(BuildFunctions.Direction, position);
    }

    /// <summary>
    /// Places a highlight object with a <see cref="ChairSprite"/> <see cref="Sprite"/> at the given position.
    /// </summary>
    /// <param name="highlight">The highlight game object that is being placed.</param>
    /// <param name="position"><see cref="Map"/> position to place the highlight.</param>
    public static void PlaceHighlight(SpriteRenderer highlight, Vector3Int position)
    {
        if (CheckObject(position))
        {
            highlight.enabled = true;

            switch (BuildFunctions.Direction)
            {
                case Direction.North:
                    highlight.sprite = Graphics.Instance.ChairNorth;
                    break;
                case Direction.South:
                    highlight.sprite = Graphics.Instance.ChairSouth;
                    break;
                case Direction.East:
                    highlight.sprite = Graphics.Instance.ChairEast;
                    break;
                case Direction.West:
                    highlight.sprite = Graphics.Instance.ChairWest;
                    break;
            };
            highlight.transform.position = Utility.MapCoordinatesToSceneCoordinates(position);
            highlight.sortingOrder = Utility.GetSortOrder(position);
        }
        else
            highlight.enabled = false;
    }

    /// <inheritdoc/>
    public override void Destroy()
    {
        StanceSit.SittingObjects.Remove(this);
        base.Destroy();
    }

    /// <inheritdoc/>
    public void Enter(Pawn pawn)
    {
        if (pawn is NPC || Direction == Direction.South || Direction == Direction.West)
            pawn.WorldPositionNonDiscrete = WorldPosition + Vector3Int.back;
        else
            pawn.WorldPositionNonDiscrete = WorldPosition;
        Occupant = pawn;
    }

    /// <inheritdoc/>
    public void Exit(Pawn pawn)
    {
        if (pawn == Occupant)
        {
            Occupant = null;
        }
        RoomNode roomNode = InteractionPoints.FirstOrDefault(x => x.Traversable);
        if (roomNode == default)
        {
            //Emergency option if there's no interaction points to move to.
            pawn.WorldPositionNonDiscrete = Vector3Int.one;
        }
        else
        {
            pawn.WorldPositionNonDiscrete = roomNode.WorldPosition;
        }
    }

    /// <inheritdoc/>
    public void ReserveInteractionPoints()
    {
        foreach (RoomNode roomNode in InteractionPoints)
        {
            roomNode.Reserved = true;
        }
    }

    /// <inheritdoc/>
    protected override void OnMapChanging()
    {
        _interactionPoints = null;
        ReserveInteractionPoints();
    }
}
