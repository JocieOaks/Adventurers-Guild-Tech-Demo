using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The <see cref="Bar"/> class is a <see cref="SpriteObject"/> for bar furniture.
/// </summary>
[System.Serializable]
public class Bar : LinearSpriteObject, IInteractable
{

    // Initialized the first time GetMaskPixels is called, _pixelsX and _pixelsY are the sprite mask for all Bars.
    static bool[,] _pixelsX;
    static bool[,] _pixelsY;
    static Sprite[] sprites = new Sprite[] { Graphics.Instance.BarX[0], Graphics.Instance.BarY[0], Graphics.Instance.BarX[0], Graphics.Instance.BarY[0] };

    List<RoomNode> _interactionPoints;

    /// <summary>
    /// Initializes a new instance of the <see cref="Bar"/> class.
    /// </summary>
    /// <param name="direction">The <see cref="Direction"/> the <see cref="Bar"/> is facing.</param>
    /// <param name="worldPosition">The position in <see cref="Map"/> coordinates of the <see cref="Bar"/>.</param>
    [JsonConstructor]
    public Bar(Direction direction, Vector3Int worldPosition)
        : base(2, sprites, direction, worldPosition, "Bar", ObjectDimensions, true)
    {
        _spriteRenderers[1].sprite = Alignment == MapAlignment.XEdge ? Graphics.Instance.BarX[1] : Graphics.Instance.BarY[1];
        _spriteRenderers[1].sortingOrder = Graphics.GetSortOrder(WorldPosition + (Alignment == MapAlignment.XEdge ? Vector3Int.down : Vector3Int.left));
    }

    /// <value>The 3D dimensions of a <see cref="Bar"/> in terms of <see cref="Map"/> coordinates.</value>
    public new static Vector3Int ObjectDimensions { get; } = new Vector3Int(1, 2, 2);

    /// <inheritdoc/>
    [JsonIgnore]
    public override IEnumerable<bool[,]> GetMaskPixels
    {
        get
        {
            if (Alignment == MapAlignment.XEdge)
            {
                if (_pixelsX == default)
                {
                    BuildPixelArray(Graphics.Instance.BarX, ref _pixelsX);
                }

                yield return _pixelsX;
            }
            else
            {
                if (_pixelsY == default)
                {
                    BuildPixelArray(Graphics.Instance.BarY, ref _pixelsY);
                }

                yield return _pixelsY;
            }
        }
    }

    /// <inheritdoc/>
    public IEnumerable<RoomNode> InteractionPoints
    {
        get
        {
            if (_interactionPoints == null)
            {
                _interactionPoints = new List<RoomNode>();

                if (Alignment == MapAlignment.XEdge)
                {
                    int i = 0;
                    while (Map.Instance[WorldPosition + Vector3Int.right * i].Occupant is Bar)
                    {
                        RoomNode roomNode = Map.Instance[WorldPosition + Vector3Int.right * i + 2 * Vector3Int.down];
                        if (roomNode.Traversible)
                            _interactionPoints.Add(roomNode);
                        i++;
                    }
                    i = 1;
                    while (Map.Instance[WorldPosition + Vector3Int.left * i].Occupant is Bar)
                    {
                        RoomNode roomNode = Map.Instance[WorldPosition + Vector3Int.left * i + 2 * Vector3Int.down];
                        if (roomNode.Traversible)
                            _interactionPoints.Add(roomNode);
                        i++;
                    }
                }
                else
                {
                    int i = 0;
                    while (Map.Instance[WorldPosition + Vector3Int.up * i].Occupant is Bar)
                    {
                        RoomNode roomNode = Map.Instance[WorldPosition + Vector3Int.up * i + 2 * Vector3Int.left];
                        if (roomNode.Traversible)
                            _interactionPoints.Add(roomNode);
                        i++;
                    }
                    i = 1;
                    while (Map.Instance[WorldPosition + Vector3Int.down * i].Occupant is Bar)
                    {
                        RoomNode roomNode = Map.Instance[WorldPosition + Vector3Int.down * i + 2 * Vector3Int.left];
                        if (roomNode.Traversible)
                            _interactionPoints.Add(roomNode);
                        i++;
                    }
                }
            }

            return _interactionPoints;
        }
    }

    /// <inheritdoc/>
    [JsonProperty]
    protected override string ObjectType { get; } = "Bar";

    /// <summary>
    /// Checks if a new <see cref="Bar"/> can be created at a given <see cref="Map"/> position.
    /// </summary>
    /// <param name="position"><see cref="Map"/> position to check.</param>
    /// <returns>Returns true if a <see cref="Bar"/> can be created at <c>position</c>.</returns>
    public static bool CheckObject(Vector3Int position)
    {
        Vector3Int dimensions = default;
        switch (BuildFunctions.Direction)
        {
            case Direction.North:
            case Direction.South:
                dimensions = ObjectDimensions;
                break;

            case Direction.East:
            case Direction.West:
                dimensions = new Vector3Int(ObjectDimensions.y, ObjectDimensions.x, ObjectDimensions.z);
                break;
        }
        return Map.Instance.CanPlaceObject(position, dimensions);
    }

    /// <summary>
    /// Initializes a new <see cref="Bar"/> at the given <see cref="Map"/> position.
    /// </summary>
    /// <param name="position"><see cref="Map"/> position to create the new <see cref="Bar"/>.</param>
    public static void CreateBar(Vector3Int position)
    {
        new Bar(BuildFunctions.Direction, position);
    }

    /// <summary>
    /// Places a highlight object with a <see cref="Bar"/> <see cref="Sprite"/> at the given position.
    /// </summary>
    /// <param name="highlight">The highlight game object that is being placed.</param>///
    /// <param name="position"><see cref="Map"/> position to place the highlight.</param>
    public static void PlaceHighlight(SpriteRenderer highlight, Vector3Int position)
    {
        if (CheckObject(position))
        {
            highlight.enabled = true;

            highlight.sprite = Map.DirectionToEdgeAlignment(BuildFunctions.Direction) == MapAlignment.XEdge ? Graphics.Instance.BarX[0] : Graphics.Instance.BarY[0];

            highlight.transform.position = Map.MapCoordinatesToSceneCoordinates(position);
            highlight.sortingOrder = Graphics.GetSortOrder(position);
        }
        else
            highlight.enabled = false;
    }

    /// <inheritdoc/>
    public override void Destroy()
    {
        AcquireFoodTask.FoodSources.Remove(this);
        base.Destroy();
    }

    /// <inheritdoc/>
    public void ReserventeractionPoints()
    { }

    /// <inheritdoc/>
    protected override void OnConfirmingObjects()
    {
        AcquireFoodTask.FoodSources.Add(this);
        base.OnConfirmingObjects();
    }

    /// <inheritdoc/>
    protected override void OnMapChanging()
    {
        _interactionPoints = null;
    }
}