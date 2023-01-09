using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[System.Serializable]
public class Bed : SpriteObject, IOccupied
{
    static bool[,] _pixels;

    [JsonConstructor]
    public Bed(Vector3Int worldPosition) :
        base(5, Graphics.Instance.BedSprite[1], worldPosition, "Bed", ObjectDimensions, true)
    {
        StanceLay.LayingObjects.Add(this);
        _spriteRenderer[1].sprite = Graphics.Instance.BedSprite[0];
        _spriteRenderer[1].sortingOrder = Graphics.GetSortOrder(WorldPosition + Vector3Int.up);

        for (int i = 2; i < _spriteRenderer.Length; i++)
        {
            _spriteRenderer[i].sprite = Graphics.Instance.BedSprite[i];
            _spriteRenderer[i].sortingOrder = Graphics.GetSortOrder(WorldPosition + Vector3Int.right * (i - 1));
        }

    }

    public static new Vector3Int ObjectDimensions { get; } = new Vector3Int(4, 2, 1);

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

    [JsonIgnore]
    public Actor Occupant { get; set; }

    [JsonIgnore]
    public bool Occupied => Occupant != null;

    public Pawn Owner { get; private set; }

    [JsonProperty]
    protected override string ObjectType { get; } = "Bed";

    public static bool CheckObject(Vector3Int position)
    {
        return Map.Instance.CanPlaceObject(position, ObjectDimensions);
    }

    public static void CreateBed(Vector3Int position)
    {
        new Bed(position);
    }

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
    public override void Destroy()
    {
        StanceLay.LayingObjects.Remove(this);
        base.Destroy();
    }

    public void Enter(Pawn pawn)
    {
        pawn.transform.Rotate(0, 0, -55);
        pawn.WorldPositionNonDiscrete = WorldPosition + Vector3Int.up;
        Occupant = pawn.Actor;
    }

    public void Exit(Pawn pawn)
    {
        pawn.transform.Rotate(0, 0, 55);
        Occupant = null;
        RoomNode roomNode = GetInteractionPoints()[0];
        pawn.WorldPositionNonDiscrete = roomNode.WorldPosition;
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
}
