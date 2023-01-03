using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEditor;

public interface IInteractable
{
    public List<RoomNode> GetInteractionPoints();
    public Vector3Int WorldPosition { get; }
}

public interface IOccupied : IInteractable
{
    public bool Occupied => Occupant != null;
    public Actor Occupant { get; set; }

    public void Enter(Pawn pawn);

    public void Exit(Pawn pawn);
}


[JsonConverter(typeof(JsonSubTypes.JsonSubtypes), "ObjectType")]
[JsonSubTypes.JsonSubtypes.KnownSubType(typeof(Bed), "Bed")]
[JsonSubTypes.JsonSubtypes.KnownSubType(typeof(Chair), "Chair")]
[JsonSubTypes.JsonSubtypes.KnownSubType(typeof(Stool), "Stool")]
[JsonSubTypes.JsonSubtypes.KnownSubType(typeof(TableRound), "TableRound")]
[JsonSubTypes.JsonSubtypes.KnownSubType(typeof(TableSquare), "TableSquare")]
[JsonSubTypes.JsonSubtypes.KnownSubType(typeof(Bar), "Bar")]
public abstract class SpriteObject : IDataPersistence
{
    [JsonIgnore]
    protected SpriteRenderer[] _spriteRenderer;
    bool _blocking;

    protected virtual string ObjectType { get; }

    public static Vector3Int ObjectDimensions
    {
        get
        {
            throw new System.AccessViolationException("Should not be trying to access abstract class dimensions.");
        }
    }

    [JsonProperty]
    public Vector3Int WorldPosition { get; }

    [JsonIgnore]
    public Vector3Int Dimensions { get; }

    [JsonIgnore]
    protected Sprite Sprite { get => SpriteRenderer.sprite; 
        set
        {
            Object.Destroy(Collider);
            SpriteRenderer.sprite = value;
            if(value != null)
                Collider = GameObject.AddComponent<PolygonCollider2D>();
        }
    }

    [JsonIgnore]
    public SpriteRenderer SpriteRenderer => _spriteRenderer[0];

    [JsonIgnore]
    protected Transform Transform => _spriteRenderer[0].transform;

    [JsonIgnore]
    protected GameObject GameObject => _spriteRenderer[0].gameObject;

    [JsonIgnore]
    protected PolygonCollider2D Collider { get; private set; }

    public SpriteObject(int spriteCount,Sprite sprite, Vector3Int position, string name, Vector3Int dimensions, bool blocking)
    {
        WorldPosition = position;
        Dimensions = dimensions;
        _blocking = blocking;

        _spriteRenderer = new SpriteRenderer[spriteCount];
        _spriteRenderer[0] = Object.Instantiate(Graphics.Instance.SpriteObject).GetComponent<SpriteRenderer>();
        Transform.position = Map.MapCoordinatesToSceneCoordinates(MapAlignment.Center, position);
        SpriteRenderer.sprite = sprite;
        SpriteRenderer.name = name;
        SpriteRenderer.sortingOrder = Graphics.GetSortOrder(position);

        SpriteRenderer.enabled = GameManager.Instance.IsOnLevel(WorldPosition.z) <= 0;

        Collider = GameObject.AddComponent<PolygonCollider2D>();
        MouseOver mouseOver = GameObject.AddComponent<MouseOver>();
        mouseOver.Set(this);

        if (blocking)
        {
            Map.Instance.StartCoroutine(Block(position, dimensions));
        }

        Graphics.LevelChanging += SetLevel;

        if (this is not WallSprite && this is not StairSprite && this is not Floor)
            DataPersistenceManager.instance.NonMonoDataPersistenceObjects.Add(this);
    }

    IEnumerator Block(Vector3Int position, Vector3Int dimensions)
    {
        yield return new WaitUntil(() => Map.Ready);

        for (int i = 0; i < dimensions.x; i++)
        {
            for (int j = 0; j < dimensions.y; j++)
            {
                Map.Instance[position + i * Vector3Int.right + j * Vector3Int.up].Occupant = this;
            }
        }
    }

    protected virtual void SetLevel()
    {
        int level = GameManager.Instance.IsOnLevel(WorldPosition.z);
        if (level > 0)
        {
            for(int i = 0; i < _spriteRenderer.Length; i++)
                _spriteRenderer[i].enabled = false;
        }
        else
            for (int i = 0; i < _spriteRenderer.Length; i++)
                _spriteRenderer[i].enabled = true;
        
        if(Collider != null)
            Collider.enabled = level == 0;
        
    }

    public virtual void Destroy()
    {
        Graphics.LevelChanging -= SetLevel;
        Graphics.ResetingSprite -= ResetSprite;

        if (_blocking)
        {
            for (int i = 0; i < Dimensions.x; i++)
            {
                for (int j = 0; j < Dimensions.y; j++)
                {
                    Map.Instance[WorldPosition + i * Vector3Int.right + j * Vector3Int.up].Occupant = null;
                }
            }
        }

        Object.Destroy(GameObject);
        DataPersistenceManager.instance.NonMonoDataPersistenceObjects.Remove(this);
    }

    public virtual void Highlight(Color color)
    {
        for (int i = 0; i < _spriteRenderer.Length; i++)
            _spriteRenderer[i].color = color;
        Graphics.ResetingSprite += ResetSprite;
    }

    protected virtual void ResetSprite()
    {
        for (int i = 0; i < _spriteRenderer.Length; i++)
        {
            _spriteRenderer[i].color = Color.white;
        }

        Graphics.ResetingSprite -= ResetSprite;
    }

    public void SaveData(GameData gameData)
    {
        gameData.SpriteObjects.Add(this);
    }

    public void LoadData(GameData gameData)
    {
        
    }

    public class MouseOver : MonoBehaviour
    {
        public SpriteObject SpriteObject { get; private set; }

        public void Set(SpriteObject spriteObject)
        {
            SpriteObject = spriteObject;
        }
    }
}

public abstract class DirectionalSpriteObject : SpriteObject
{
    [JsonProperty]
    public Direction Direction { get; }

    public DirectionalSpriteObject(int spriteCount, Sprite positive, Sprite negative, Direction direction, Vector3Int position, string name, Vector3Int ObjectDimension, bool blocking)
    : base(spriteCount, null, position, name, ObjectDimension, blocking)
    {
        Direction = direction;

        switch (direction)
        {
            case Direction.North:
                Sprite = positive;
                break;
            case Direction.South:
                Sprite = negative;
                SpriteRenderer.flipX = true;
                break;
            case Direction.East:
                Sprite = positive;
                SpriteRenderer.flipX = true;
                break;
            case Direction.West:
                Sprite = negative;
                break;
        }
    }
}

public abstract class LinearSpriteObject : SpriteObject
{
    [JsonProperty]
    public MapAlignment Alignment { get; }

    public LinearSpriteObject(int spriteCount, Sprite xSprite, Sprite ySprite, Vector3Int position, MapAlignment alignment, string name, Vector3Int dimensions, bool blocking) : base(spriteCount, null, position, name, dimensions, blocking )
    {
        Alignment = alignment;
        Sprite = alignment == MapAlignment.XEdge ? xSprite : ySprite;
        Graphics.ConfirmingObject += Confirm;
        Graphics.CheckingLineConstraints += DestroyIfOutOfRange;
    }

    void DestroyIfOutOfRange(int start, int end)
    {
        if (Alignment == MapAlignment.XEdge && (WorldPosition.x < start || WorldPosition.x > end) || Alignment == MapAlignment.YEdge && (WorldPosition.y < start || WorldPosition.y > end))
        {
            Graphics.ConfirmingObject -= Confirm;
            Graphics.CheckingLineConstraints -= DestroyIfOutOfRange;

            Destroy();
        }
    }

    protected virtual void Confirm()
    {
        Graphics.CheckingLineConstraints -= DestroyIfOutOfRange;
        Graphics.ConfirmingObject -= Confirm;
    }
}

public abstract class AreaSpriteObject : SpriteObject
{

    public AreaSpriteObject(int spriteCount, Sprite sprite, Vector3Int position, string name, Vector3Int dimensions, bool blocking) : base(spriteCount, sprite, position, name, dimensions, blocking)
    {
        SpriteRenderer.color = Graphics.Instance.HighlightColor;
        Graphics.ConfirmingObject += Confirm;
        Graphics.CheckingAreaConstraints += DestroyIfOutOfRange;
    }

    protected virtual void DestroyIfOutOfRange(Vector3Int start, Vector3Int end)
    {
        int minX = start.x < end.x ? start.x : end.x;
        int maxX = start.x > end.x ? start.x : end.x;
        int minY = start.y < end.y ? start.y : end.y;
        int maxY = start.y > end.y ? start.y : end.y;

        if (WorldPosition.x < minX || WorldPosition.y < minY || WorldPosition.x > maxX || WorldPosition.y > maxY)
        {
            Graphics.ConfirmingObject -= Confirm;
            Graphics.CheckingAreaConstraints -= DestroyIfOutOfRange;

            Destroy();
        }
    }

    protected virtual void Confirm()
    {
        SpriteRenderer.color = Color.white;
        Graphics.CheckingAreaConstraints -= DestroyIfOutOfRange;
        Graphics.ConfirmingObject -= Confirm;
    }
}