using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// The <see cref="TableRoundSprite"/> class is a <see cref="SpriteObject"/> for round table furniture.
/// </summary>
[System.Serializable]
public class TableRoundSprite : SpriteObject
{
    // Initialized the first time GetMaskPixels is called, _pixels are the sprite mask for all TableRounds.
    static bool[,] _pixels;
    static Sprite[] sprites = new Sprite[] { Graphics.Instance.TableRound[0] };

    [JsonConstructor]

    /// <summary>
    /// Initializes a new instance of the <see cref="TableRoundSprite"/> class.
    /// </summary>
    /// <param name="worldPosition">The <see cref="IWorldPosition.WorldPosition"/> of the  <see cref="TableRoundSprite"/>.</param>
    public TableRoundSprite(Vector3Int worldPosition)
        : base(3, sprites, Direction.Undirected, worldPosition, "Round Table", new Vector3Int(1, 1, 2), true)
    {
        Dimensions = ObjectDimensions;
        _spriteRenderers[1].sprite = Graphics.Instance.TableRound[1];
        _spriteRenderers[1].sortingOrder = Graphics.GetSortOrder(WorldPosition + Vector3Int.up);
        _spriteRenderers[2].sprite = Graphics.Instance.TableRound[2];
        _spriteRenderers[2].sortingOrder = Graphics.GetSortOrder(WorldPosition + Vector3Int.right);
    }

    /// <value>The 3D dimensions of a <see cref="TableRoundSprite"/> in terms of <see cref="Map"/> coordinates.</value>
    public static new Vector3Int ObjectDimensions { get; } = new Vector3Int(3, 3, 2);

    /// <value>he 3D dimensions of the <see cref="SpriteObject"/> in terms of <see cref="Map"/> coordinates. 
    /// Normally should be equivalent to <see cref="ObjectDimensions"/> but can be publicly accessed without knowing the <see cref="SpriteObject"/>'s type.</value>
    [JsonIgnore]
    public new Vector3Int Dimensions { get; }

    /// <inheritdoc/>
    [JsonIgnore]
    public override IEnumerable<bool[,]> GetMaskPixels
    {
        get
        {
            if (_pixels == default)
            {
                BuildPixelArray(Graphics.Instance.TableRound, ref _pixels);
            }

            yield return _pixels;
        }
    }

    /// <inheritdoc/>
    [JsonProperty]
    protected override string ObjectType { get; } = "TableRound";

    /// <summary>
    /// Checks if a new <see cref="TableRoundSprite"/> can be created at a given <see cref="Map"/> position.
    /// </summary>
    /// <param name="position"><see cref="Map"/> position to check.</param>
    /// <returns>Returns true a <see cref="TableRoundSprite"/> can be created at <c>position</c>.</returns>
    public static bool CheckObject(Vector3Int position)
    {
        return Map.Instance.CanPlaceObject(position, ObjectDimensions);
    }

    /// <summary>
    /// Initializes a new <see cref="TableRoundSprite"/> at the given <see cref="Map"/> position.
    /// </summary>
    /// <param name="position"><see cref="Map"/> position to create the new <see cref="TableRoundSprite"/>.</param>
    public static void CreateTableRound(Vector3Int position)
    {
        new TableRoundSprite(position);
    }

    /// <summary>
    /// Places a highlight object with a <see cref="TableRoundSprite"/> <see cref="Sprite"/> at the given position.
    /// </summary>
    /// <param name="highlight">The highlight game object that is being placed.</param>
    /// <param name="position"><see cref="Map"/> position to place the highlight.</param>
    public static void PlaceHighlight(SpriteRenderer highlight, Vector3Int position)
    {
        if (CheckObject(position))
        {
            highlight.enabled = true;
            highlight.sprite = Graphics.Instance.TableRound[0];
            highlight.transform.position = Map.MapCoordinatesToSceneCoordinates(position);
            highlight.sortingOrder = Graphics.GetSortOrder(position);
        }
        else
            highlight.enabled = false;
    }
}
