using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[System.Serializable]
public class Bed : SpriteObject, IOccupied
{
    public static void PlaceHighlight(SpriteRenderer highlight, Vector3Int position)
    {
        if (CheckObject(position))
        {
            highlight.enabled = true;
            highlight.flipX = false;
            highlight.sprite = Graphics.Instance.BedSprite[1];
            highlight.transform.position = Map.MapCoordinatesToSceneCoordinates(MapAlignment.Center, position);
            highlight.sortingOrder = Graphics.GetSortOrder(position);
        }
        else
            highlight.enabled = false;
    }

    public static void CreateBed(Vector3Int position)
    {
        new Bed(position);
    }

    public static bool CheckObject(Vector3Int position)
    {
        return Map.Instance.CanPlaceObject(position, ObjectDimensions);
    }

    public static new Vector3Int ObjectDimensions { get; } = new Vector3Int(4, 2, 1);

    /*static (Color32[] array, int width, int height) _pixels;

    public override (Color32[], int, int) GetPixels
    {
        get
        {
            if(_pixels == default)
            {
                _pixels.array = Graphics.Instance.BedSprite[0].texture.GetPixels32();
                for (int i = 1; i < Graphics.Instance.BedSprite.Length; i++)
                {
                    Color32[] array = Graphics.Instance.BedSprite[i].texture.GetPixels32();
                    for (int j = 0; j < _pixels.array.Length; j++)
                    {
                        if (array[j].a > 0)
                        {
                            _pixels.array[j] = Color.black;
                        }
                    }
                }
                _pixels.width = Graphics.Instance.BedSprite[0].texture.width;
                _pixels.height = Graphics.Instance.BedSprite[0].texture.height;
            }

            return _pixels;
        }
    }*/

    public Pawn Owner { get; private set; }

    public bool Occupied => Occupant != null;

    public Actor Occupant { get; set; }

    [JsonProperty]
    protected override string ObjectType { get; } = "Bed";

    [JsonConstructor]
    public Bed(Vector3Int worldPosition) :
        base(5, Graphics.Instance.BedSprite[1], worldPosition, "Bed", ObjectDimensions, true)
    {
        StanceLay.LayingObjects.Add(this);
        _spriteRenderer[1].sprite = Graphics.Instance.BedSprite[0];
        _spriteRenderer[1].sortingOrder = Graphics.GetSortOrder(WorldPosition + Vector3Int.up);

        for(int i = 2; i < _spriteRenderer.Length; i++) 
        {
            _spriteRenderer[i].sprite = Graphics.Instance.BedSprite[i];
            _spriteRenderer[i].sortingOrder = Graphics.GetSortOrder(WorldPosition + Vector3Int.right * (i - 1));
        }
        
    }

    public override void Destroy()
    {
        StanceLay.LayingObjects.Remove(this);
        base.Destroy();
    }

    public List<RoomNode> GetInteractionPoints()
    {
        List<RoomNode> interactionPoints = new List<RoomNode>();
        for(int i = -2; i < 6; i++)
        {
            for(int j = -2; j < 4; j++)
            {
                RoomNode roomNode = Map.Instance[WorldPosition + new Vector3Int(i, j)];
                if(roomNode.Traversible)
                    interactionPoints.Add(roomNode);
            }
        }

        return interactionPoints;
    }

    public void Enter(Pawn pawn)
    {
        pawn.transform.Rotate(0, 0, -55);
        pawn.WorldPosition = WorldPosition + Vector3Int.up;
        Occupant = pawn.Actor;
    }

    public void Exit(Pawn pawn)
    {
        pawn.transform.Rotate(0, 0, 55);
        Occupant = null;
        RoomNode roomNode = GetInteractionPoints()[0];
        pawn.WorldPosition = roomNode.WorldPosition;
    }
}

[System.Serializable]
public class Chair : DirectionalSpriteObject, IOccupied
{
    [JsonProperty]
    protected override string ObjectType { get; } = "Chair";

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
            highlight.transform.position = Map.MapCoordinatesToSceneCoordinates(MapAlignment.Center, position);
            highlight.sortingOrder = Graphics.GetSortOrder(position);
        }
        else
            highlight.enabled = false;
    }

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

    /*static (Color32[] array, int width, int height) _pixelsNorth;
    static (Color32[] array, int width, int height) _pixelsEast;
    static (Color32[] array, int width, int height) _pixelsSouth;
    static (Color32[] array, int width, int height) _pixelsWest;

    public override (Color32[], int, int) GetPixels
    {
        get
        {
            switch(Direction)
            {
                case Direction.North:
                    if (_pixelsNorth == default)
                    {
                        _pixelsNorth.array = Graphics.Instance.ChairNorth.texture.GetPixels32();
                    }
                    _pixelsNorth.width = Graphics.Instance.ChairNorth.texture.width;
                    _pixelsNorth.height = Graphics.Instance.ChairNorth.texture.height;
                    return _pixelsNorth;
                case Direction.South:
                    if (_pixelsSouth == default)
                    {
                        _pixelsSouth.array = Graphics.Instance.ChairSouth.texture.GetPixels32();
                    }
                    _pixelsSouth.width = Graphics.Instance.ChairSouth.texture.width;
                    _pixelsSouth.height = Graphics.Instance.ChairSouth.texture.height;
                    return _pixelsSouth;
                case Direction.West:
                    if (_pixelsWest == default)
                    {
                        _pixelsWest.array = Graphics.Instance.ChairWest.texture.GetPixels32();
                    }
                    _pixelsWest.width = Graphics.Instance.ChairWest.texture.width;
                    _pixelsWest.height = Graphics.Instance.ChairWest.texture.height;
                    return _pixelsWest;
                default:
                    if (_pixelsEast == default)
                    {
                        _pixelsEast.array = Graphics.Instance.ChairEast.texture.GetPixels32();
                    }
                    _pixelsEast.width = Graphics.Instance.ChairEast.texture.width;
                    _pixelsEast.height = Graphics.Instance.ChairEast.texture.height;
                    return _pixelsEast;
            }
        }
    }*/

    public List<RoomNode> GetInteractionPoints()
    {
        int minX = -2;
        int minY = -2;
        int maxX = 2;
        int maxY = 2;

        switch(Direction)
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

        List<RoomNode> interactionPoints = new List<RoomNode>();
        for (int i = minX; i < maxX; i++)
        {
            for (int j = minY; j < maxY; j++)
            {
                RoomNode roomNode = Map.Instance[WorldPosition + new Vector3Int(i, j)];
                if (roomNode.Traversible)
                    interactionPoints.Add(roomNode);
            }
        }

        return interactionPoints;
    }

    public static new Vector3Int ObjectDimensions { get; } = new Vector3Int(1, 1, 2);

    public bool Occupied => Occupant != null;

    public Actor Occupant { get; set; }

    [JsonConstructor]
    public Chair(Direction direction, Vector3Int worldPosition) 
        : base(1, Graphics.Instance.ChairNorth, Graphics.Instance.ChairSouth, Graphics.Instance.ChairEast, Graphics.Instance.ChairWest, direction, worldPosition, "Chair", ObjectDimensions, true)
    {
        StanceSit.SittingObjects.Add(this);
    }

    public override void Destroy()
    {
        StanceSit.SittingObjects.Remove(this);
        base.Destroy();
    }

    public void Enter(Pawn pawn)
    {
        if (pawn is NPC || Direction == Direction.South || Direction == Direction.West)
            pawn.WorldPosition = WorldPosition + Vector3Int.back;
        else
            pawn.WorldPosition = WorldPosition;
        Occupant = pawn.Actor;
    }

    public void Exit(Pawn pawn)
    {
        Occupant = null;

        RoomNode roomNode = GetInteractionPoints()[0];
        pawn.WorldPosition = roomNode.WorldPosition;
    }
}

[System.Serializable]
public class Stool : SpriteObject, IOccupied
{
    /*static (Color32[] array, int width, int height) _pixels;

    public override (Color32[], int, int) GetPixels
    {
        get
        {
            if (_pixels == default)
            {
                _pixels.array = Graphics.Instance.Stool.texture.GetPixels32();
                _pixels.width = Graphics.Instance.Stool.texture.width;
                _pixels.height = Graphics.Instance.Stool.texture.height;
            }

            return _pixels;
        }
    }*/

    [JsonProperty]
    protected override string ObjectType { get; } = "Stool";
    public static void PlaceHighlight(SpriteRenderer highlight, Vector3Int position)
    {
        if (CheckObject(position))
        {
            highlight.enabled = true;
            highlight.sprite = Graphics.Instance.Stool;
            highlight.transform.position = Map.MapCoordinatesToSceneCoordinates(MapAlignment.Center, position);
            highlight.sortingOrder = Graphics.GetSortOrder(position);
        }
        else
            highlight.enabled = false;
    }

    public static bool CheckObject(Vector3Int position)
    {
        return Map.Instance.CanPlaceObject(position, ObjectDimensions);
    }

    public static void CreateStool(Vector3Int position)
    {
        new Stool(position);
    }

    public static new Vector3Int ObjectDimensions { get; } = new Vector3Int(1, 1, 2);

    public bool Occupied => Occupant != null;

    public Actor Occupant { get; set; }

    [JsonConstructor]
    public Stool(Vector3Int worldPosition)
        : base(1, Graphics.Instance.Stool, worldPosition, "Stool", ObjectDimensions, true)
    {
        StanceSit.SittingObjects.Add(this);
    }

    public override void Destroy()
    {
        StanceSit.SittingObjects.Remove(this);
        base.Destroy();
    }

    public List<RoomNode> GetInteractionPoints()
    {
        List<RoomNode> interactionPoints = new List<RoomNode>();
        for (int i = -2; i < 2; i++)
        {
            for (int j = -2; j < 2; j++)
            {
                RoomNode roomNode = Map.Instance[WorldPosition + new Vector3Int(i, j)];
                if (roomNode.Traversible)
                    interactionPoints.Add(roomNode);
            }
        }

        return interactionPoints;
    }

    public void Enter(Pawn pawn)
    {
        pawn.WorldPosition = WorldPosition + Vector3Int.back;
        Occupant = pawn.Actor;
    }

    public void Exit(Pawn pawn)
    {
        Occupant = null;

        RoomNode roomNode = GetInteractionPoints()[0];
        pawn.WorldPosition = roomNode.WorldPosition;
    }
}

[System.Serializable]
public class TableRound : SpriteObject
{
    /*static (Color32[] array, int width, int height) _pixels;

    public override (Color32[], int, int) GetPixels
    {
        get
        {
            if (_pixels == default)
            {
                _pixels.array = Graphics.Instance.TableRound[0].texture.GetPixels32();
                for (int i = 1; i < Graphics.Instance.TableRound.Length; i++)
                {
                    Color32[] array = Graphics.Instance.TableRound[i].texture.GetPixels32();
                    for (int j = 0; j < _pixels.array.Length; j++)
                    {
                        if (array[j].a > 0)
                        {
                            _pixels.array[j] = Color.black;
                        }
                    }
                }
                _pixels.width = Graphics.Instance.TableRound[0].texture.width;
                _pixels.height = Graphics.Instance.TableRound[0].texture.height;
            }

            return _pixels;
        }
    }*/

    [JsonProperty]
    protected override string ObjectType { get; } = "TableRound";
    public static void PlaceHighlight(SpriteRenderer highlight, Vector3Int position)
    {
        if (CheckObject(position))
        {
            highlight.enabled = true;
            highlight.sprite = Graphics.Instance.TableRound[0];
            highlight.transform.position = Map.MapCoordinatesToSceneCoordinates(MapAlignment.Center, position);
            highlight.sortingOrder = Graphics.GetSortOrder(position);
        }
        else
            highlight.enabled = false;
    }

    public static bool CheckObject(Vector3Int position)
    {
        return Map.Instance.CanPlaceObject(position, ObjectDimensions);
    }

    public static void CreateTableRound(Vector3Int position)
    {
        new TableRound(position);
    }

    public static new Vector3Int ObjectDimensions { get; } = new Vector3Int(3, 3, 2);

    [JsonIgnore]
    public new Vector3Int Dimensions { get; }

    [JsonConstructor]
    public TableRound(Vector3Int worldPosition)
        : base(3, Graphics.Instance.TableRound[0], worldPosition, "Round Table", new Vector3Int(1,1,2), true)
    {
        Dimensions = ObjectDimensions;
        _spriteRenderer[1].sprite = Graphics.Instance.TableRound[1];
        _spriteRenderer[1].sortingOrder = Graphics.GetSortOrder(WorldPosition + Vector3Int.up);
        _spriteRenderer[2].sprite = Graphics.Instance.TableRound[2];
        _spriteRenderer[2].sortingOrder = Graphics.GetSortOrder(WorldPosition + Vector3Int.right);
    }
}

[System.Serializable]
public class TableSquare : SpriteObject
{

    /*static (Color32[] array, int width, int height) _pixels;

    public override (Color32[], int, int) GetPixels
    {
        get
        {
            if (_pixels == default)
            {
                _pixels.array = Graphics.Instance.TableSquare[0].texture.GetPixels32();
                for (int i = 1; i < Graphics.Instance.TableSquare.Length; i++)
                {
                    Color32[] array = Graphics.Instance.TableSquare[i].texture.GetPixels32();
                    for (int j = 0; j < _pixels.array.Length; j++)
                    {
                        if (array[j].a > 0)
                        {
                            _pixels.array[j] = Color.black;
                        }
                    }
                }
                _pixels.width = Graphics.Instance.TableSquare[0].texture.width;
                _pixels.height = Graphics.Instance.TableSquare[0].texture.height;
            }

            return _pixels;
        }
    }*/

    [JsonProperty]
    protected override string ObjectType { get; } = "TableSquare";
    public static void PlaceHighlight(SpriteRenderer highlight, Vector3Int position)
    {
        if (CheckObject(position))
        {
            highlight.enabled = true;
            highlight.sprite = Graphics.Instance.TableSquare[0];
            highlight.transform.position = Map.MapCoordinatesToSceneCoordinates(MapAlignment.Center, position);
            highlight.sortingOrder = Graphics.GetSortOrder(position);
        }
        else
            highlight.enabled = false;
    }

    public static bool CheckObject(Vector3Int position)
    {
        return Map.Instance.CanPlaceObject(position, ObjectDimensions);
    }

    public static void CreateTableSquare(Vector3Int position)
    {
        new TableSquare(position);
    }

    public static new Vector3Int ObjectDimensions { get; } = new Vector3Int(3, 3, 2);

    [JsonConstructor]
    public TableSquare(Vector3Int worldPosition)
        : base(3, Graphics.Instance.TableSquare[0], worldPosition, "Square Table", ObjectDimensions, true)
    {
        _spriteRenderer[1].sprite = Graphics.Instance.TableSquare[1];
        _spriteRenderer[1].sortingOrder = Graphics.GetSortOrder(WorldPosition + 2 * Vector3Int.up);
        _spriteRenderer[2].sprite = Graphics.Instance.TableSquare[2];
        _spriteRenderer[2].sortingOrder = Graphics.GetSortOrder(WorldPosition + 2 * Vector3Int.right);
    }
}

[System.Serializable]
public class Bar : LinearSpriteObject, IInteractable
{
    /*static (Color32[] array, int width, int height) _pixelsX;
    static (Color32[] array, int width, int height) _pixelsY;

    public override (Color32[], int, int) GetPixels
    {
        get
        {
            if (Alignment == MapAlignment.XEdge)
            {
                if (_pixelsX == default)
                {
                    _pixelsX.array = Graphics.Instance.BarX[0].texture.GetPixels32();
                    for (int i = 1; i < Graphics.Instance.BarX.Length; i++)
                    {
                        Color32[] array = Graphics.Instance.BarX[i].texture.GetPixels32();
                        for (int j = 0; j < _pixelsX.array.Length; j++)
                        {
                            if (array[j].a > 0)
                            {
                                _pixelsX.array[j] = Color.black;
                            }
                        }
                    }
                    _pixelsX.width = Graphics.Instance.BarX[0].texture.width;
                    _pixelsX.height = Graphics.Instance.BarX[0].texture.height;
                }

                return _pixelsX;
            }
            else
            {
                if (_pixelsY == default)
                {
                    _pixelsY.array = Graphics.Instance.BarY[0].texture.GetPixels32();
                    for (int i = 1; i < Graphics.Instance.BarY.Length; i++)
                    {
                        Color32[] array = Graphics.Instance.BarY[i].texture.GetPixels32();
                        for (int j = 0; j < _pixelsY.array.Length; j++)
                        {
                            if (array[j].a > 0)
                            {
                                _pixelsY.array[j] = Color.black;
                            }
                        }
                    }
                    _pixelsY.width = Graphics.Instance.BarY[0].texture.width;
                    _pixelsY.height = Graphics.Instance.BarY[0].texture.height;
                }

                return _pixelsY;
            }
        }
    }*/

    [JsonProperty]
    protected override string ObjectType { get; } = "Bar";
    public static void PlaceHighlight(SpriteRenderer highlight, Vector3Int position, MapAlignment alignment)
    {
        if (CheckObject(position, alignment))
        {
            highlight.enabled = true;
            highlight.sprite = alignment == MapAlignment.XEdge ? Graphics.Instance.BarX[0] : Graphics.Instance.BarY[0];
            highlight.transform.position = Map.MapCoordinatesToSceneCoordinates(MapAlignment.Center, position);
            highlight.sortingOrder = Graphics.GetSortOrder(position);
        }
        else
            highlight.enabled = false;
    }

    public static void CreateBar(int x, int y, int z, MapAlignment alignment)
    {
        new Bar(alignment, new Vector3Int(x, y, z));
    }

    public static bool CheckObject(Vector3Int position, MapAlignment alignment)
    {
        Vector3Int dimensions = default;

        if (alignment == MapAlignment.XEdge)
            dimensions = ObjectDimensions;
        else
            dimensions = new Vector3Int(ObjectDimensions.y, ObjectDimensions.x, ObjectDimensions.z);


        return Map.Instance.CanPlaceObject(position, dimensions);
    }

    public List<RoomNode> GetInteractionPoints()
    {
        List<RoomNode> interactionPoints = new List<RoomNode>();

        if(Alignment == MapAlignment.XEdge)
        {
            
            int i = 0;
            while (Map.Instance[WorldPosition + Vector3Int.right * i].Occupant is Bar)
            {
                RoomNode roomNode = Map.Instance[WorldPosition + Vector3Int.right * i + 2 * Vector3Int.down];
                if (roomNode.Traversible)
                    interactionPoints.Add(roomNode);
                i++;
            }
            i = 1;
            while (Map.Instance[WorldPosition + Vector3Int.left * i].Occupant is Bar)
            {
                RoomNode roomNode = Map.Instance[WorldPosition + Vector3Int.left * i + 2 * Vector3Int.down];
                if (roomNode.Traversible)
                    interactionPoints.Add(roomNode);
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
                    interactionPoints.Add(roomNode);
                i++;
            }
            i = 1;
            while (Map.Instance[WorldPosition + Vector3Int.down * i].Occupant is Bar)
            {
                RoomNode roomNode = Map.Instance[WorldPosition + Vector3Int.down * i + 2 * Vector3Int.left];
                if (roomNode.Traversible)
                    interactionPoints.Add(roomNode);
                i++;
            }
        }

        return interactionPoints;
    }

    public static new Vector3Int ObjectDimensions { get; } = new Vector3Int(1, 2, 2);

    public override void Destroy()
    {
        AcquireFoodTask.FoodSources.Remove(this);
        base.Destroy();
    }

    protected override void Confirm()
    {
        AcquireFoodTask.FoodSources.Add(this);
        base.Confirm();
    }

    [JsonConstructor]
    public Bar(MapAlignment alignment ,Vector3Int worldPosition)
        : base(2, Graphics.Instance.BarX[0], Graphics.Instance.BarY[0], worldPosition, alignment, "Bar", alignment == MapAlignment.XEdge ? ObjectDimensions : new Vector3Int(2,1,2), true)
    {
        _spriteRenderer[1].sprite = alignment == MapAlignment.XEdge ? Graphics.Instance.BarX[1] : Graphics.Instance.BarY[1];
        _spriteRenderer[1].sortingOrder = Graphics.GetSortOrder(WorldPosition + (alignment == MapAlignment.XEdge ? Vector3Int.down : Vector3Int.left));
    }
}