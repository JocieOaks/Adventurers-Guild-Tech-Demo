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
            highlight.sprite = Graphics.Instance.BedSprite;
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

    public Pawn Owner { get; private set; }

    public bool Occupied => Occupant != null;

    public Actor Occupant { get; set; }

    [JsonProperty]
    protected override string ObjectType { get; } = "Bed";

    [JsonConstructor]
    public Bed(Vector3Int worldPosition) :
        base(1, Graphics.Instance.BedSprite, worldPosition, "Bed", ObjectDimensions, true)
    {
        StanceLay.LayingObjects.Add(this);
        //StanceSit.SittingObjects.Add(this);
    }

    public override void Destroy()
    {
        StanceLay.LayingObjects.Remove(this);
        //StanceSit.SittingObjects.Remove(this);
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
                    highlight.sprite = Graphics.Instance.ChairPositive;
                    highlight.flipX = false;
                    break;
                case Direction.South:
                    highlight.sprite = Graphics.Instance.ChairNegative;
                    highlight.flipX = true;
                    break;
                case Direction.East:
                    highlight.sprite = Graphics.Instance.ChairPositive;
                    highlight.flipX = true;
                    break;
                case Direction.West:
                    highlight.sprite = Graphics.Instance.ChairNegative;
                    highlight.flipX = false;
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
        : base(1, Graphics.Instance.ChairPositive, Graphics.Instance.ChairNegative, direction, worldPosition, "Chair", ObjectDimensions, true)
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
    [JsonProperty]
    protected override string ObjectType { get; } = "TableRound";
    public static void PlaceHighlight(SpriteRenderer highlight, Vector3Int position)
    {
        if (CheckObject(position))
        {
            highlight.enabled = true;
            highlight.sprite = Graphics.Instance.TableRound;
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
        : base(1, Graphics.Instance.TableRound, worldPosition, "Round Table", new Vector3Int(1,1,2), true)
    {
        Dimensions = ObjectDimensions;
    }
}

[System.Serializable]
public class TableSquare : SpriteObject
{
    [JsonProperty]
    protected override string ObjectType { get; } = "TableSquare";
    public static void PlaceHighlight(SpriteRenderer highlight, Vector3Int position)
    {
        if (CheckObject(position))
        {
            highlight.enabled = true;
            highlight.sprite = Graphics.Instance.TableSquare;
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
        : base(1, Graphics.Instance.TableSquare, worldPosition, "Square Table", ObjectDimensions, true)
    {
    }
}

[System.Serializable]
public class Bar : LinearSpriteObject, IInteractable
{
    [JsonProperty]
    protected override string ObjectType { get; } = "Bar";
    public static void PlaceHighlight(SpriteRenderer highlight, Vector3Int position, MapAlignment alignment)
    {
        if (CheckObject(position, alignment))
        {
            highlight.enabled = true;
            highlight.sprite = alignment == MapAlignment.XEdge ? Graphics.Instance.BarX : Graphics.Instance.BarY;
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
        : base(1, Graphics.Instance.BarX, Graphics.Instance.BarY, worldPosition, alignment, "Bar", alignment == MapAlignment.XEdge ? ObjectDimensions : new Vector3Int(2,1,2), true)
    {
    }
}