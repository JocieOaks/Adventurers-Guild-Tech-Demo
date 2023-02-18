using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Linq;

/// <summary>
/// The <see cref="StoolSprite"/> class is a <see cref="SpriteObject"/> for stool furniture.
/// </summary>
[System.Serializable]
public class StoolSprite : SpriteObject, IOccupied
{
    // Initialized the first time GetMaskPixels is called, _pixels are the sprite mask for all Stools.
    static bool[,] _pixels;
    static Sprite[] sprites = new Sprite[] { Graphics.Instance.Stool };

    List<RoomNode> _interactionPoints;

    /// <summary>
    /// Initializes a new instance of the <see cref="StoolSprite"/> class.
    /// </summary>
    /// <param name="worldPosition">The <see cref="IWorldPosition.WorldPosition"/> of the  <see cref="StoolSprite"/>.</param>
    [JsonConstructor]
    public StoolSprite(Vector3Int worldPosition)
        : base(1, sprites, Direction.Undirected, worldPosition, "Stool", ObjectDimensions, true)
    {
        StanceSit.SittingObjects.Add(this);
    }

    /// <value>The 3D dimensions of a <see cref="StoolSprite"/> in terms of <see cref="Map"/> coordinates.</value>
    public static new Vector3Int ObjectDimensions { get; } = new Vector3Int(1, 1, 2);

    /// <inheritdoc/>
    [JsonIgnore]
    public override IEnumerable<bool[,]> GetMaskPixels
    {
        get
        {
            if (_pixels == default)
            {
                BuildPixelArray(Graphics.Instance.Stool, ref _pixels);
            }

            yield return _pixels;
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
                _interactionPoints = new List<RoomNode>();
                for (int i = -2; i < 2; i++)
                {
                    for (int j = -2; j < 2; j++)
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
    protected override string ObjectType { get; } = "Stool";

    /// <summary>
    /// Checks if a new <see cref="StoolSprite"/> can be created at a given <see cref="Map"/> position.
    /// </summary>
    /// <param name="position"><see cref="Map"/> position to check.</param>
    /// <returns>Returns true a <see cref="StoolSprite"/> can be created at <c>position</c>.</returns>
    public static bool CheckObject(Vector3Int position)
    {
        return Map.Instance.CanPlaceObject(position, ObjectDimensions);
    }

    /// <summary>
    /// Initializes a new <see cref="StoolSprite"/> at the given <see cref="Map"/> position.
    /// </summary>
    /// <param name="position"><see cref="Map"/> position to create the new <see cref="StoolSprite"/>.</param>
    public static void CreateStool(Vector3Int position)
    {
        new StoolSprite(position);
    }

    /// <summary>
    /// Places a highlight object with a <see cref="StoolSprite"/> <see cref="Sprite"/> at the given position.
    /// </summary>
    /// <param name="highlight">The highlight game object that is being placed.</param>
    /// <param name="position"><see cref="Map"/> position to place the highlight.</param>
    public static void PlaceHighlight(SpriteRenderer highlight, Vector3Int position)
    {
        if (CheckObject(position))
        {
            highlight.enabled = true;
            highlight.sprite = Graphics.Instance.Stool;
            highlight.transform.position = Map.MapCoordinatesToSceneCoordinates(position);
            highlight.sortingOrder = Graphics.GetSortOrder(position);
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
        pawn.WorldPositionNonDiscrete = WorldPosition + Vector3Int.back;
        Occupant = pawn;
    }

    /// <inheritdoc/>
    public void Exit(Pawn pawn)
    {
        if (pawn == Occupant)
        {
            Occupant = null;

            RoomNode roomNode = InteractionPoints.First();
            pawn.WorldPositionNonDiscrete = roomNode.WorldPosition; 
        }
    }

    /// <inheritdoc/>
    public void ReserventeractionPoints()
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
        ReserventeractionPoints();
    }
}
