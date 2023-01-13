using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[System.Serializable]
public class Bar : LinearSpriteObject, IInteractable
{
    static bool[,] _pixelsX;
    static bool[,] _pixelsY;

    [JsonConstructor]
    public Bar(MapAlignment alignment, Vector3Int worldPosition)
        : base(2, Graphics.Instance.BarX[0], Graphics.Instance.BarY[0], worldPosition, alignment, "Bar", alignment == MapAlignment.XEdge ? ObjectDimensions : new Vector3Int(2, 1, 2), true)
    {
        _spriteRenderer[1].sprite = alignment == MapAlignment.XEdge ? Graphics.Instance.BarX[1] : Graphics.Instance.BarY[1];
        _spriteRenderer[1].sortingOrder = Graphics.GetSortOrder(WorldPosition + (alignment == MapAlignment.XEdge ? Vector3Int.down : Vector3Int.left));
    }

    public static new Vector3Int ObjectDimensions { get; } = new Vector3Int(1, 2, 2);

    [JsonIgnore]
    public override IEnumerable<bool[,]> GetMaskPixels
    {
        get
        {
            if (Alignment == MapAlignment.XEdge)
            {
                if (_pixelsX == default)
                {
                    BuildPixelArray(Graphics.Instance.BarX, ref _pixelsX);
                }

                yield return _pixelsX;
            }
            else
            {
                if (_pixelsY == default)
                {
                    BuildPixelArray(Graphics.Instance.BarY, ref _pixelsY);
                }

                yield return _pixelsY;
            }
        }
    }

    [JsonProperty]
    protected override string ObjectType { get; } = "Bar";
    public static bool CheckObject(Vector3Int position, MapAlignment alignment)
    {
        Vector3Int dimensions = default;

        if (alignment == MapAlignment.XEdge)
            dimensions = ObjectDimensions;
        else
            dimensions = new Vector3Int(ObjectDimensions.y, ObjectDimensions.x, ObjectDimensions.z);


        return Map.Instance.CanPlaceObject(position, dimensions);
    }

    public static void CreateBar(int x, int y, int z, MapAlignment alignment)
    {
        new Bar(alignment, new Vector3Int(x, y, z));
    }

    public static void PlaceHighlight(SpriteRenderer highlight, Vector3Int position, MapAlignment alignment)
    {
        if (CheckObject(position, alignment))
        {
            highlight.enabled = true;
            highlight.sprite = alignment == MapAlignment.XEdge ? Graphics.Instance.BarX[0] : Graphics.Instance.BarY[0];
            highlight.transform.position = Map.MapCoordinatesToSceneCoordinates(position);
            highlight.sortingOrder = Graphics.GetSortOrder(position);
        }
        else
            highlight.enabled = false;
    }
    public override void Destroy()
    {
        AcquireFoodTask.FoodSources.Remove(this);
        base.Destroy();
    }
    List<RoomNode> _interactionPoints;

    public IEnumerable<RoomNode> InteractionPoints
    {
        get
        {
            if (_interactionPoints == null)
            {
                _interactionPoints = new List<RoomNode>();

                if (Alignment == MapAlignment.XEdge)
                {

                    int i = 0;
                    while (Map.Instance[WorldPosition + Vector3Int.right * i].Occupant is Bar)
                    {
                        RoomNode roomNode = Map.Instance[WorldPosition + Vector3Int.right * i + 2 * Vector3Int.down];
                        if (roomNode.Traversible)
                            _interactionPoints.Add(roomNode);
                        i++;
                    }
                    i = 1;
                    while (Map.Instance[WorldPosition + Vector3Int.left * i].Occupant is Bar)
                    {
                        RoomNode roomNode = Map.Instance[WorldPosition + Vector3Int.left * i + 2 * Vector3Int.down];
                        if (roomNode.Traversible)
                            _interactionPoints.Add(roomNode);
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
                            _interactionPoints.Add(roomNode);
                        i++;
                    }
                    i = 1;
                    while (Map.Instance[WorldPosition + Vector3Int.down * i].Occupant is Bar)
                    {
                        RoomNode roomNode = Map.Instance[WorldPosition + Vector3Int.down * i + 2 * Vector3Int.left];
                        if (roomNode.Traversible)
                            _interactionPoints.Add(roomNode);
                        i++;
                    }
                }
            }

            return _interactionPoints;
        }
    }

    protected override void OnMapChanging()
    {
        _interactionPoints = null;
    }

    public void Reserve() { }

    protected override void Confirm()
    {
        AcquireFoodTask.FoodSources.Add(this);
        base.Confirm();
    }
}
