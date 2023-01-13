using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Linq;

[System.Serializable]
public class Chair : DirectionalSpriteObject, IOccupied
{
    static bool[,] _pixelsEast;

    static bool[,] _pixelsNorth;

    static bool[,] _pixelsSouth;

    static bool[,] _pixelsWest;

    [JsonConstructor]
    public Chair(Direction direction, Vector3Int worldPosition)
        : base(1, Graphics.Instance.ChairNorth, Graphics.Instance.ChairSouth, Graphics.Instance.ChairEast, Graphics.Instance.ChairWest, direction, worldPosition, "Chair", ObjectDimensions, true)
    {
        StanceSit.SittingObjects.Add(this);
    }

    public static new Vector3Int ObjectDimensions { get; } = new Vector3Int(1, 1, 2);

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
    public Actor Occupant { get; set; }

    [JsonIgnore]
    public bool Occupied => Occupant != null;

    [JsonProperty]
    protected override string ObjectType { get; } = "Chair";

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
        return Map.Instance.CanPlaceObject(position, default);
    }

    public static void CreateChair(Vector3Int position)
    {
        new Chair(BuildFunctions.Direction, position);
    }

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
            highlight.transform.position = Map.MapCoordinatesToSceneCoordinates(position);
            highlight.sortingOrder = Graphics.GetSortOrder(position);
        }
        else
            highlight.enabled = false;
    }
    public override void Destroy()
    {
        StanceSit.SittingObjects.Remove(this);
        base.Destroy();
    }

    public void Enter(Pawn pawn)
    {
        if (pawn is NPC || Direction == Direction.South || Direction == Direction.West)
            pawn.WorldPositionNonDiscrete = WorldPosition + Vector3Int.back;
        else
            pawn.WorldPositionNonDiscrete = WorldPosition;
        Occupant = pawn.Actor;
    }

    public void Exit(Pawn pawn)
    {
        Occupant = null;

        RoomNode roomNode = InteractionPoints.First();
        pawn.WorldPositionNonDiscrete = roomNode.WorldPosition;
    }

    List<RoomNode> _interactionPoints;

    public IEnumerable<RoomNode> InteractionPoints
    {
        get
        {
            if(_interactionPoints == null)
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
                        if (roomNode.Traversible)
                            _interactionPoints.Add(roomNode);
                    }
                }
            }
            return _interactionPoints;
        }
    }

    protected override void OnMapChanging()
    {
        _interactionPoints = null;
        Reserve();
    }

    public void Reserve()
    {
        foreach(RoomNode roomNode in InteractionPoints)
        {
            roomNode.Reserved = true;
        }
    }
}
