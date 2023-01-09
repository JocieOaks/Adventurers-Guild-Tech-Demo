using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[System.Serializable]
public class Stool : SpriteObject, IOccupied
{
    static bool[,] _pixels;

    [JsonConstructor]
    public Stool(Vector3Int worldPosition)
        : base(1, Graphics.Instance.Stool, worldPosition, "Stool", ObjectDimensions, true)
    {
        StanceSit.SittingObjects.Add(this);
    }

    public static new Vector3Int ObjectDimensions { get; } = new Vector3Int(1, 1, 2);

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
    public Actor Occupant { get; set; }

    [JsonIgnore]
    public bool Occupied => Occupant != null;

    [JsonProperty]
    protected override string ObjectType { get; } = "Stool";
    public static bool CheckObject(Vector3Int position)
    {
        return Map.Instance.CanPlaceObject(position, ObjectDimensions);
    }

    public static void CreateStool(Vector3Int position)
    {
        new Stool(position);
    }

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
    public override void Destroy()
    {
        StanceSit.SittingObjects.Remove(this);
        base.Destroy();
    }

    public void Enter(Pawn pawn)
    {
        pawn.WorldPositionNonDiscrete = WorldPosition + Vector3Int.back;
        Occupant = pawn.Actor;
    }

    public void Exit(Pawn pawn)
    {
        Occupant = null;

        RoomNode roomNode = GetInteractionPoints()[0];
        pawn.WorldPositionNonDiscrete = roomNode.WorldPosition;
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
}
