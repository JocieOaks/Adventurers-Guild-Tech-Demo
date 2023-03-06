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
    static readonly Sprite[] sprites = new Sprite[] { Graphics.Instance.Stool };

    // Initialized the first time GetMaskPixels is called, _pixels are the sprite mask for all Stools.
    static bool[,] _pixels;
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

    public void EndPlayerInteraction(PlayerPawn pawn)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    public void Enter(Pawn pawn)
    {
        pawn.ForcePosition(WorldPosition + Vector3Int.back);
        pawn.Occupying = this;
        Occupant = pawn;
    }

    /// <inheritdoc/>
    public void Exit(Pawn pawn, Vector3Int exitTo = default)
    {
        if (pawn == Occupant)
        {
            Occupant = null;
        }
        if (pawn.Occupying == this)
        {
            pawn.Occupying = null;
        }
        if (exitTo != default)
        {
            pawn.ForcePosition(exitTo);
        }
        else
        {
            RoomNode roomNode = InteractionPoints.FirstOrDefault(x => x.Traversable);
            if (roomNode == default)
            {
                //Emergency option if there's no interaction points to move to.
                pawn.ForcePosition(Vector3Int.one);
            }
            else
            {
                pawn.ForcePosition(roomNode.WorldPosition);
            }
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
    public override float SpeedMultiplier(Vector3Int nodePosition)
    {
        if (nodePosition == WorldPosition)
            return 0.5f;
        else return 1;
    }

    public void StartPlayerInteraction(PlayerPawn pawn)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    protected override void OnMapChanging()
    {
        _interactionPoints = null;
        ReserveInteractionPoints();
    }
}
