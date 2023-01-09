using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[System.Serializable]
public class TableSquare : SpriteObject
{

    static bool[,] _pixels;

    [JsonConstructor]
    public TableSquare(Vector3Int worldPosition)
        : base(3, Graphics.Instance.TableSquare[0], worldPosition, "Square Table", ObjectDimensions, true)
    {
        _spriteRenderer[1].sprite = Graphics.Instance.TableSquare[1];
        _spriteRenderer[1].sortingOrder = Graphics.GetSortOrder(WorldPosition + 2 * Vector3Int.up);
        _spriteRenderer[2].sprite = Graphics.Instance.TableSquare[2];
        _spriteRenderer[2].sortingOrder = Graphics.GetSortOrder(WorldPosition + 2 * Vector3Int.right);
    }

    public static new Vector3Int ObjectDimensions { get; } = new Vector3Int(3, 3, 2);

    [JsonIgnore]
    public override IEnumerable<bool[,]> GetMaskPixels
    {
        get
        {
            if (_pixels == default)
            {
                BuildPixelArray(Graphics.Instance.TableSquare, ref _pixels);
            }

            yield return _pixels;
        }
    }

    [JsonProperty]
    protected override string ObjectType { get; } = "TableSquare";
    public static bool CheckObject(Vector3Int position)
    {
        return Map.Instance.CanPlaceObject(position, ObjectDimensions);
    }

    public static void CreateTableSquare(Vector3Int position)
    {
        new TableSquare(position);
    }

    public static void PlaceHighlight(SpriteRenderer highlight, Vector3Int position)
    {
        if (CheckObject(position))
        {
            highlight.enabled = true;
            highlight.sprite = Graphics.Instance.TableSquare[0];
            highlight.transform.position = Map.MapCoordinatesToSceneCoordinates(position);
            highlight.sortingOrder = Graphics.GetSortOrder(position);
        }
        else
            highlight.enabled = false;
    }
}
