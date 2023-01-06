using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class StairSprite : AreaSpriteObject
{
    public static void CreateStair(Vector3Int position)
    {
        Direction direction = BuildFunctions.Direction;
        if(Map.Instance[position].TryGetNodeAs(~direction, out Stair stairNode, false))
            position.z = stairNode.WorldPosition.z + 1;

        if (!CheckObject(position))
            return;

        Layer layer = Map.Instance[position.z];

        int z = position.z - layer.Origin.z;

        new StairSprite(position, z, direction);
    }

    public static void PlaceHighlight(SpriteRenderer highlight, Vector3Int position)
    {
        Direction direction = BuildFunctions.Direction;
        if (Map.Instance[position].TryGetNodeAs(~direction, out Stair stairNode, false))
            position.z = stairNode.WorldPosition.z + 1;

        if (CheckObject(position))
        {
            highlight.enabled = true;

            switch (BuildFunctions.Direction)
            {
                case Direction.North:
                    highlight.sprite = Graphics.Instance.StairsNorth;
                    break;
                case Direction.South:
                    highlight.sprite = Graphics.Instance.StairsSouth;
                    break;
                case Direction.East:
                    highlight.sprite = Graphics.Instance.StairsEast;
                    break;
                case Direction.West:
                    highlight.sprite = Graphics.Instance.StairsWest;
                    break;
            };
            highlight.transform.position = Map.MapCoordinatesToSceneCoordinates(MapAlignment.Center, position);
            highlight.sortingOrder = Graphics.GetSortOrder(position);
        }
        else
            highlight.enabled = false;
        
    }

    public static bool CheckObject(Vector3Int position)
    {
        return Map.Instance.CanPlaceObject(position, ObjectDimensions) && GameManager.Instance.IsOnLevel(position.z) <= 0;
    }

    public static new Vector3Int ObjectDimensions = Vector3Int.one;

    public Direction Direction { get; }

    public override Vector3 OffsetVector => Vector3.down * 2;

    static bool[,] _pixelsNorth;
    static bool[,] _pixelsSouth;
    static bool[,] _pixelsEast;
    static bool[,] _pixelsWest;
    static bool[,] _pixelsCube;

    public override IEnumerable<bool[,]> GetMaskPixels
    {
        get
        {
            switch(Direction)
            {
                case Direction.North:

                    if(_pixelsNorth == null)
                    {
                        BuildPixelArray(Graphics.Instance.StairsNorth, ref _pixelsNorth);
                    }

                    yield return _pixelsNorth;
                    break;
                case Direction.South:

                    if (_pixelsSouth == null)
                    {
                        BuildPixelArray(Graphics.Instance.StairsSouth, ref _pixelsSouth);
                    }

                    yield return _pixelsSouth;
                    break;
                case Direction.East:

                    if (_pixelsEast == null)
                    {
                        BuildPixelArray(Graphics.Instance.StairsEast, ref _pixelsEast);
                    }

                    yield return _pixelsEast;
                    break;
                case Direction.West:

                    if (_pixelsWest == null)
                    {
                        BuildPixelArray(Graphics.Instance.StairsWest, ref _pixelsWest);
                    }

                    yield return _pixelsWest;
                    break;
            }

            if (_pixelsCube == null)
            {
                BuildPixelArray(Graphics.Instance.Cube, ref _pixelsCube);
            }

            for (int i = 1; i < _spriteRenderer.Length; i++)
            {
                yield return _pixelsCube;
            }
        }
    }

    SortingGroup _sortingGroup;

    public StairSprite(Vector3Int position, int z, Direction direction) : base(z + 1, null, position, "Stair", ObjectDimensions, false)
    {
        Direction = direction;
        switch (direction)
        {
            case Direction.North:
                Sprite = Graphics.Instance.StairsNorth;
                break;
            case Direction.South:
                Sprite = Graphics.Instance.StairsSouth;
                break;
            case Direction.East:
                Sprite = Graphics.Instance.StairsEast;
                break;
            case Direction.West:
                Sprite = Graphics.Instance.StairsWest;
                break;
        }

        SpriteRenderer.sortingOrder = 0;
        SpriteRenderer.enabled = GameManager.Instance.IsOnLevel(WorldPosition.z) <= 0;

        _sortingGroup = GameObject.AddComponent<SortingGroup>();
        _sortingGroup.sortingOrder = Graphics.GetSortOrder(position + z * Vector3Int.back);

        for (int i = 1; i < z + 1; i++)
        {
            SpriteRenderer current = _spriteRenderer[i];

            current.transform.localPosition = Vector3Int.down * i * 2;
            current.name = "Stair";
            current.sortingOrder = -i;
            current.sprite = Graphics.Instance.Cube;
        }
    }

    Stair _stair;

    public StairSprite(Vector3Int position, int z, Direction direction, Stair stair) : this(position, z, direction)
    {
        _stair = stair;
        Confirm();
    }

    protected override void Confirm()
    {
        RoomNode roomNode = Map.Instance[WorldPosition];

        if(_stair == null)
            _stair = new Stair(this, roomNode, WorldPosition.z, Direction);

        base.Confirm();
    }

    public override void Destroy()
    {
        RoomNode roomNode = Map.Instance[WorldPosition];
        (roomNode as Stair)?.Destroy();
        base.Destroy();
    }
}


public class Stair : RoomNode
{
    public Direction Direction { get; }

    StairSprite stairSprite;

    public override Vector3Int SurfacePosition => WorldPosition + Vector3Int.forward;

    public Stair(Vector3Int position, Direction direction) : base(Map.Instance[position])
    {
        Direction = direction;

        if (RoomPosition.z == Room.Height - 1)
        {
            new Landing(this, direction);
        }

        stairSprite = new StairSprite(WorldPosition, RoomPosition.z, direction, this);
    }

    public Stair(StairSprite sprite, RoomNode node, int z, Direction direction) : base(node)
    {
        Direction = direction;
        stairSprite = sprite;
        SetZ(z - Room.Origin.z);
        if (RoomPosition.z == Room.Height - 1)
        {
            new Landing(this, direction);
        }
    }

    public void Destroy()
    {
        SetZ(Room.Origin.z);
        new RoomNode(this);
    }

    public override T GetNodeAs<T>(Direction direction, bool traversible = true)
    {
        if (typeof(T) == typeof(RoomNode) && direction == Direction && traversible)
        {
            if (GetNode(direction) is T node)
            {
                if (node.WorldPosition.z == WorldPosition.z + 1)
                {
                    return node;
                }
                else
                    return default(T);
            }
            else
                return default(T);
        }
        else
        {
            return base.GetNodeAs<T>(direction, traversible);
        }
    }
}
