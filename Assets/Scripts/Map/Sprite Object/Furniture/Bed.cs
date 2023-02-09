﻿using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Linq;

/// <summary>
/// The <see cref="Bed"/> class is a <see cref="SpriteObject"/> for bed furniture.
/// </summary>
[System.Serializable]
public class Bed : SpriteObject, IOccupied
{
    // Initialized the first time GetMaskPixels is called, _pixels is the sprite mask for all Beds.
    static bool[,] _pixels;
    static Sprite[] sprites = new Sprite[] { Graphics.Instance.BedSprite[1] };

    List<RoomNode> _interactionPoints;

    [JsonConstructor]

    /// <summary>
    /// Initializes a new instance of the <see cref="Bed"/> class.
    /// </summary>
    /// <param name="worldPosition">The position in <see cref="Map"/> coordinates of the <see cref="Bed"/>.</param>
    public Bed(Vector3Int worldPosition) :
        base(5, sprites, Direction.Undirected, worldPosition, "Bed", ObjectDimensions, true)
    {
        StanceLay.LayingObjects.Add(this);
        _spriteRenderers[1].sprite = Graphics.Instance.BedSprite[0];
        _spriteRenderers[1].sortingOrder = Graphics.GetSortOrder(WorldPosition + Vector3Int.up);

        for (int i = 2; i < _spriteRenderers.Length; i++)
        {
            _spriteRenderers[i].sprite = Graphics.Instance.BedSprite[i];
            _spriteRenderers[i].sortingOrder = Graphics.GetSortOrder(WorldPosition + Vector3Int.right * (i - 1));
        }

    }

    /// <value>The 3D dimensions of a <see cref="Bed"/> in terms of <see cref="Map"/> coordinates.</value>
    public static new Vector3Int ObjectDimensions { get; } = new Vector3Int(4, 2, 1);

    /// <inheritdoc/>
    [JsonIgnore]
    public override IEnumerable<bool[,]> GetMaskPixels
    {
        get
        {
            if (_pixels == default)
            {
                BuildPixelArray(Graphics.Instance.BedSprite, ref _pixels);
            }

            yield return _pixels;
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
                for (int i = -2; i < 6; i++)
                {
                    for (int j = -2; j < 4; j++)
                    {
                        RoomNode roomNode = Map.Instance[WorldPosition + new Vector3Int(i, j)];
                        if (roomNode.Traversible)
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
    
    /// <value>The <see cref="Pawn"/> that owns this <see cref="Bed"/>.</value>
    public Pawn Owner { get; private set; }

    /// <inheritdoc/>
    [JsonProperty]
    protected override string ObjectType { get; } = "Bed";

    /// <summary>
    /// Checks if a new <see cref="Bed"/> can be created at a given <see cref="Map"/> position.
    /// </summary>
    /// <param name="position"><see cref="Map"/> position to check.</param>
    /// <returns>Returns true a <see cref="Bed"/> can be created at <c>position</c>.</returns>
    public static bool CheckObject(Vector3Int position)
    {
        return Map.Instance.CanPlaceObject(position, ObjectDimensions);
    }

    /// <summary>
    /// Initializes a new <see cref="Bed"/> at the given <see cref="Map"/> position.
    /// </summary>
    /// <param name="position"><see cref="Map"/> position to create the new <see cref="Bed"/>.</param>
    public static void CreateBed(Vector3Int position)
    {
        new Bed(position);
    }

    /// <summary>
    /// Places a highlight object with a <see cref="Bed"/> <see cref="Sprite"/> at the given position.
    /// </summary>
    /// <param name="highlight">The highlight game object that is being placed.</param>
    /// <param name="position"><see cref="Map"/> position to place the highlight.</param>
    public static void PlaceHighlight(SpriteRenderer highlight, Vector3Int position)
    {
        if (CheckObject(position))
        {
            highlight.enabled = true;
            highlight.flipX = false;
            highlight.sprite = Graphics.Instance.BedSprite[1];
            highlight.transform.position = Map.MapCoordinatesToSceneCoordinates(position);
            highlight.sortingOrder = Graphics.GetSortOrder(position);
        }
        else
            highlight.enabled = false;
    }

    /// <inheritdoc/>
    public override void Destroy()
    {
        StanceLay.LayingObjects.Remove(this);
        base.Destroy();
    }

    /// <inheritdoc/>
    public void Enter(Pawn pawn)
    {
        pawn.transform.Rotate(0, 0, -55);
        pawn.WorldPositionNonDiscrete = WorldPosition + Vector3Int.up;
        Occupant = pawn;
    }

    /// <inheritdoc/>
    public void Exit(Pawn pawn)
    {
        if (pawn == Occupant)
        {
            pawn.transform.Rotate(0, 0, 55);
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
