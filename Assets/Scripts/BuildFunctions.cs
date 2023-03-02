using UnityEngine;
using System;
using UnityEngine.UI;

/// <summary>
/// Demarks the type of <see cref="SpriteObject"/>s being built, so that <see cref="GameManager"/> now how to handle <see cref="BuildFunctions"/>.
/// </summary>
public enum BuildMode
{
    /// <summary>Nothing is currently being built.</summary>
    None,
    /// <summary>The <see cref="SpriteObject"/> being built consists of a single object.</summary>
    Point,
    /// <summary>The <see cref="SpriteObject"/> being built is a <see cref="LinearSpriteObject"/> in which multiple objects appear in a line.</summary>
    Line,
    /// <summary>The <see cref="SpriteObject"/> being built is a <see cref="AreaSpriteObject"/> in which multiple objects appear across a broad area.</summary>
    Area,
    /// <summary>Special <see cref="BuildMode"/> for placing a door into a <see cref="WallSprite"/>.</summary>
    Door,
    /// <summary><see cref="SpriteObject"/>s are to be destroyed instead of created.</summary>
    Demolish
}

/// <summary>
/// The <see cref="BuildFunctions"/> class is a static class containing references to methods used in building.
/// To change what is being built, the <see cref="CheckSpriteObject"/>, <see cref="CreateSpriteObject"/>, and <see cref="HighlightSpriteObject"/> are set to reference
/// the corresponding static methods of the <see cref="SpriteObject"/> being created.
/// </summary>
public static class BuildFunctions
{
    static MapAlignment s_alignment;
    static Vector3Int s_areaEnd;
    static Vector3Int s_areaStart;
    static Direction s_direction;
    static int s_lineEnd;
    static Vector3Int s_lineStart;
    public static event Action<Vector3Int, Vector3Int> CheckingAreaConstraints;

    public static event Action<int, int> CheckingLineConstraints;

    public static event Action ConfirmingObjects;

    /// <value>Gives the type of object being built, so that <see cref="GameManager"/> knows how to use the player input.</value>
    public static BuildMode BuildMode { get; set; }

    /// <value>Gives reference to the corresponding <see cref="SpriteObject"/> method to determine 
    /// if the given <see cref="Map"/> coordinates are a valid position for the <see cref="SpriteObject"/>.</value>
    public static Func<Vector3Int,bool> CheckSpriteObject { get; set; }

    /// <value>Gives reference to the corresponding <see cref="SpriteObject"/> method to create a new instance of the <see cref="SpriteObject"/>.</value>
    public static Action<Vector3Int> CreateSpriteObject { get; set; }

    /// <value>The <see cref="global::Direction"/> any newly built <see cref="SpriteObject"/>s will face.</value>
    public static Direction Direction 
    { 
        get => s_direction; 
        set
        {
            s_direction = value;
            s_alignment = Utility.DirectionToEdgeAlignment(s_direction);
        } 
    }

    /// <value>Gives reference to the corresponding <see cref="SpriteObject"/> method to place a highlight of the <see cref="SpriteObject"/> to be built.</value>
    public static Action<SpriteRenderer, Vector3Int> HighlightSpriteObject { get; set; }

    /// <summary>
    /// Confirms all built objects that have not yet been confirmed, adding them permanently to the <see cref="Map"/>.
    /// </summary>
    public static void Confirm()
    {
        ConfirmingObjects?.Invoke();
        s_lineStart = Vector3Int.zero;
        s_lineEnd = -1;
        Graphics.Instance.UpdateGraphics();
    }

    /// <summary>
    /// Destroys the given <see cref="SpriteObject"/>. If <c>spriteObject</c> is a <see cref="WallSprite"/> with a door, the door is removed instead.
    /// </summary>
    /// <param name="spriteObject">The <see cref="SpriteObject"/> being destroyed.</param>
    public static void Demolish(SpriteObject spriteObject)
    {
        if (spriteObject is WallSprite wall && wall.IsDoor)
        {
            wall.RemoveDoor();
        }
        else
        {
            spriteObject.Destroy();
        }
        Graphics.Instance.UpdateGraphics();
    }

    /// <summary>
    /// Places <see cref="SpriteObject"/>s, as determined by <see cref="CreateSpriteObject"/>, in a rectangle between <see cref="s_areaStart"/> and the given position.
    /// </summary>
    /// <param name="endPosition">The end point of the area, in <see cref="Map"/> coordinates.</param>
    public static void PlaceArea(Vector3Int endPosition)
    {
        if (s_areaEnd != endPosition)
        {
            int minX = s_areaStart.x < s_areaEnd.x ? s_areaStart.x : s_areaEnd.x;
            int maxX = s_areaStart.x > s_areaEnd.x ? s_areaStart.x : s_areaEnd.x;
            int minY = s_areaStart.y < s_areaEnd.y ? s_areaStart.y : s_areaEnd.y;
            int maxY = s_areaStart.y > s_areaEnd.y ? s_areaStart.y : s_areaEnd.y;

            for (int i = minX < endPosition.x ? minX : endPosition.x; i <= (maxX > endPosition.x ? maxX : endPosition.x); i++)
            {
                for (int j = minY < endPosition.y ? minY : endPosition.y; j <= (maxY > endPosition.y ? maxY : endPosition.y); j++)
                {
                    Vector3Int position = new(i, j, endPosition.z);
                    if ((i < minX || i > maxX || j < minY || j > maxY) && CheckSpriteObject(position))
                    {
                        CreateSpriteObject(position);
                    }
                }
            }

            s_areaEnd = endPosition;
            CheckingAreaConstraints?.Invoke(s_areaStart, s_areaEnd);
        }
    }

    /// <summary>
    /// Creates a door on the <see cref="WallSprite"/> that is at the given position.
    /// </summary>
    /// <param name="position">The <see cref="IWorldPosition.WorldPosition"/> of the <see cref="WallSprite"/>.</param>
    /// <param name="alignment">The <see cref="MapAlignment"/> of the <see cref="WallSprite"/>.</param>
    /// <param name="_">The <see cref="AccentMaterial"/> for the door. CURRENTLY UNUSED</param>
    public static void PlaceDoor(Vector3Int position, MapAlignment alignment, AccentMaterial _)
    {
        if (WallSprite.CheckDoor(position, alignment))
        {
            WallSprite.CreateDoor(position, alignment);
        }
    }

    /// <summary>
    /// Sets all <see cref="RoomNode"/>s in a given <see cref="Room"/> to have the floorType.
    /// </summary>
    /// <param name="room">The <see cref="Room"/> where the <see cref="FloorSprite"/> is being placed.</param>
    /// <param name="floorType">The index of the floor's <see cref="Sprite"/>.</param>
    public static void PlaceFloor(Room room, int floorType)
    {
        foreach (RoomNode node in room.Nodes)
        {
            Vector3Int position = room.GetWorldPosition(node);
            if (Map.Instance.IsSupported(position, MapAlignment.Center))
                Map.Instance[position].Floor.SpriteIndex = floorType;
        }
    }

    /// <summary>
    /// Places <see cref="SpriteObject"/>s, as determined by <see cref="CreateSpriteObject"/>, in a line between the given positions.
    /// </summary>
    /// <param name="endPoint">The position of the end. May not necessarily be along the line, so only the parameter on the alignment is actually used.</param>
    public static void PlaceLine(Vector3Int endPoint)
    {
        int end = AlignedCoordinate(endPoint);
        int start = AlignedCoordinate(s_lineStart);
        if (s_lineEnd != end && end != start)
        {
            bool switchPoints = false;
            

            if (start > end)
            {
                start = end;
                end = s_lineEnd;
                switchPoints = true;
            }

            for (int i = start; i <= end; i++)
            {
                if (i < AlignedCoordinate(s_lineStart) || i > s_lineEnd)
                {
                    Vector3Int position;
                    if (s_alignment == MapAlignment.XEdge)
                    {
                        position = new(i, s_lineStart.y, s_lineStart.z);
                    }
                    else
                    {
                        position = new(s_lineStart.x, i, s_lineStart.z);
                    }


                    if (CheckSpriteObject(position))
                    {
                        CreateSpriteObject(position);
                    }
                }
            }

            if (switchPoints)
            {
                s_lineStart = s_alignment == MapAlignment.XEdge ? new Vector3Int(start, s_lineStart.y, s_lineStart.z) : new Vector3Int(s_lineStart.x, start, s_lineStart.z);
            }

            s_lineEnd = end;


            CheckingLineConstraints?.Invoke(start, end);
        }
    }

    /// <summary>
    /// Sets the starting point for placing <see cref="AreaSpriteObject"/>s.
    /// </summary>
    /// <param name="position">The starting point of the area, in <see cref="Map"/> coordinates.</param>
    public static void StartPlacingArea(Vector3Int position)
    {
        s_areaStart = position;
        s_areaEnd = position;

        if (CheckSpriteObject(position))
        {
            CreateSpriteObject(position);
        }

    }

    /// <summary>
    /// Sets the starting point for placing <see cref="LinearSpriteObject"/>s.
    /// </summary>
    /// <param name="position">The starting point of the line, in <see cref="Map"/> coordinates.</param>
    public static void StartPlacingLine(Vector3Int position)
    {
        s_lineStart = position;
        s_lineEnd = AlignedCoordinate(position);
        if (CheckSpriteObject(position))
        {
            CreateSpriteObject(position);
        }
    }

    /// <summary>
    /// Gets the coordinate of a <see cref="Vector3Int"/> that is along a <see cref="MapAlignment"/>.
    /// </summary>
    /// <param name="position">The position being evaluated.</param>
    /// <returns>Returns either the x or y coordinate of <c>position</c> depending on <see cref="s_alignment"/>.</returns>
    static int AlignedCoordinate(Vector3Int position)
    {
        return s_alignment == MapAlignment.XEdge ? position.x : position.y;
    }
}
