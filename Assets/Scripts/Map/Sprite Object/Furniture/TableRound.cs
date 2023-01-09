using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[System.Serializable]
public class TableRound : SpriteObject
{
    static bool[,] _pixels;

    [JsonConstructor]
    public TableRound(Vector3Int worldPosition)
        : base(3, Graphics.Instance.TableRound[0], worldPosition, "Round Table", new Vector3Int(1, 1, 2), true)
    {
        Dimensions = ObjectDimensions;
        _spriteRenderer[1].sprite = Graphics.Instance.TableRound[1];
        _spriteRenderer[1].sortingOrder = Graphics.GetSortOrder(WorldPosition + Vector3Int.up);
        _spriteRenderer[2].sprite = Graphics.Instance.TableRound[2];
        _spriteRenderer[2].sortingOrder = Graphics.GetSortOrder(WorldPosition + Vector3Int.right);
    }

    public static new Vector3Int ObjectDimensions { get; } = new Vector3Int(3, 3, 2);

    [JsonIgnore]
    public new Vector3Int Dimensions { get; }

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

    [JsonProperty]
    protected override string ObjectType { get; } = "TableRound";
    public static bool CheckObject(Vector3Int position)
    {
        return Map.Instance.CanPlaceObject(position, ObjectDimensions);
    }

    public static void CreateTableRound(Vector3Int position)
    {
        new TableRound(position);
    }

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
